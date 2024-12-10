using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;
using Mono.Cecil;
using SMAPI_Installation;
using SMAPIStardewGame;
using StardewModdingAPI;
using System.Reflection;
using CustomAttribute = Mono.Cecil.CustomAttribute;

namespace SMAPIStardewValley
{
    public class dnlibPatchPatch_StardewValley
    {
        public static void dnlibPatch()
        {


            AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve.HandleAssemblyResolve;
            string externalFilesDir = MainActivity.GetPrivateStoragePath();

            // 获取上一级目录
            string parentDir = Path.GetDirectoryName(externalFilesDir);

            string modsDir = Path.Combine(parentDir, "Mods");
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
            }
            HarmonyPatch();
         
        

      
      
       //    ProcessItemMigrator(modsDir, "StardewValleyExpanded.dll");

         //666   ModifyRecursiveIterateLocation(modsDir, "StardewValleyExpanded.dll");

          ClearMethodBody(modsDir, "StardewValleyExpanded.dll", "StardewValleyExpanded.EndNexusMusic", "Hook");
            ModifyXmlElementToXmlArrayWithMonoMod(modsDir, "FarmTypeManager.ModEntry", "Items");

        }

        public static void ModifyXmlElementToXmlArrayWithMonoMod(string modsDir, string className, string fieldName)
        {



            string[] dllFiles = Directory.GetFiles(modsDir, "FarmTypeManager.dll", SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 使用 MonoMod 加载程序集
                    Console.WriteLine($"正在加载程序集: {modDllPath}");
                    var module = ModuleDefinition.ReadModule(modDllPath);

                    // 找到指定的类
                    var type = module.Types.FirstOrDefault(t => t.FullName == className);
                    if (type == null)
                    {
                        Console.WriteLine($"未找到类 {className}！");
                        return;
                    }
                    Console.WriteLine($"已找到类 {className}");

                    // 如果是嵌套类，考虑查找其嵌套类型
                    var nestedType = type.NestedTypes.FirstOrDefault(nt => nt.FullName == $"{className}/BuriedItems");
                    if (nestedType == null)
                    {
                        Console.WriteLine($"未找到嵌套类 {className}+BuriedItems！");
                        return;
                    }
                    Console.WriteLine($"已找到嵌套类 {className}+BuriedItems");

                    // 找到指定的字段
                    var field = nestedType.Fields.FirstOrDefault(f => f.Name == fieldName);
                    if (field == null)
                    {
                        Console.WriteLine($"未找到字段 {fieldName}！");
                        return;
                    }
                    Console.WriteLine($"已找到字段 {fieldName}");

                    // 修改 XML 特性
                    var attributes = field.CustomAttributes;

                    // 查找并移除原来的 XmlElement 属性
                    var xmlElementAttr = attributes.FirstOrDefault(attr => attr.AttributeType.FullName == "System.Xml.Serialization.XmlElementAttribute");
                    if (xmlElementAttr != null)
                    {
                        attributes.Remove(xmlElementAttr);
                        Console.WriteLine($"已移除字段 {fieldName} 上的 XmlElement 属性");
                    }
                    else
                    {
                        Console.WriteLine($"字段 {fieldName} 上没有找到 XmlElement 属性，跳过移除");
                    }

                    // 创建并添加新的 XmlArray 和 XmlArrayItem 属性
                    var xmlArrayCtor = module.ImportReference(typeof(System.Xml.Serialization.XmlArrayAttribute).GetConstructor(new Type[] { typeof(string) }));
                    var xmlArrayItemCtor = module.ImportReference(typeof(System.Xml.Serialization.XmlArrayItemAttribute).GetConstructor(new Type[] { typeof(string) }));

                    field.CustomAttributes.Add(new CustomAttribute(xmlArrayCtor)
                    {
                        ConstructorArguments = { new CustomAttributeArgument(module.ImportReference(typeof(string)), "Items") }
                    });
                    Console.WriteLine($"已添加 XmlArray 属性，参数: 'Items'");

                    field.CustomAttributes.Add(new CustomAttribute(xmlArrayItemCtor)
                    {
                        ConstructorArguments = { new CustomAttributeArgument(module.ImportReference(typeof(string)), "Item") }
                    });
                    Console.WriteLine($"已添加 XmlArrayItem 属性，参数: 'Item'");

                    // 保存修改后的程序集并命名为原来的名称
                    string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));

         

