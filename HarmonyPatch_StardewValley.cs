using dnlib.DotNet;
using dnlib.DotNet.Emit;
using System;
using System.IO;
using System.Linq;
using SMAPI_Installation;
using StardewModdingAPI;
using StardewValley.Objects;
using StardewValley;
using dnlib.DotNet.Writer;
using HarmonyLib;

namespace SMAPIStardewValley
{
    public class HarmonyPatch_StardewValley
    {
        public static void HarmonyPatch()
        {
            string externalFilesDir = GameMainActivity.externalFilesDir;

            // 获取上一级目录
            string parentDir = Path.GetDirectoryName(externalFilesDir);

            string modsDir = Path.Combine(parentDir, "Mods");
            if (!Directory.Exists(modsDir))
            {
                Directory.CreateDirectory(modsDir);
            }

            // 处理 FarmTypeManager.dll
            ProcessAssembly(modsDir, "FarmTypeManager.dll", "FarmTypeManager.ModEntry", "FarmTypeManager.ModEntry/HarmonyPatch_OptimizeMonsterCode", "ApplyPatch");

            // 处理 StardewValleyExpanded.dll
            ProcessAssembly1(modsDir, "StardewValleyExpanded.dll", "StardewValleyExpanded.ClintVolumeControl", "StardewValleyExpanded.ClintVolumeControl/CueWrapper", "Pitch", "Volume", "IsPitchBeingControlledByRPC");
          //666  UpdatePatchesInAssemblies(modsDir,"StardewValleyExpanded.dll", "doFishSpecificWaterColoring", "FishPond_doFishSpecificWaterColoring");
            // 修改 ItemMigrator.FixCrop 方法
            ProcessItemMigrator(modsDir, "StardewValleyExpanded.dll");
            ModifyRecursiveIterateLocation(modsDir, "StardewValleyExpanded.dll");
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
            var dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (var modDllPath in dllFiles)
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
            var dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (var modDllPath in dllFiles)
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
                                    Console.WriteLine($"正在处理类型: {targetNestedTypeFullName}");

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
    var dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

    foreach (var modDllPath in dllFiles)
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
            var dllFiles = Directory.GetFiles(modsDir, dllFileName, SearchOption.AllDirectories);

            foreach (var modDllPath in dllFiles)
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