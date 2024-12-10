using System;
using System.Buffers;
using System.IO;
using System.IO.Compression;
using K4os.Compression.LZ4;
using SMAPI_Installation;
using StardewModdingAPI;
using Xamarin.Android.AssemblyStore;


namespace Xamarin.Android.Tools.DecompressAssemblies
{
    internal class App
    {
        private const uint CompressedDataMagic = 1514946904u;

        private static readonly ArrayPool<byte> bytePool;



        private static bool UncompressDLL(Stream inputStream, string fileName, string filePath, string prefix)
        {
            string text = prefix + filePath;

        
            string privatePath = MainActivity.GetPrivateStoragePath() + "/smapi-internal";

            // Check if the directory exists, if not, create it
            if (!Directory.Exists(privatePath))
            {
                Directory.CreateDirectory(privatePath);
            }

            text = Path.Combine(privatePath, filePath); 
            bool result = true;
         

            using (BinaryReader binaryReader = new BinaryReader(inputStream))
            {
                if (binaryReader.ReadUInt32() == 1514946904)
                {
                    binaryReader.ReadUInt32();
                    uint num = binaryReader.ReadUInt32();
                    int num2 = (int)(inputStream.Length - 12);
                    byte[] array = bytePool.Rent(num2);
                    binaryReader.Read(array, 0, num2);
                    byte[] array2 = bytePool.Rent((int)num);
                    int num3 = LZ4Codec.Decode(array, 0, num2, array2, 0, (int)num);

                    if (num3 != (int)num)
                    {
                        Console.Error.WriteLine($"  解压LZ4数据失败 {fileName} (解码: {num3})");
                        result = false;
                    }
                    else
                    {
                        string directoryName = Path.GetDirectoryName(text);
                        if (!string.IsNullOrEmpty(directoryName))
                        {
                            Directory.CreateDirectory(directoryName);
                        }

                       
                        using (FileStream fileStream = File.Open(text, FileMode.Create, FileAccess.Write))
                        {
                            fileStream.Write(array2, 0, num3);
                            fileStream.Flush();
                        }
                       

                     
                        if (ShouldDeleteAssembly(text))
                        {
                            File.Delete(text);
                          
                        }
                    }

                    bytePool.Return(array);
                    bytePool.Return(array2);
                }
                else
                {
                   
                }
            }
            return result;
        }

        private static bool ShouldDeleteAssembly(string filePath)
        {
           
            string fileName = Path.GetFileName(filePath).ToLower();

            
            if (fileName.Equals("monogame.framework.dll"))
            {
                return false;
            }

         
            return  
                  fileName.StartsWith("xamarin.")
                || fileName.StartsWith("mono.")
                      || fileName.StartsWith("system.")
                || fileName.StartsWith("microsoft."); 
        }


        private static bool UncompressDLL(string filePath, string prefix)
        {
            using (FileStream inputStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                return UncompressDLL(inputStream, filePath, Path.GetFileName(filePath), prefix);
            }
        }

        private static async Task<bool> UncompressFromAPK_IndividualEntries(ZipArchive apk, string filePath, string assembliesPath, string prefix)
        {
            foreach (ZipArchiveEntry item in apk.Entries)  // 使用 .Entries 遍历条目
            {
                if (item.FullName.StartsWith(assembliesPath, StringComparison.Ordinal) && item.FullName.EndsWith(".dll", StringComparison.Ordinal))
                {
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        item.Open().CopyTo(memoryStream);  // 使用 Open() 读取条目
                        memoryStream.Seek(0L, SeekOrigin.Begin);
                        string filePath2 = item.FullName.Substring(assembliesPath.Length);
                        UncompressDLL(memoryStream, filePath + "!" + item.FullName, filePath2, prefix);
                    }
                }
            }
            return true;
        }

        private static async Task<bool> UncompressFromAPK_AssemblyStores(string filePath, string prefix)
        {
            foreach (AssemblyStoreAssembly assembly in new AssemblyStoreExplorer(filePath, null, keepStoreInMemory: true).Assemblies)
            {
                string text = assembly.DllName;
                if (!string.IsNullOrEmpty(assembly.Store.Arch))
                {
                    text = assembly.Store.Arch + "/" + text;
                }
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    assembly.ExtractImage(memoryStream);
                    memoryStream.Seek(0L, SeekOrigin.Begin);
                    UncompressDLL(memoryStream, filePath + "!" + text, text, prefix);
                }
            }
            return true;
        }


        public static async Task<bool> UncompressFromAPKAsync(string filePath, string assembliesPath)
        {
            string prefix = $"uncompressed-{Path.GetFileNameWithoutExtension(filePath)}{Path.DirectorySeparatorChar}";

            // 打开 ZIP 文件进行读取
            using (ZipArchive zipArchive = System.IO.Compression.ZipFile.OpenRead(filePath))
            {
                // 检查 ZIP 文件中是否包含指定的 "assemblies.blob" 条目
                bool containsAssembliesBlob = zipArchive.Entries.Any(entry => entry.FullName == assembliesPath + "assemblies.blob");

                if (!containsAssembliesBlob)
                {
                    return await UncompressFromAPK_IndividualEntries(zipArchive, filePath, assembliesPath, prefix);
                }
            }

            // 如果找到了 "assemblies.blob" 则使用 AssemblyStores 方式进行解压
            return await UncompressFromAPK_AssemblyStores(filePath, prefix);
        }



        static App()
        {
            bytePool = ArrayPool<byte>.Shared;
        }
        public static string GetPrivateStoragePath()
        {
            return MainActivity.mainActivity.ApplicationContext.GetExternalFilesDir(null).AbsolutePath;
        }
        private static bool CompressDLL(string filePath, string outputFile)
        {
            bool result = true;
            Console.WriteLine("正在压缩 " + filePath);
            string directoryName = Path.GetDirectoryName(outputFile);
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            using (FileStream fileStream = File.Open(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] array = new byte[fileStream.Length];
                fileStream.Read(array, 0, array.Length);
                byte[] array2 = bytePool.Rent(LZ4Codec.MaximumOutputSize(array.Length));
                int num = LZ4Codec.Encode(array, 0, array.Length, array2, 0, array2.Length);
                if (num < 0)
                {
                    Console.Error.WriteLine("压缩失败: " + filePath);
                    result = false;
                }
                else
                {
                    using (FileStream fileStream2 = File.Open(outputFile, FileMode.Create, FileAccess.Write))
                    {
                        fileStream2.Write(BitConverter.GetBytes(1514946904u), 0, 4);
                        fileStream2.Write(BitConverter.GetBytes(0u), 0, 4);
                        fileStream2.Write(BitConverter.GetBytes((uint)array.Length), 0, 4);
                        fileStream2.Write(array2, 0, num);
                        long position = fileStream2.Position;
                        fileStream2.Seek(4L, SeekOrigin.Begin);
                        fileStream2.Write(BitConverter.GetBytes((uint)num), 0, 4);
                        fileStream2.Seek(position, SeekOrigin.Begin);
                    }
                    Console.WriteLine("压缩到: " + outputFile);
                }
                bytePool.Return(array2);
                return result;
            }
        }
    }
}
