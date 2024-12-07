using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Mono.Cecil;
using StardewValley;
using StardewValley.BellsAndWhistles;
using System;
using System.IO;
using System.Linq;


public class dnlibModify
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
    public static void ModifyOptionsElementDrawMethod(string assemblyPath)
    {
        try
        {
            // 加载程序集
            var module = ModuleDefMD.Load(assemblyPath);

         
            var optionsElementType = module.Types.FirstOrDefault(t => t.Name == "OptionsElement");
            if (optionsElementType == null)
            {
                Console.WriteLine("OptionsElement class not found!");
                return;
            }

          
            var drawMethod = optionsElementType.Methods.FirstOrDefault(m => m.Name == "draw");
            if (drawMethod == null)
            {
                Console.WriteLine("draw method not found!");
                return;
            }

            
            //666ModifyDrawMethodBody(drawMethod);

            // 保存修改后的程序集
            module.Write("ModifiedAssembly.dll");
            Console.WriteLine("Method successfully modified and saved!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error modifying method: {ex.Message}");
        }
    }

   /* private static void ModifyDrawMethodBody(MethodDef drawMethod)
    {
        // 获取 ILProcessor
        var ilProcessor = drawMethod.Body.GetILProcessor();  // 获取IL处理器

        // 清空原方法的指令
        drawMethod.Body.Instructions.Clear();

        // 获取方法所属的类型（即 OptionsElement）
        var optionsElementType = drawMethod.DeclaringType;

        // 插入自定义的逻辑：检查 whichOption 是否为 -1
        ilProcessor.Append(OpCodes.Ldarg_0); // 载入 this（即 OptionsElement）
        ilProcessor.Append(OpCodes.Ldfld, optionsElementType.Fields.First(f => f.Name == "whichOption"));  // 获取 whichOption 字段
        ilProcessor.Append(OpCodes.Ldc_I4, -1);  // 比较值 -1
        ilProcessor.Append(OpCodes.Beq_S, ilProcessor.Create(OpCodes.Nop));  // 如果 whichOption == -1，跳转到 Nop（表示跳过原来的逻辑）

        // 添加自定义绘制逻辑
        ilProcessor.Append(OpCodes.Ldarg_1); // b
        ilProcessor.Append(OpCodes.Ldarg_2); // slotX
        ilProcessor.Append(OpCodes.Ldarg_3); // slotY
        ilProcessor.Append(OpCodes.Ldarg_0); // this
        ilProcessor.Append(OpCodes.Ldfld, optionsElementType.Fields.First(f => f.Name == "label"));  // 获取 label 字段
        ilProcessor.Append(OpCodes.Ldc_I4, 999); // 其他参数
        ilProcessor.Append(OpCodes.Ldc_I4, -1);
        ilProcessor.Append(OpCodes.Ldc_I4, 999);
        ilProcessor.Append(OpCodes.Ldc_R4, 1f);
        ilProcessor.Append(OpCodes.Ldc_R4, 0.1f);
        ilProcessor.Append(OpCodes.Ldc_I4, 0);
        ilProcessor.Append(OpCodes.Ldc_I4, -1);
        ilProcessor.Append(OpCodes.Ldnull);
        ilProcessor.Append(OpCodes.Call, typeof(SpriteText).GetMethod("drawString"));  // 调用 SpriteText.drawString

        // 跳过原始方法的后续代码
        ilProcessor.Append(OpCodes.Ret);  // 结束方法
    }*/
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