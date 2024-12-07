using ICSharpCode.SharpZipLib.Zip;
using SMAPI_Installation;
using StardewModdingAPI;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SMAPIStardewValley
{
    public class ExtractAssetsContentToPrivateStorage
    {
        // Show an error dialog on the UI thread
        private static void ShowErrorDialog(string title, string message)
        {
            MainActivity.mainActivity.RunOnUiThread(() =>
            {
                AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.mainActivity);
                dialog.SetTitle(title);  // Set dialog title
                dialog.SetMessage(message);  // Set error message

                // Add confirmation button
                dialog.SetPositiveButton("确认", (sender, e) => { });

                // Show dialog
                dialog.Show();
            });
        }

       
        public static async Task ExtractApkAssetsContentAsync()
        {
            try
            {
                var rooteStoragePath = MainActivity.GetPrivateStoragePath();
                string apkFilePath = Path.Combine(rooteStoragePath, "split_content.apk");

                // Check if the APK exists
                if (!File.Exists(apkFilePath))
                {
                    // Show an error dialog for missing APK
                    ShowErrorDialog("错误", "请安装谷歌版本星露谷物语");
                    return;
                }

                // Define the target directory for extraction (Content folder)
                string contentDirectoryPath = Path.Combine(rooteStoragePath, "Content");

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
            }
            catch (Exception ex)
            {
                // Show an error message in case of failure
                ShowErrorDialog("提取内容文件失败", "错误: " + ex.Message);

            }
        }
    }
}
