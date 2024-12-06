using System;
using System.IO;
using System.Security.Cryptography;
using Xamarin.Android.AssemblyStore;

public static class HashUtils
{
    public static string ComputeHash32(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 8).ToLower();  // 只取前 32 位
        }
    }

    public static string ComputeHash64(string filePath)
    {
        using (var stream = File.OpenRead(filePath))
        {
            var sha256 = SHA256.Create();
            var hashBytes = sha256.ComputeHash(stream);
            return BitConverter.ToString(hashBytes).Replace("-", "").Substring(0, 16).ToLower();  // 只取前 64 位
        }
    }
    internal static string GetFilePath(this AssemblyStoreExplorer explorer, string assemblyName)
    {
        // 检查 AssemblyStoreExplorer 是否为空
        if (explorer == null)
        {
            throw new ArgumentNullException(nameof(explorer), "AssemblyStoreExplorer cannot be null.");
        }

        // 遍历所有的程序集
        foreach (var assembly in explorer.Assemblies)
        {
            // 找到名称匹配的程序集
            if (string.Equals(assembly.DllName, assemblyName, StringComparison.OrdinalIgnoreCase))
            {
                // 返回该程序集的实际文件路径
                return Path.Combine(explorer.StorePath, assembly.Store.Arch, assembly.DllName);
            }
        }

        // 如果找不到对应的程序集，返回空
        return null;
    }
    // 将程序集字节流追加到 assemblies.blob

}
