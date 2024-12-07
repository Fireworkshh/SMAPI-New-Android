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
            
            string arm64V8aDir = Path.Combine(sourceDir, "smapi-internal", "arm64_v8a");
            string smapiInternalDir = Path.Combine(sourceDir, "smapi-internal");

        
            if (Directory.Exists(arm64V8aDir))
            {
            
                if (!Directory.Exists(smapiInternalDir))
                {
                    Directory.CreateDirectory(smapiInternalDir);
                }

            
                var files = Directory.GetFiles(arm64V8aDir);
                foreach (var file in files)
                {
             
                    string fileName = Path.GetFileName(file);
                    string targetFilePath = Path.Combine(smapiInternalDir, fileName);

             
                    if (File.Exists(targetFilePath))
                    {
                        File.Delete(targetFilePath);
                    }

              
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
            
            string sourceDir = MainActivity.GetPrivateStoragePath();
            string targetDir = Path.Combine(sourceDir, "Content", "Data");

         
            string sourceFileName = "Buildings.xnb";
            string targetFilePath = Path.Combine(targetDir, sourceFileName);

           
            if (!Directory.Exists(targetDir))
            {
                Directory.CreateDirectory(targetDir);
            }

           
            AssetManager assets = context.Assets;

       
            using (Stream assetStream = assets.Open(sourceFileName))
            {
            
                if (File.Exists(targetFilePath))
                {
                    File.Delete(targetFilePath);
                }

            
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
          
            Console.WriteLine($"Error moving Buildings.xnb from assets: {ex.Message}");
        }
    }
}
