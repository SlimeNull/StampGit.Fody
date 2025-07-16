using System.Linq;
using System.Collections.Generic;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Fody;
using System.IO;
using Mono.Cecil.Rocks;
using System;
using MetaStamp.Internal;
using System.Runtime.InteropServices;

namespace MetaStamp
{
    public class ModuleWeaver : BaseModuleWeaver
    {
        private void ModifyPropertyGetter(PropertyDefinition property, Action<ILProcessor> procedure)
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

            procedure.Invoke(ilProcessor);

            getter.Body.OptimizeMacros();
        }

        private void ModifyPropertyGetter(
            string attributeFullName,
            Action<TypeReference, ILProcessor> procedure)
        {
            foreach (var type in ModuleDefinition.Types)
            {
                foreach (var property in type.Properties)
                {
                    // 检查属性是否包含 GitCommitAttribute 或 GitBranchAttribute
                    var customAttributes = property.CustomAttributes;
                    var matchedAttribute = customAttributes.FirstOrDefault(attr => attr.AttributeType.FullName == attributeFullName);

                    // 如果找到一个自定义特性，修改 getter
                    if (matchedAttribute != null)
                    {
                        ModifyPropertyGetter(property, ilProcessor => procedure.Invoke(property.PropertyType, ilProcessor));
                    }
                }
            }
        }

        private void ModifyPropertyGetter(
            string attributeFullName,
            object? returnValue)
        {
            ModifyPropertyGetter(attributeFullName, (type, ilProcessor) =>
            {
                object targetType = Convert.ChangeType(returnValue, Type.GetType(type.FullName));

                Action<ILProcessor>? procedure = returnValue switch
                {
                    string strValue => ilProcessor =>
                    {
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldstr, strValue));
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                    }
                    ,

                    int i32Value => ilProcessor =>
                    {
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, i32Value));
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                    }
                    ,

                    Enum enumValue => ilProcessor =>
                    {
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldc_I4, (int)returnValue));
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                    }
                    ,

                    null => ilProcessor =>
                    {
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ldnull));
                        ilProcessor.Append(ilProcessor.Create(OpCodes.Ret));
                    }
                    ,

                    _ => null,
                };

                if (procedure is null)
                {
                    return;
                }

                procedure.Invoke(ilProcessor);
            });
        }

        public override void Execute()
        {
            string? commitID = GitUtils.GetRepoCommitId(SolutionDirectoryPath);
            string? branchName = GitUtils.GetRepoBranchName(SolutionDirectoryPath);

            ModifyPropertyGetter("MetaStamp.GitCommitIDAttribute", commitID);
            ModifyPropertyGetter("MetaStamp.GitBranchAttribute", branchName);
            ModifyPropertyGetter("MetaStamp.BuildPlatformID", Environment.OSVersion.Platform);
            ModifyPropertyGetter("MetaStamp.BuildOperationSystem", Environment.OSVersion.VersionString);
            ModifyPropertyGetter("MetaStamp.BuildDateTime", DateTime.Now.ToString());
        }

        public override IEnumerable<string> GetAssembliesForScanning()
        {
            yield break;
        }
    }
}