                    module.Write(modifiedAssemblyPath);
                    Console.WriteLine($"修改后的程序集已保存为: {modifiedAssemblyPath}");

                    // 删除原文件并覆盖为修改后的文件
                    File.Delete(modDllPath);  // 删除原文件
                    File.Move(modifiedAssemblyPath, modDllPath);  // 将修改后的文件重命名为原文件名
                    Console.WriteLine($"已删除原始程序集并覆盖为修改后的版本：{modDllPath}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"修改程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }

        public static void HarmonyPatch()
        {
            string externalFilesDir = MainActivity.GetPrivateStoragePath();

            // 获取上一级目录
            string parentDir = Path.GetDirectoryName(externalFilesDir);

            // 遍历 Mods 文件夹寻找 FarmTypeManager.dll
            string modsDir = Path.Combine(parentDir, "Mods");
            string[] dllFiles = Directory.GetFiles(modsDir, "FarmTypeManager.dll", SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 加载程序集
                    ModuleDefMD module = ModuleDefMD.Load(modDllPath);

                    // 输出所有类型，帮助检查全名
                    foreach (var type in module.Types)
                    {
                        if (type.FullName == "FarmTypeManager.ModEntry")
                        {
                            foreach (var nestedType in type.NestedTypes)
                            {
                                if (nestedType.FullName == "FarmTypeManager.ModEntry/HarmonyPatch_OptimizeMonsterCode")
                                {
                                    // 获取 ApplyPatch 方法
                                    MethodDef applyPatchMethod = nestedType.Methods.FirstOrDefault(m => m.Name == "ApplyPatch");

                                    if (applyPatchMethod != null)
                                    {
                                        // 清除方法体的所有内容
                                        applyPatchMethod.Body = new CilBody();
                                        applyPatchMethod.Body.Variables.Clear();
                                        applyPatchMethod.Body.ExceptionHandlers.Clear();
                                        applyPatchMethod.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));

                                        // 保存修改后的程序集
                                        string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                        module.Write(modifiedAssemblyPath);

                                        // 删除原文件并覆盖
                                        File.Delete(modDllPath);
                                        File.Move(modifiedAssemblyPath, modDllPath);

                                      //  Console.WriteLine("修改后的程序集已保存至: " + modDllPath);
                                    }
                                    else
                                    {
                                      //  Console.WriteLine("未找到 ApplyPatch 方法！");
                                    }

                                    return;
                                }
                            }
                        }
                    }

