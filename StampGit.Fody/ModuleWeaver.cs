using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Fody;
using System.IO;
using Mono.Cecil.Rocks;

namespace SourceControlSummary
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        private static string? GetRepoCommitId(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            while (path != null)
            {
                string gitFolderPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitFolderPath))
                {
                    string headFilePath = Path.Combine(gitFolderPath, "HEAD");
                    if (File.Exists(headFilePath))
                    {
                        string headContent = File.ReadAllText(headFilePath).Trim();
                        if (headContent.StartsWith("ref:"))
                        {
                            string branchRef = headContent.Substring(5).Trim();
                            string branchRefFilePath = Path.Combine(gitFolderPath, branchRef.Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(branchRefFilePath))
                            {
                                return File.ReadAllText(branchRefFilePath).Trim();
                            }
                        }
                        else
                        {
                            return headContent;
                        }
                    }
                }
                path = Directory.GetParent(path)?.FullName;
            }

            return null;
        }

        private static string? GetRepoBranchName(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            if (!Directory.Exists(path))
            {
                return null;
            }

            while (path != null)
            {
                string gitFolderPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitFolderPath))
                {
                    string headFilePath = Path.Combine(gitFolderPath, "HEAD");
                    if (File.Exists(headFilePath))
                    {
                        string headContent = File.ReadAllText(headFilePath).Trim();
                        if (headContent.StartsWith("ref:"))
                        {
                            string branchRef = headContent.Substring(5).Trim();
                            return branchRef.Split('/').Last(); // 返回分支名称
                        }
                    }
                }

                path = Directory.GetParent(path)?.FullName;
            }

            return null;
        }

        public override void Execute()
        {
            // 获取 Git 提交 ID 和分支名称
            string? commitID = GetRepoCommitId(SolutionDirectoryPath);
            string? branchName = GetRepoBranchName(SolutionDirectoryPath);

            if (commitID == null && branchName == null)
            {
                WriteMessage("Unable to retrieve Git commit and branch information.", MessageImportance.High);
                return;
            }

            foreach (var type in ModuleDefinition.Types)
            {
                foreach (var property in type.Properties)
                {
                    // 检查属性是否包含 GitCommitAttribute 或 GitBranchAttribute
                    var customAttributes = property.CustomAttributes;

                    var gitCommitAttribute = customAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == typeof(GitCommitAttribute).FullName);
                    var gitBranchAttribute = customAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == typeof(GitBranchAttribute).FullName);

                    // 如果找到一个自定义特性，修改 getter
                    if (gitCommitAttribute != null && commitID != null)
                    {
                        WriteMessage($"Modifying property '{property.Name}' to return commit ID: {commitID}", MessageImportance.High);
                        ModifyPropertyGetter(property, commitID);
                    }

                    if (gitBranchAttribute != null && branchName != null)
                    {
                        WriteMessage($"Modifying property '{property.Name}' to return branch name: {branchName}", MessageImportance.High);
                        ModifyPropertyGetter(property, branchName);
                    }
                }
            }
        }

        private void ModifyPropertyGetter(PropertyDefinition property, string returnValue)
        {
            // 确保属性有 getter
            var getter = property.GetMethod;
            if (getter == null)
            {
                WriteError($"Property '{property.Name}' of type '{property.PropertyType.FullName}' has no getter.");
                return;
            }

            // 清空现有的 getter 方法体
            getter.Body = new MethodBody(getter);
            var ilProcessor = getter.Body.GetILProcessor();

            // 注入 IL 指令来返回指定值
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ldstr, returnValue)); // 加载字符串值到栈
            ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));               // 返回栈上的值

            getter.Body.OptimizeMacros();
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }
    }
}
