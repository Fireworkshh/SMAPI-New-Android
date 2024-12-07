using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text.Style;
using Android.Text;
using Android.Views;
using Android.Widget;
using SMAPIStardewValley;
using StardewModdingAPI;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xamarin.Android.Tools.DecompressAssemblies;
using Android.Text.Method;

namespace SMAPI_Installation
{
    [Activity(Label = "SMAPIStardew Valley ", MainLauncher = true)]
    public class MainActivity : Activity
    {
        private const int RequestCodeStoragePermission = 1;
        public static MainActivity mainActivity;
        private ProgressBar progressBar;
        private TextView statusText;
        private TextView descriptionText;
        private Button installButton;
        private Button launchButton; 
        private TextView infoText;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            mainActivity = this;
            infoText = FindViewById<TextView>(Resource.Id.infoText);
          
            // 获取 UI 控件引用
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            statusText = FindViewById<TextView>(Resource.Id.statusText);
            descriptionText = FindViewById<TextView>(Resource.Id.descriptionText);
            installButton = FindViewById<Button>(Resource.Id.installButton);
            launchButton = FindViewById<Button>(Resource.Id.launchButton);
            launchButton.Visibility = ViewStates.Invisible;
            SetProgressBarVisibility(false);
            infoText.Visibility = ViewStates.Visible;
            string text = "1. 请在谷歌下载游戏然后点击安装，安装后启动即可。\n" +
                  
                       "2. 模组放在/storage/emulated/0/Android/data/app.SMAPIStardew/Mods。\n" +
                       "3. 有任何问题请加群 985754557 询问。\n" +
                       "4. 项目地址已经开源 https://github.com/Fireworkshh/SMAPI-New-Android。\n" +
                       "5. 有问题可以在哔哩哔哩联系。";


            SpannableString spannableString = new SpannableString(text);


            int start = text.IndexOf("有任何问题请加群 985754557 询问");
            int end = start + "有任何问题请加群 985754557 询问".Length;
            spannableString.SetSpan(new URLSpan("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=4guX1RqKVQE7nawKcsnOZ477ntb2nrY3&authKey=oTUbE%2BI4fVMghqGJ4rYwAjTzoJ4d2fI8ixDcsNF6S4NYOTkJ63iBrRGhZaB2XAkH&noverify=0&group_code=781588105"), start, end, SpanTypes.ExclusiveExclusive);

            start = text.IndexOf("有问题可以在哔哩哔哩联系");
            end = start + "有问题可以在哔哩哔哩联系".Length;
            spannableString.SetSpan(new URLSpan("https://b23.tv/zVL6x1g"), start, end, SpanTypes.ExclusiveExclusive);

            infoText.TextFormatted = spannableString;


            infoText.MovementMethod = LinkMovementMethod.Instance;



