using Android.Content.Res;
using SMAPI_Installation;
using System;
using System.IO;
using System.IO.Compression;

namespace StardewModdingAPI
{
    public static class Unpacker
    {
        public static void UnpackAllFilesFromAssets(Android.Content.Context context, string zipFileName)
        {
            try
            {
                // 获取目标路径，这里直接使用当前路径
                string destinationDir = MainActivity.GetPrivateStoragePath();

                // 访问 assets 目录中的 ZIP 文件
                AssetManager assets = context.Assets;
                using (Stream zipFileStream = assets.Open(zipFileName))
                {
                    // 使用 ZipArchive 处理压缩文件
                    using (ZipArchive zip = new ZipArchive(zipFileStream))
                    {
                        foreach (var entry in zip.Entries)
                        {
                            // 获取目标文件的路径，直接用 entry.FullName 作为文件名
                            string destinationPath = Path.Combine(destinationDir, entry.FullName);

                            // 如果是目录条目，则跳过
                            if (entry.FullName.EndsWith("/"))
                            {
                                continue;
                            }

                            // 如果目标文件不存在，才进行解压
                            if (!File.Exists(destinationPath))
                            {
                                // 创建父目录（如果不存在）
                                string entryDirectory = Path.GetDirectoryName(destinationPath);
                                if (!Directory.Exists(entryDirectory))
                                {
                                    Directory.CreateDirectory(entryDirectory);
                                }

                                // 解压文件
                                using (var entryStream = entry.Open())
                                using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                                {
                                    entryStream.CopyTo(fileStream);
                                }
                            }
                        }
                    }
                }

                // 输出解压成功消息
                Console.WriteLine($"{zipFileName} unpacked successfully.");
            }
            catch (Exception ex)
            {
                // 处理错误
                Console.WriteLine($"Error unpacking {zipFileName}: {ex.Message}");
            }
        }
    }
}
