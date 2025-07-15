using Fody;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Collections.Generic;
using System.Reflection;

namespace StampGit.Fody
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        private static string? GetRepoCommitId(string? path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return null;
            }

            // 检查给定路径是否存在
            if (!Directory.Exists(path))
            {
                return null;
            }

            // 循环递归查找 .git 文件夹
            while (path != null)
            {
                string gitFolderPath = Path.Combine(path, ".git");
                if (Directory.Exists(gitFolderPath))
                {
                    string headFilePath = Path.Combine(gitFolderPath, "HEAD");
                    if (File.Exists(headFilePath))
                    {
                        // 读取 HEAD 文件内容
                        string headContent = File.ReadAllText(headFilePath).Trim();
                        if (headContent.StartsWith("ref:"))
                        {
                            // HEAD 指向某个分支
                            string branchRef = headContent.Substring(5).Trim(); // 去掉 "ref: "
                            string branchRefFilePath = Path.Combine(gitFolderPath, branchRef.Replace("/", Path.DirectorySeparatorChar.ToString()));
                            if (File.Exists(branchRefFilePath))
                            {
                                // 返回分支引用中的提交 ID
                                return File.ReadAllText(branchRefFilePath).Trim();
                            }
                        }
                        else
                        {
                            // HEAD 文件本身就是提交 ID
                            return headContent;
                        }
                    }
                }
                // 获取父目录
                path = Directory.GetParent(path)?.FullName;
            }

            // 如果未找到 .git 文件夹，则返回 null
            return null;
        }

        public override void Execute()
        {
            if (GetRepoCommitId(ProjectFilePath) is not { } commitID)
            {
                return;
            }

            var commitStampAttribute = ModuleDefinition.Assembly.CustomAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == "StampGit.Fody.CommitStampAttribute");
            if (commitStampAttribute is null)
            {
                commitStampAttribute = new CustomAttribute()
            }

            commitStampAttribute.Properties.Add(
                new CustomAttributeNamedArgument("ID", new CustomAttributeArgument(ModuleDefinition.TypeSystem.String, commitID)));

        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }

        private TypeDefinition GetAssemblyInformationalVersionAttributeTypeInfo()
        {
            var msCoreLib = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("mscorlib", null));
            var msCoreAttribute = msCoreLib.MainModule.Types.FirstOrDefault(x => x.Name == "AssemblyInformationalVersionAttribute");
            if (msCoreAttribute != null)
            {
                return msCoreAttribute;
            }
            var systemRuntime = ModuleDefinition.AssemblyResolver.Resolve(new AssemblyNameReference("System.Runtime", null));
            return systemRuntime.MainModule.Types.First(x => x.Name == "AssemblyInformationalVersionAttribute");
        }

        private static CustomAttribute GetAssemblyInformationalVersionAttribute(Collection<CustomAttribute> customAttributes)
        {
            var customAttribute = customAttributes.FirstOrDefault(x => x.AttributeType.Name == "AssemblyInformationalVersionAttribute");
            return customAttribute;
        }
    }
}
