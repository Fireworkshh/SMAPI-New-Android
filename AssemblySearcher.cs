using System;
using System.IO;
using System.Reflection;
using Xamarin.Android.AssemblyStore;

namespace StardewModdingAPI
{
    public static class AssemblySearcher
    {
        // 搜索当前执行程序集所在目录的所有程序集
        public static void PrintAllAssemblies(string filePath)
        {



  



            // 创建 AssemblyStoreExplorer 实例，解析 APK 或 AAB 文件
            var explorer = new AssemblyStoreExplorer(filePath, null, keepStoreInMemory: true);

            // 遍历所有的程序集
            foreach (var assembly in explorer.Assemblies)
            {
                // 获取程序集名称
                string assemblyName = assembly.DllName;

                // 如果程序集包含架构信息，附加到名称前面
                if (!string.IsNullOrEmpty(assembly.Store.Arch))
                {
                    assemblyName = assembly.Store.Arch + "/" + assemblyName;
                }

             
            }
        }
    }
}
