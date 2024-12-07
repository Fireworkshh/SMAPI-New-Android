using dnlib.DotNet.Emit;
using dnlib.DotNet;
using System;
using System.IO;
using System.Linq;
using SMAPI_Installation;
using StardewModdingAPI;

namespace SMAPIStardewValley
{
    public class HarmonyPatch_OptimizeMonsterCode
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
            var dllFiles = Directory.GetFiles(modsDir, "FarmTypeManager.dll", SearchOption.AllDirectories);

            foreach (var modDllPath in dllFiles)
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

                                        Console.WriteLine("修改后的程序集已保存至: " + modDllPath);
                                    }
                                    else
                                    {
                                        Console.WriteLine("未找到 ApplyPatch 方法！");
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
    }
}