            if (IsSMAPIInstalled())
            {
                installButton.Visibility = ViewStates.Gone;

                statusText.Visibility = ViewStates.Visible;

                launchButton.Visibility = ViewStates.Visible;
                ShowLaunchButton();


            }
            else
            {
                statusText.Visibility = ViewStates.Visible;

                statusText.Text = "请安装SMAPI...";
                installButton.Click += async (sender, e) =>
                {

                    SetProgressBarVisibility(true);


                    StartInstallationProcess();

                };


            }
        }






  

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == RequestCodeStoragePermission)
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
                {
                    if (CheckSelfPermission(Android.Manifest.Permission.ManageExternalStorage) == Permission.Granted)
                    {
                        // 权限授予，继续操作
                        RequestStoragePermissions();
                    }
                    else
                    {
                        // 权限未授予，提醒用户
                        Toast.MakeText(this, "存储权限未授予，请手动授予权限", ToastLength.Long).Show();
                    }
                }
                else
                {
                    // 处理其他权限的结果
                    if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        RequestStoragePermissions();
                    }
                    else
                    {
                        Toast.MakeText(this, "存储权限未授予", ToastLength.Long).Show();
                    }
                }
            }
        }

        private void RequestStoragePermissions()
        {
            // Handle additional functionality when storage permission is granted
            Toast.MakeText(this, "存储权限已授予，继续操作", ToastLength.Short).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestCodeStoragePermission)
            {
                if (grantResults.Length > 0 && grantResults.All(result => result == Permission.Granted))
                {
                    // All permissions granted, continue
                    Toast.MakeText(this, "所有权限已授予，继续操作", ToastLength.Short).Show();
                }
                else
                {
                    // Some permissions were denied
                    ShowErrorDialog("权限请求失败", "需要完整存储权限和应用查询权限才能继续");
                }
            }
        }
        private  async void  StartInstallationProcess()
        {
            string packageName = "com.chucklefish.stardewvalley";

            
            if (IsAppInstalled(packageName))
            {

            
            
                await  ExtractAndSaveApk(packageName);
             
            }
            else
            {
              
                ShowInstallDialog(packageName);
            }
        }
     
        private bool IsAppInstalled(string packageName)
        {
            PackageManager packageManager = Application.Context.PackageManager;
            try
            {
                packageManager.GetPackageInfo(packageName, PackageInfoFlags.MatchDefaultOnly);
                return true; // App is installed
            }
            catch (PackageManager.NameNotFoundException)
            {
                return false; // App is not installed
            }
        }

        // Show a dialog asking the user to install the app
        private void ShowInstallDialog(string packageName)
        {
    
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);

            // 设置对话框消息
            dialog.SetMessage("游戏未安装，是否立即安装？");

            // 设置“安装”按钮，点击后跳转到 Google Play 商店
            dialog.SetPositiveButton("安装", (sender, e) =>
            {
                // 创建 Intent，跳转到 Google Play 商店
                string googlePlayUrl = "https://play.google.com/store/apps/details?id=" + packageName;
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(googlePlayUrl));
                StartActivity(intent);

            });

            // 设置“取消”按钮，点击后不做任何操作
            dialog.SetNegativeButton("取消", (sender, e) => { });

            // 显示对话框
            dialog.Show();
        }




        private string GetApkPathByPackageName(string packageName)
        {
            try
            {
                // 获取PackageManager实例
                PackageManager packageManager = PackageManager;

                // 使用包名获取应用的ApplicationInfo
                ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);

                // 获取应用的APK路径
                string apkPath = appInfo.SourceDir;

                return apkPath;
            }
            catch (PackageManager.NameNotFoundException ex)
            {
                // 弹出错误对话框，应用未安装
                ShowErrorDialog("应用未找到", "应用包名无效或应用未安装: " + ex.Message);
                return string.Empty;
            }
        }
        // Extract APK and save it to the private storage
        private async Task ExtractAndSaveApk(string packageName)
        {
            SetProgressBarVisibility(true);
            installButton.Visibility = ViewStates.Gone;

            string apkPath = GetApkPathByPackageName(packageName);
            string splitContentApkPath = GetSplitContentApkPath(packageName);

            if (!string.IsNullOrEmpty(apkPath) || !string.IsNullOrEmpty(splitContentApkPath))
            {
                RunOnUiThread(() =>
                {
                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                    progressBar.Progress = 0;
                    statusText.Text = "正在提取APK...";
                    descriptionText.Text = "请稍候...";
                });

                // 获取私有存储路径
                string privatePath = GetPrivateStoragePath();

                // 创建目标路径以保存 APK 文件
                string destinationPath = Path.Combine(privatePath, "stardewvalley.apk");

                try
                {
                    // 如果找到主 APK 文件，将其复制到私有存储
                    if (!string.IsNullOrEmpty(apkPath))
                    {
                        File.Copy(apkPath, destinationPath, true); // true 表示覆盖已存在文件
                    }

                    // 提取 split_content.apk 文件
                    if (File.Exists(splitContentApkPath))
                    {
                        string splitContentDestinationPath = Path.Combine(privatePath, "split_content.apk");
                        File.Copy(splitContentApkPath, splitContentDestinationPath, true);
                    }

                    UpdateProgress(25, "正在安装解压APK...");

                    // 提取 APK 内容
                    UpdateProgress(50, "正在提取游戏内容...");
                  

                    string splitContentapks = Path.Combine(privatePath, "split_content.apk");

                    if (!string.IsNullOrEmpty(splitContentapks))
                    {
                        await ExtractAssetsContentToPrivateStorage.ExtractApkAssetsContentAsync();
                    }

                    // 解压 APK 内部资源
                    App.UncompressFromAPK(destinationPath, "assemblies/");

                    // 解压其他资产
                    UpdateProgress(75, "正在安装SMAPI...");
                    Unpacker.UnpackAllFilesFromAssets(this, "smapi-internal.zip");
                    Unpacker.UnpackAllFilesFromAssets(this, "dotnet.zip");

                    // 移动建筑物文件
                    MoveBuildings.MoveBuildingsFileToTargetDirectory(this);

                    // 更新进度为 100% 完成安装
                    UpdateProgress(100, "SMAPI安装完成！");

                    RunOnUiThread(() =>
                    {
                        descriptionText.Text = "请启动游戏";
                        // 显示安装完成消息
                        if (IsSMAPIInstalled())
                        {
                            installButton.Visibility = ViewStates.Invisible;
                            statusText.Visibility = ViewStates.Visible;
                            launchButton.Visibility = ViewStates.Visible; // 默认隐藏启动按钮
                            ShowLaunchButton();
                        }
                    });
                }
                catch (IOException e)
                {
                    // 显示错误信息弹窗
                    ShowErrorDialog("文件复制失败", e.Message);
                    throw;
                }
            }
            else
            {
                // 未找到APK文件或split_content.apk
                Toast.MakeText(this, "未找到APK文件或split_content.apk", ToastLength.Long).Show();
            }
        }

        // 获取已安装应用的路径和split_content.apk路径
        private string GetSplitContentApkPath(string packageName)
        {
            string splitContentApkPath = string.Empty;

            try
            {
                // 获取PackageManager实例
                PackageManager packageManager = PackageManager;
                ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);
                string appSourceDir = appInfo.SourceDir; // 获取应用的APK路径

                // 假设split_content.apk与主APK文件位于同一目录
                string directory = Path.GetDirectoryName(appSourceDir);
                splitContentApkPath = Path.Combine(directory, "split_content.apk");
            }
            catch (PackageManager.NameNotFoundException ex)
            {
                // 弹出错误对话框，应用未找到
                ShowErrorDialog("应用未找到", "应用包名无效或应用未安装: " + ex.Message);
            }

            return splitContentApkPath;
        }

        private void ShowErrorDialog(string title, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);  // 设置标题
            dialog.SetMessage(message);  // 设置错误消息

            // 添加确认按钮
            dialog.SetPositiveButton("确认", (sender, e) => { });

            // 显示对话框
            dialog.Show();
        }
        private bool IsSMAPIInstalled()
        {
            
            string privateStoragePath = GetPrivateStoragePath();

            
            string[] directoriesToCheck = new string[]
            {
        Path.Combine(privateStoragePath, "Content"),
        Path.Combine(privateStoragePath, "Content/Animals"),
        Path.Combine(privateStoragePath, "Content/Buildings"),
         Path.Combine(privateStoragePath, "Content/Characters"),
        Path.Combine(privateStoragePath, "Content/Data"),
         Path.Combine(privateStoragePath, "Content/Effects"),
            Path.Combine(privateStoragePath, "dotnet"),
        Path.Combine(privateStoragePath, "smapi-internal")
            };

            
            foreach (string directory in directoriesToCheck)
            {
                if (!Directory.Exists(directory))
                {
                    return false; 
                }
            }

            return true; 
        }

        private void ShowLaunchButton()
        {
           
          
      

            // 添加按钮的样式和点击事件
          
            launchButton.SetBackgroundColor(Android.Graphics.Color.Rgb(34, 193, 195));
            launchButton.SetTextColor(Android.Graphics.Color.White);
            launchButton.TextSize = 18;
            launchButton.SetPadding(30, 20, 30, 20);
            launchButton.Click += LaunchGame; // 添加点击事件
        }
       
        private void LaunchGame(object sender, EventArgs e)
        {
            descriptionText.Text = "启动游戏中...";
            progressBar.Visibility = ViewStates.Visible;
            // 启动游戏逻辑
            SetProgressBarVisibility(true);
            Intent intent = new Intent(this, typeof(GameMainActivity));
       
                StartActivity(intent); // 启动游戏
            
        
        }

        private bool isProgressVisible = false;
        private void SetProgressBarVisibility(bool isVisible)
        {
            isProgressVisible = isVisible;

            RunOnUiThread(() =>
            {
                if (isProgressVisible)
                {
                    progressBar.Visibility = ViewStates.Visible;
                }
                else
                {
                    progressBar.Visibility = ViewStates.Invisible;
                }
            });
        }

        // Update progress bar and status message
        private void UpdateProgress(int progress, string status)
        {
            RunOnUiThread(() =>
            {
                progressBar.Progress = progress;
                statusText.Text = status;
            });
        }

        // Extract assets content to the private storage



        // Get APK path by package name using PackageManager
    
        // Get the application's private storage path
        public static string GetPrivateStoragePath()
        {
            return mainActivity.GetExternalFilesDirs((string?)null)[0].ToString();
        }
    }
}
