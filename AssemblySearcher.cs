using System;
using System.IO;
using System.Reflection;
using Xamarin.Android.AssemblyStore;

namespace StardewModdingAPI
{
    public static class AssemblySearcher
    {
       
        public static void PrintAllAssemblies(string filePath)
        {



  



         
            var explorer = new AssemblyStoreExplorer(filePath, null, keepStoreInMemory: true);

            
            foreach (var assembly in explorer.Assemblies)
            {
             
                string assemblyName = assembly.DllName;

             
                if (!string.IsNullOrEmpty(assembly.Store.Arch))
                {
                    assemblyName = assembly.Store.Arch + "/" + assemblyName;
                }

             
            }
        }
    }
}
