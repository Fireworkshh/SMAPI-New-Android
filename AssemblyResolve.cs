using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace SMAPIStardewGame
{
    public class AssemblyResolve
    {
        public static Assembly HandleAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyName = new AssemblyName(args.Name).Name;



            string rootDirectory = Path.Combine("/storage/emulated/0/Android/data/app.SMAPIStardew/files/smapi-internal");

            try
            {
                var assemblyPaths = Directory.EnumerateFiles(rootDirectory, "*.dll", SearchOption.AllDirectories)
                    .Where(path => Path.GetFileNameWithoutExtension(path) == assemblyName).ToList();

                if (assemblyPaths.Any())
                {
                    return Assembly.LoadFrom(assemblyPaths.First());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"加载程序集时出错: {ex.Message}，程序集名: {assemblyName}");
                throw new Exception($"加载程序集时出错: {ex.Message}", ex);
            }

            return null;



        }
    }
}
