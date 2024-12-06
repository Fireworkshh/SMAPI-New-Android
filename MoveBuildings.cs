using Android.Content.Res;
using SMAPI_Installation;
using System;
using System.IO;
using System.Security.Cryptography;
using Xamarin.Android.AssemblyStore;

public static class MoveBuildings
{
      private static void MoveFilesFromArm64V8aToSmapiInternal(string sourceDir)
    {
        try
        {
            // 获取 smapi-internal/arm64_v8a 目录和目标 smapi-internal 目录路径
            string arm64V8aDir = Path.Combine(sourceDir, "smapi-internal", "arm64_v8a");
            string smapiInternalDir = Path.Combine(sourceDir, "smapi-internal");

            // 如果 arm64_v8a 目录存在且有文件
            if (Directory.Exists(arm64V8aDir))
            {
                // 确保 smapi-internal 目录存在
                if (!Directory.Exists(smapiInternalDir))
                {
                    Directory.CreateDirectory(smapiInternalDir);
                }

                // 获取 arm64_v8a 目录中的所有文件
                var files = Directory.GetFiles(arm64V8aDir);
                foreach (var file in files)
                {
                    // 构建目标文件路径
                    string fileName = Path.GetFileName(file);
                    string targetFilePath = Path.Combine(smapiInternalDir, fileName);

                    // 如果目标文件已存在，则删除
                    if (File.Exists(targetFilePath))
                    {
                        File.Delete(targetFilePath);
                    }

                    // 移动文件到目标目录
                    File.Move(file, targetFilePath);

                    Console.WriteLine($"Moved file: {fileName} from arm64_v8a to smapi-internal.");
                }
            }
            else
            {
                Console.WriteLine("The arm64_v8a directory does not exist or is empty.");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error moving files from arm64_v8a to smapi-internal: {ex.Message}");
        }
    }

    public static void MoveBuildingsFileToTargetDirectory(Android.Content.Context context)
    {
        try
        {
            // 获取目标目录路径
            string sourceDir = MainActivity.GetPrivateStoragePath();
            string targetDir = Path.Combine(sourceDir, "Content", "Data");

            // 构建源文件路径和目标文件路径
            string sourceFileName = "Buildings.xnb";
            string targetFilePath = Path.Combine(targetDir, sourceFileName);

            // 确保目标目录存在
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

            // 获取 assets 目录
            AssetManager assets = context.Assets;

            // 检查 Buildings.xnb 是否存在于 assets 目录
            using (Stream assetStream = assets.Open(sourceFileName))
            {
                // 如果目标文件已经存在，删除它
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

                // 将文件从 assets 复制到目标路径
                using (var fileStream = new FileStream(targetFilePath, FileMode.Create))
                {
                    assetStream.CopyTo(fileStream);
                }

                Console.WriteLine("Buildings.xnb has been moved successfully from assets.");
                MoveFilesFromArm64V8aToSmapiInternal(sourceDir);
            }
        }
        catch (Exception ex)
        {
            // 处理错误
            Console.WriteLine($"Error moving Buildings.xnb from assets: {ex.Message}");
        }
    }
}
