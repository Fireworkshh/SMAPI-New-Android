using ICSharpCode.SharpZipLib.Zip;
using SMAPI_Installation;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SMAPIStardewValley
{
    public class ExtractAssetsContentToPrivateStorage
    {
        public static async Task ExtractAssetsContentAsync(string privatePath)
        {
            try
            {
                // Path to the APK file
                string apkFilePath = Path.Combine(privatePath, "stardewvalley.apk");

                // Check if the APK exists
                if (!File.Exists(apkFilePath))
                {
                    Toast.MakeText(MainActivity.mainActivity, "APK 文件不存在", ToastLength.Long).Show();
                    return;
                }

                // Define the target directory for extraction (Content folder)
                string contentDirectoryPath = Path.Combine(privatePath, "Content");

                // Create the Content directory if it does not exist
                if (!Directory.Exists(contentDirectoryPath))
                {
                    Directory.CreateDirectory(contentDirectoryPath);
                }

                // Open the APK as a ZIP archive using SharpZipLib
                using (FileStream fs = File.OpenRead(apkFilePath))
                using (ZipFile zipFile = new ZipFile(fs))
                {
                    // Iterate through the entries in the APK (assets directory)
                    foreach (ZipEntry entry in zipFile)
                    {
                        // Check if the entry is inside the 'assets/Content' folder
                        if (entry.Name.StartsWith("assets/Content/"))
                        {
                            // Construct the file path to extract the content to
                            string extractedFilePath = Path.Combine(contentDirectoryPath, entry.Name.Substring("assets/Content/".Length));

                            // Skip extraction if the file already exists
                            if (File.Exists(extractedFilePath))
                            {
                                continue; // Skip this entry if the file already exists
                            }

                            // Ensure the directory exists
                            string extractedFileDirectory = Path.GetDirectoryName(extractedFilePath);
                            if (!Directory.Exists(extractedFileDirectory))
                            {
                                Directory.CreateDirectory(extractedFileDirectory);
                            }

                            // Extract the entry to the target directory
                            using (Stream entryStream = zipFile.GetInputStream(entry))
                            using (FileStream fileStream = new FileStream(extractedFilePath, FileMode.Create, FileAccess.Write))
                            {
                                await entryStream.CopyToAsync(fileStream);
                            }
                        }
                    }
                }

                // Optionally, show a success message
                // Toast.MakeText(MainActivity.mainActivity, "提取并保存内容文件成功", ToastLength.Long).Show();
            }
            catch (Exception ex)
            {
                // Show an error message in case of failure
                Toast.MakeText(MainActivity.mainActivity, "提取内容文件失败: " + ex.Message, ToastLength.Long).Show();
                throw;
            }
        }
    }
}
