using dnlib.DotNet.Emit;
using dnlib.DotNet;
using Mono.Cecil;
using StardewValley;
using System;
using System.IO;
using System.Linq;


public class CorrectedTargetFramework
{
    public static void TargetFrameworkPatch()
    {
        string directoryPath = "/storage/emulated/0/Android/data/app.SMAPIStardew/Mods"; 
        string newTargetFramework = "net8.0"; 
        string rootDirectory = Path.Combine("/storage/emulated/0/Android/data/app.SMAPIStardew/files/smapi-internal");

     
        var assemblyResolver = new DefaultAssemblyResolver();
        assemblyResolver.AddSearchDirectory(rootDirectory);

      
        var assemblyFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);

        foreach (var assemblyPath in assemblyFiles)
        {
            try
            {
             
                var assemblyDefinition = AssemblyDefinition.ReadAssembly(assemblyPath, new ReaderParameters { AssemblyResolver = assemblyResolver });

               
                if (IsTargetFrameworkUpdated(assemblyDefinition, newTargetFramework))
                {
               
                    continue; 
                }
                if (IsStardewValleyAssembly(assemblyDefinition))
                {
                
                    continue; 
                }

               
                ModifyStardewReference(assemblyDefinition);

              
                UpdateTargetFramework(assemblyDefinition, newTargetFramework);
                UpdateReferences(assemblyDefinition);

             
                string modifiedAssemblyPath = Path.Combine(Path.GetDirectoryName(assemblyPath), "Modified_" + Path.GetFileName(assemblyPath));
                assemblyDefinition.Write(modifiedAssemblyPath);

          
             
                if (File.Exists(assemblyPath))
                {
                    File.Delete(assemblyPath);
             
                }

               
                File.Move(modifiedAssemblyPath, assemblyPath);
             
            }
            catch (Exception ex)
            {
                Console.WriteLine($"处理程序集 '{assemblyPath}' 时发生错误: {ex.Message}");
            }
        }
    }

   
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

    
    static void UpdateTargetFramework(AssemblyDefinition assemblyDefinition, string targetFramework)
    {
        var metadata = assemblyDefinition.CustomAttributes.FirstOrDefault(a => a.AttributeType.Name == "TargetFrameworkAttribute");
        if (metadata != null)
        {
      
            var currentFramework = metadata.ConstructorArguments[0].Value as string;
            if (currentFramework == targetFramework)
            {
              
                return;
            }
        
            metadata.ConstructorArguments[0] = new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, targetFramework);
    
        }
        else
        {
          
            var targetFrameworkAttribute = new Mono.Cecil.CustomAttribute(
                assemblyDefinition.MainModule.ImportReference(
                    typeof(System.Runtime.Versioning.TargetFrameworkAttribute).GetConstructor(new[] { typeof(string) })
                )
            );
            targetFrameworkAttribute.ConstructorArguments.Add(new CustomAttributeArgument(assemblyDefinition.MainModule.TypeSystem.String, targetFramework));
            assemblyDefinition.CustomAttributes.Add(targetFrameworkAttribute);
         
        }
    }

  
    static void UpdateReferences(AssemblyDefinition assemblyDefinition)
    {
      
        foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences)
        {
        
            if (reference.Name.StartsWith("System") || reference.Name.StartsWith("Microsoft"))
            {
              
                if (reference.Version != new Version(8, 0, 0, 0))
                {
                    reference.Version = new Version(8, 0, 0, 0);
              
                }
            }
            else if (reference.Name.Equals("0Harmony", StringComparison.OrdinalIgnoreCase))
            {
              
                reference.Name = "Harmony";
         
            }
            else
            {
             
            }
        }
    }
   
    static bool IsStardewValleyAssembly(AssemblyDefinition assemblyDefinition)
    {
    
        return assemblyDefinition.Name.Name.Equals("StardewValley", StringComparison.OrdinalIgnoreCase);
    }

    static void ModifyStardewReference(AssemblyDefinition assemblyDefinition)
    {
    
        foreach (var reference in assemblyDefinition.MainModule.AssemblyReferences)
        {
            if (reference.Name.Equals("Stardew\u0020Valley", StringComparison.OrdinalIgnoreCase))
            {
                reference.Name = "StardewValley";
              
            }
        }
    }
}