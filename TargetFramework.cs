using Mono.Cecil;
using StardewValley;
using System;
using System.IO;
using System.Linq;


public class TargetFramework
{
    public static void TargetFrameworkPatch()
    {
        string directoryPath = "/storage/emulated/0/Android/data/app.SMAPIStardew/Mods"; 
        string newTargetFramework = "net8.0"; 
        string rootDirectory = Path.Combine("/storage/emulated/0/Android/data/app.SMAPIStardew/files/smapi-internal");

     
        var assemblyResolver = new DefaultAssemblyResolver();
        assemblyResolver.AddSearchDirectory(rootDirectory); // 添加程序集目录，确保加载相关的 DLL 文件

        // 遍历文件夹及所有子文件夹，查找所有的 .dll 文件
        var assemblyFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);

        foreach (var assemblyPath in assemblyFiles)
        {
            try
            {
                // 加载程序集并传递给解析器
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver });

                // 检查目标框架是否需要更新
                if (IsTargetFrameworkUpdated(assemblyDefinition, newTargetFramework))
                {
                  //  Console.WriteLine($"程序集 '{assemblyPath}' 已是目标框架 '{newTargetFramework}'。");
                    continue; // 如果目标框架已经是期望的版本，跳过
                }
                if (IsStardewValleyAssembly(assemblyDefinition))
                {
                //    Console.WriteLine($"程序集 '{assemblyPath}' 是 StardewValley 程序集。");
                    continue; // 跳过 StardewValley 程序集的修改
                }

                // 修改 Stardew Valley 的引用
                ModifyStardewReference(assemblyDefinition);

                // 更新目标框架和引用
                UpdateTargetFramework(assemblyDefinition, newTargetFramework);
                UpdateReferences(assemblyDefinition);

                // 保存修改后的程序集
                string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Modified_" + Path.GetFileName(assemblyPath));
                assemblyDefinition.Write(modifiedAssemblyPath);

            //    Console.WriteLine($"程序集 '{assemblyPath}' 已成功修改并保存为 '{modifiedAssemblyPath}'");

                // 删除原文件并覆盖
                if (File.Exists(assemblyPath))
                {
                    File.Delete(assemblyPath);
             
                }

                // 将修改后的程序集重命名为原文件名
                File.Move(modifiedAssemblyPath, assemblyPath);
             
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理程序集 '{assemblyPath}' 时发生错误: {ex.Message}");
            }
        }
    }

    // 检查目标框架是否已经是指定的版本，避免重复修改
    static bool IsTargetFrameworkUpdated(AssemblyDefinition assemblyDefinition, string targetFramework)
    {
        var metadata = assemblyDefinition.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TargetFrameworkAttribute");
        if (metadata != null)
        {
            var currentFramework = metadata.ConstructorArguments[0].Value as string;
            return currentFramework == targetFramework;
        }
        return false;
    }

    // 更新程序集的目标框架
    static void UpdateTargetFramework(AssemblyDefinition assemblyDefinition, string targetFramework)
    {
        var metadata = assemblyDefinition.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TargetFrameworkAttribute");
        if (metadata != null)
        {
            // 如果目标框架已经是我们要设置的版本，则跳过
            var currentFramework = metadata.ConstructorArguments[0].Value as string;
            if (currentFramework == targetFramework)
            {
              
                return;
            }
            // 更新现有的 TargetFramework 属性
            metadata.ConstructorArguments[0] = new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, targetFramework);
      //      Console.WriteLine($"更新目标框架为 '{targetFramework}'");
        }
        else
        {
            // 如果没有找到目标框架属性，则添加新的
            var targetFrameworkAttribute = new CustomAttribute(
                assemblyDefinition.MainModule.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })
                )
            );
            targetFrameworkAttribute.ConstructorArguments.Add(new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, targetFramework));
            assemblyDefinition.CustomAttributes.Add(targetFrameworkAttribute);
         
        }
    }

    // 更新程序集引用的版本
    // 更新程序集引用的版本
    static void UpdateReferences(AssemblyDefinition assemblyDefinition)
    {
        // 遍历所有程序集引用并更新它们的版本
        foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences)
        {
            // 如果程序集名称以 System 或 Microsoft 开头，假设它们是 .NET 运行时相关的引用
            if (reference.Name.StartsWith("System") || reference.Name.StartsWith("Microsoft"))
            {
                // 检查引用版本是否已经是 8.0.0.0
                if (reference.Version != new Version(8, 0, 0, 0))
                {
                    reference.Version = new Version(8, 0, 0, 0);
                    //Console.WriteLine($"更新 {reference.Name} 为版本 8.0.0.0");
                }
            }
            else if (reference.Name.Equals("0Harmony", StringComparison.OrdinalIgnoreCase))
            {
                // 修改引用名称为 Harmony
                reference.Name = "Harmony";
             //   Console.WriteLine("将引用 '0Harmony' 修改为 'Harmony'");
            }
            else
            {
                // 如果是其他引用，可以在这里添加更多逻辑进行更新
           //     Console.WriteLine($"引用 {reference.Name} 没有被更新。");
            }
        }
    }
    
    static bool IsStardewValleyAssembly(AssemblyDefinition assemblyDefinition)
    {
        // 检查程序集名称是否为 StardewValley 或者程序集是否已经是正确的版本
        return assemblyDefinition.Name.Name.Equals("StardewValley", StringComparison.OrdinalIgnoreCase);
    }

    // 修改 Stardew Valley.dll 的引用为 StardewValley.dll
    static void ModifyStardewReference(AssemblyDefinition assemblyDefinition)
    {
        // 查找并修改所有引用
        foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences)
        {
            if (reference.Name.Equals("Stardew\u0020Valley", StringComparison.OrdinalIgnoreCase))
            {
                reference.Name = "StardewValley";
               // Console.WriteLine($"将引用 'Stardew Valley' 修改为 'StardewValley'");
            }
        }
    }
}