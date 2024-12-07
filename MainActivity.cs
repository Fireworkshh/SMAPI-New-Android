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
        private Button launchButton; // 新增启动游戏按钮
        private TextView infoText; // 新增文本控件
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);  // 加载布局文件

            mainActivity = this;
            infoText = FindViewById<TextView>(Resource.Id.infoText); // 获取信息文本控件

            // 获取 UI 控件引用
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            statusText = FindViewById<TextView>(Resource.Id.statusText);
            descriptionText = FindViewById<TextView>(Resource.Id.descriptionText);
            installButton = FindViewById<Button>(Resource.Id.installButton);
            launchButton = FindViewById<Button>(Resource.Id.launchButton); // 获取启动按钮
            launchButton.Visibility = ViewStates.Invisible; // 默认隐藏启动按钮
            SetProgressBarVisibility(false);
            infoText.Visibility = ViewStates.Visible;
            string text = "1. 请在谷歌下载游戏然后点击安装，安装后启动即可。\n" +
                       "2. 请在设置打开系统优化，不要关闭系统优化，否则可能无法提取安装包。\n" +
                       "3. 模组放在/storage/emulated/0/Android/data/app.SMAPIStardew/Mods。\n" +
                       "4. 有任何问题请加群 985754557 询问。\n" +
                       "5. 项目地址已经开源 https://github.com/Fireworkshh/SMAPI-New-Android。\n" +
                       "6. 哔哩哔哩 UP 主是猫离个喵哦。";

            // 使用 SpannableString 设置超链接
            SpannableString spannableString = new SpannableString(text);

            // 设置第二行和第三行为超链接
            int start = text.IndexOf("有任何问题请加群 985754557 询问");
            int end = start + "有任何问题请加群 985754557 询问".Length;
            spannableString.SetSpan(new URLSpan("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=4guX1RqKVQE7nawKcsnOZ477ntb2nrY3&authKey=oTUbE%2BI4fVMghqGJ4rYwAjTzoJ4d2fI8ixDcsNF6S4NYOTkJ63iBrRGhZaB2XAkH&noverify=0&group_code=781588105"), start, end, SpanTypes.ExclusiveExclusive);

            start = text.IndexOf("哔哩哔哩 UP 主是猫离个喵哦");
            end = start + "哔哩哔哩 UP 主是猫离个喵哦".Length;
            spannableString.SetSpan(new URLSpan("https://b23.tv/zVL6x1g"), start, end, SpanTypes.ExclusiveExclusive);

            // 设置 TextView 的文本
            infoText.TextFormatted = spannableString;

            // 启用超链接点击支持
            infoText.MovementMethod = LinkMovementMethod.Instance;


            // 设定按钮点击事件

            if (IsSMAPIInstalled())
            {
                installButton.Visibility = ViewStates.Gone;

                statusText.Visibility = ViewStates.Visible;

                launchButton.Visibility = ViewStates.Visible; // 默认隐藏启动按钮
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


         


            // 请求存储权限
            RequestPermissions(new string[] { Android.Manifest.Permission.WriteExternalStorage }, RequestCodeStoragePermission);
        }
        private  async void  StartInstallationProcess()
        {
            string packageName = "com.chucklefish.stardewvalley";

            // 检查游戏是否已安装
            if (IsAppInstalled(packageName))
            {

            
                // 游戏已安装，提取并保存APK
                await  ExtractAndSaveApk(packageName);
             
            }
            else
            {
                // 游戏未安装，提示用户安装
                ShowInstallDialog(packageName);
            }
        }
        // Check if the app is installed
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
            // 创建对话框构建器
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


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestCodeStoragePermission)
            {
                if (grantResults.Length > 0 && grantResults.All(result => result == Permission.Granted))
                {
                    // All permissions granted, operate normally
                 //   Toast.MakeText(this, "所有权限已授予，继续操作", ToastLength.Short).Show();
                }
                else
                {
                    // Some permissions were denied
                    Toast.MakeText(this, "需要完整存储权限和应用查询权限才能继续", ToastLength.Long).Show();
                }
            }
        }

        // Extract APK and save it to the private storage
        private async Task ExtractAndSaveApk(string packageName)
        {
            SetProgressBarVisibility(true);
            installButton.Visibility = ViewStates.Gone;
           
            string apkPath = GetApkPathByPackageName(packageName);

            if (!string.IsNullOrEmpty(apkPath))
            {
                RunOnUiThread(() =>
                {
                    progressBar.Visibility = Android.Views.ViewStates.Visible;
                    progressBar.Progress = 0;
                    statusText.Text = "正在提取APK...";
                    descriptionText.Text = "请稍候...";
                });
                // Get the private storage directory
                string privatePath = GetPrivateStoragePath();

                // Create destination path for saving the APK
                string destinationPath = Path.Combine(privatePath, "stardewvalley.apk");

                try
                {
                    // Copy the APK file to private storage
                    File.Copy(apkPath, destinationPath, true); // true means overwrite if exists
                    UpdateProgress(25, "正在安装解压APK...");
                  

                    UpdateProgress(50, "正在提取游戏内容...");


                   ExtractAssetsContentToPrivateStorage.ExtractAssetsContentAsync(privatePath);
                    // Call a function to decompress APK and extract content
                    App.UncompressFromAPK(destinationPath, "assemblies/");

                    // Unpack additional assets

                    UpdateProgress(75, "正在安装SMAPI...");
                 
                    Unpacker.UnpackAllFilesFromAssets(this, "smapi-internal.zip");
                    MoveBuildings.MoveBuildingsFileToTargetDirectory(this);
                    // Update progress to 100% after everything is done
                    UpdateProgress(100, "SMAPI安装完成！");
              
                    RunOnUiThread(() =>
                    {
                        descriptionText.Text = "请启动游戏";
                        // Show installation complete message
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
                    Toast.MakeText(this, "文件复制失败: " + e.Message, ToastLength.Long).Show();
                    throw;
                }
            }
            else
            {
                Toast.MakeText(this, "未找到APK文件", ToastLength.Long).Show();
            }
        }
        private bool IsSMAPIInstalled()
        {
            // 获取私有存储路径
            string privateStoragePath = GetPrivateStoragePath();

            // 检查多个文件夹是否存在
            string[] directoriesToCheck = new string[]
            {
        Path.Combine(privateStoragePath, "Content"),
     //   Path.Combine(privateStoragePath, "dotnet"),
        Path.Combine(privateStoragePath, "smapi-internal")
            };

            // 遍历文件夹，检查是否存在
            foreach (string directory in directoriesToCheck)
            {
                if (!Directory.Exists(directory))
                {
                    return false; // 如果任意一个文件夹不存在，返回false
                }
            }

            return true; // 所有文件夹都存在，返回true
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
        private string GetApkPathByPackageName(string packageName)
        {
            PackageManager packageManager = Application.Context.PackageManager;

            try
            {
                // Get the package info for the specified package name
                PackageInfo packageInfo = packageManager.GetPackageInfo(packageName, 0); // 0 means no extra flags
                return packageInfo.ApplicationInfo.SourceDir;
            }
            catch (PackageManager.NameNotFoundException)
            {
                return null;
            }
        }

        // Get the application's private storage path
        public static string GetPrivateStoragePath()
        {
            return mainActivity.GetExternalFilesDirs((string?)null)[0].ToString();
        }
    }
}
