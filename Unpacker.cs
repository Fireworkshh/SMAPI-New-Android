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
            
                string destinationDir = MainActivity.GetPrivateStoragePath();

               
                AssetManager assets = context.Assets;
                using (Stream zipFileStream = assets.Open(zipFileName))
                {
                   
                    using (ZipArchive zip = new ZipArchive(zipFileStream))
                    {
                        foreach (var entry in zip.Entries)
                        {
                      
                            string destinationPath = Path.Combine(destinationDir, entry.FullName);

                       
                            if (entry.FullName.EndsWith("/"))
                            {
                                continue;
                            }

                         
                            if (!File.Exists(destinationPath))
                            {
                              
                                string entryDirectory = Path.GetDirectoryName(destinationPath);
                                if (!Directory.Exists(entryDirectory))
                                {
                                    Directory.CreateDirectory(entryDirectory);
                                }

                              
                                using (var entryStream = entry.Open())
                                using (var fileStream = new FileStream(destinationPath, FileMode.Create))
                                {
                                    entryStream.CopyTo(fileStream);
                                }
                            }
                        }
                    }
                }

              
                Console.WriteLine($"{zipFileName} unpacked successfully.");
            }
            catch (Exception ex)
            {
                
                Console.WriteLine($"Error unpacking {zipFileName}: {ex.Message}");
            }
        }
      
    }

}