                    Console.WriteLine("未找到 HarmonyPatch_OptimizeMonsterCode 类！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }
    


private static void ClearMethodBody(string modsDir, string dllFileName, string targetTypeFullName, string methodName)
        {
            string[] dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 加载程序集
                    ModuleDefMD module = ModuleDefMD.Load(modDllPath);

                    foreach (var type in module.Types)
                    {
                        if (type.FullName == targetTypeFullName)
                        {
                            // 查找指定方法
                            var method = type.Methods.FirstOrDefault(m => m.Name == methodName);
                            if (method != null)
                            {
                                // 清空方法体
                                method.Body = new CilBody(); // 这将清空方法的所有指令

                                // 更新并保存修改后的程序集
                                string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                module.Write(modifiedAssemblyPath);

                                // 删除原文件并替换为修改后的文件
                                File.Delete(modDllPath);
                                File.Move(modifiedAssemblyPath, modDllPath);

                               // Console.WriteLine($"方法 {methodName} 的内容已被清空，并保存至: {modDllPath}");
                                return;
                            }
                            else
                            {
                                Console.WriteLine($"未找到方法 {methodName}！");
                            }
                        }
                    }

                    Console.WriteLine($"未找到类 {targetTypeFullName}！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }
        /// <summary>
        /// 替换补丁方法调用
        /// </summary>

        /// <summary>
        /// 替换补丁方法调用
        /// </summary>
        private static void ReplacePatchMethod(ModuleDefMD module, Instruction oldInstruction, string newPatchMethod)
        {
            // 查找新补丁方法
            var newMethod = module.Types.SelectMany(t => t.Methods)
                                         .FirstOrDefault(m => m.Name == newPatchMethod);

            if (newMethod == null)
            {
                Console.WriteLine($"无法找到新补丁方法: {newPatchMethod}");
                return;
            }

            // 替换旧补丁方法调用的地方
            oldInstruction.Operand = newMethod;
            Console.WriteLine($"替换了旧补丁方法调用: {oldInstruction.Operand} -> {newMethod.FullName}");
        }
        private static void ProcessAssembly(string modsDir, string dllFileName, string targetTypeFullName, string targetNestedTypeFullName, params string[] fieldsToRemove)
        {
            string[] dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 加载程序集
                    ModuleDefMD module = ModuleDefMD.Load(modDllPath);

                    foreach (var type in module.Types)
                    {
                        if (type.FullName == targetTypeFullName)
                        {
                            foreach (var nestedType in type.NestedTypes)
                            {
                                if (nestedType.FullName == targetNestedTypeFullName)
                                {
                                    // 移除指定的字段
                                    foreach (var field in nestedType.Fields.ToList())
                                    {
                                        // 使用 LINQ 来比较字段名
                                        if (fieldsToRemove.Any(fieldName => field.Name == fieldName))
                                        {
                                            nestedType.Fields.Remove(field);
                                        }
                                    }

                                    // 更新并保存修改后的程序集
                                    string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                    module.Write(modifiedAssemblyPath);

                                    // 删除原文件并替换为修改后的文件
                                    File.Delete(modDllPath);
                                    File.Move(modifiedAssemblyPath, modDllPath);

                                    // Console.WriteLine($"修改后的程序集已保存至: {modDllPath}");
                                    return;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"未找到 {targetNestedTypeFullName} 类！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }
        private static void ProcessAssembly1(string modsDir, string dllFileName, string targetTypeFullName, string targetNestedTypeFullName, params string[] fieldsToRemove)
        {
            string[] dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 加载程序集
                    ModuleDefMD module = ModuleDefMD.Load(modDllPath);
                    bool modified = false;

                    foreach (var type in module.Types)
                    {
                        if (type.FullName == targetTypeFullName)
                        {
                            foreach (var nestedType in type.NestedTypes)
                            {
                                if (nestedType.FullName == targetNestedTypeFullName)
                                {
                                    //Console.WriteLine($"正在处理类型: {targetNestedTypeFullName}");

                                    // 移除指定的字段
                                    var utf8FieldsToRemove = fieldsToRemove.Select(f => new dnlib.DotNet.UTF8String(f)).ToArray();
                                    ReadOnlySpan<dnlib.DotNet.UTF8String> span = new ReadOnlySpan<dnlib.DotNet.UTF8String>(utf8FieldsToRemove);

                                    // 遍历字段和属性并移除
                                    var fieldsToDelete = new List<FieldDef>();
                                    var propertiesToDelete = new List<PropertyDef>();

                                    foreach (var field in nestedType.Fields)
                                    {
                                        if (span.Contains(field.Name))
                                        {
                                            fieldsToDelete.Add(field);
                                        }
                                    }

                                    foreach (var property in nestedType.Properties)
                                    {
                                        if (span.Contains(property.Name))
                                        {
                                            propertiesToDelete.Add(property);
                                        }
                                    }

                                    // 移除字段
                                    foreach (var field in fieldsToDelete)
                                    {
                                        nestedType.Fields.Remove(field);
                                        Console.WriteLine($"已移除字段: {field.Name}");
                                        modified = true;
                                    }

                                    // 移除属性
                                    foreach (var property in propertiesToDelete)
                                    {
                                        nestedType.Properties.Remove(property);
                                        Console.WriteLine($"已移除属性: {property.Name}");
                                        modified = true;
                                    }

                                    // 如果有修改，保存程序集
                                    if (modified)
                                    {
                                        string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                        module.Write(modifiedAssemblyPath);

                                        // 删除原文件并替换为修改后的文件
                                        File.Delete(modDllPath);
                                        File.Move(modifiedAssemblyPath, modDllPath);
                                      //  Console.WriteLine($"修改后的程序集已保存至: {modDllPath}");
                                    }
                                    else
                                    {
                                     //   Console.WriteLine("未找到需要移除的字段或属性。");
                                    }
                                    return;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"未找到类型 {targetNestedTypeFullName}！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }


        public static void ModifyRecursiveIterateLocation(string modsDir, string dllFileName)
{
    string[] dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

    foreach (string modDllPath in dllFiles)
    {
        try
        {
        //    Console.WriteLine($"正在处理程序集: {modDllPath}");
            ModuleDefMD module = ModuleDefMD.Load(modDllPath);

            foreach (var type in module.Types)
            {
                if (type.FullName == "SpaceShared.SpaceUtility")
                {
                    foreach (var method in type.Methods)
                    {
                        // 检查方法名和参数数量
                        if (method.Name == "_recursiveIterateLocation" && method.Parameters.Count == 2)
                        {
                                    // 进一步检查参数类型是否匹配
                                    if (method.Name == "_recursiveIterateLocation" && method.Parameters.Count == 2 &&
                                      method.Parameters[0].Type.FullName == "StardewValley.GameLocation" &&
                                      method.Parameters[1].Type.FullName == "System.Func`2<StardewValley.Item,StardewValley.Item>")
                                    {
                                //        Console.WriteLine("找到匹配参数的方法 '_recursiveIterateLocation'。");

                                // 清空方法体
                                method.Body.Instructions.Clear();
                                method.Body.ExceptionHandlers.Clear(); // 清空异常处理
                                method.Body.Variables.Clear(); // 清空局部变量

                                method.Body.Instructions.Add(Instruction.Create(OpCodes.Ret)); // 确保方法以返回结束
                               // Console.WriteLine("已清空方法体并添加返回指令。");

                                // 保存修改后的程序集
                                string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                var options = new ModuleWriterOptions(module);
                                options.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;
                                module.Write(modifiedAssemblyPath);

                                File.Delete(modDllPath);
                                File.Move(modifiedAssemblyPath, modDllPath);

                             //   Console.WriteLine($"修改后的程序集已保存至: {modDllPath}");
                                return;
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"处理程序集 '{modDllPath}' 时出错: {ex.Message}");
        }
    }
  //  Console.WriteLine("未找到具有指定参数的匹配 '_recursiveIterateLocation' 方法。");
}

        // 处理 StardewValleyExpanded.dll 中的 ItemMigrator.FixCrop 方法
        private static void ProcessItemMigrator(string modsDir, string dllFileName)
        {
            string[] dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (string modDllPath in dllFiles)
            {
                try
                {
                    // 加载程序集
                    ModuleDefMD module = ModuleDefMD.Load(modDllPath);

                    // 查找 ItemMigrator 类型和 FixCrop 方法
                    foreach (var type in module.Types)
                    {
                        if (type.FullName == "JsonAssets.ItemMigrator")
                        {
                            foreach (var method in type.Methods)
                            {
                                if (method.Name == "FixCrop")
                                {
                                    // 修改 FixCrop 方法的 IL 代码
                                    var body = method.Body;
                                    var instructions = body.Instructions;

                                    // 在 IL 代码中找到 data，并修改为 cropData
                                    foreach (var instruction in instructions)
                                    {
                                        if (instruction.OpCode == OpCodes.Ldtoken &&
                                            instruction.Operand is FieldDef field &&
                                            field.Name == "data") // 找到字段 data
                                        {
                                            // 查找并修改字段名称
                                            foreach (var instruction2 in instructions)
                                            {
                                                if (instruction2.OpCode == OpCodes.Newobj)
                                                {
                                                    // 修改 KeyValuePair<string, CropData> data 为 KeyValuePair<string, CropData> cropData
                                                    if (instruction2.Operand is MethodDef methodDef &&
                                                        methodDef.Name == ".ctor")
                                                    {
                                                        // 替换名称
                                                        methodDef.Name = "cropData";
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    // 保存修改后的程序集
                                    string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(modDllPath), "Modified_" + Path.GetFileName(modDllPath));
                                    module.Write(modifiedAssemblyPath);

                                    // 删除原文件并替换为修改后的文件
                                    File.Delete(modDllPath);
                                    File.Move(modifiedAssemblyPath, modDllPath);

                                    //Console.WriteLine($"修改后的程序集已保存至: {modDllPath}");
                                    return;
                                }
                            }
                        }
                    }

                    Console.WriteLine($"未找到 {dllFileName} 中的 ItemMigrator.FixCrop 方法！");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"处理程序集 '{modDllPath}' 时发生错误: {ex.Message}");
                }
            }
        }
       
    }
}
