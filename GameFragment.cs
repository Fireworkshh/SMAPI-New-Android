using Android;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using Google.Android.Material.FloatingActionButton;
using SMAPI_Installation;
using SMAPIStardewValley;
using StardewModdingAPI;
using Xamarin.Android.Tools.DecompressAssemblies;
using static AndroidX.Loader.App.LoaderManager;
using Context = Android.Content.Context;
using Resource = SMAPIStardewValley.Resource;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;
using SMAPIStardewGame;
public class GameFragment : AndroidX.Fragment.App.Fragment
{
    private FloatingActionButton startGameButton; // 启动游戏按钮
    private ProgressBar progressBar; // 进度条
    private static Context context;

    private TextView statusText;
    private TextView descriptionText;
  
    private Button launchButton;
    private TextView infoText;
    protected FloatingActionButton MainFABRefresh; // 刷新按钮
    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        // Inflate the fragment layout
        var view = inflater.Inflate(Resource.Layout.fragment_game, container, false);

        // 查找按钮控件
        var MainFABImport = view.FindViewById<Button>(Resource.Id.install_smapi);

        MainFABImport.Click += (s, e) =>
        {
            SetProgressBarVisibility(true);
            StartInstallationProcess();
        };

        // 初始化UI控件
        infoText = view.FindViewById<TextView>(Resource.Id.infoText);
        progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar);
        statusText = view.FindViewById<TextView>(Resource.Id.statusText);
        descriptionText = view.FindViewById<TextView>(Resource.Id.descriptionText);
        launchButton = view.FindViewById<Button>(Resource.Id.launchButton);

        // 初始化进度条
        progressBar.Visibility = ViewStates.Invisible;
        var layoutParams = new LinearLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent
        )
        {
            Gravity = GravityFlags.Center
        };
        progressBar.LayoutParameters = layoutParams;
        progressBar.Indeterminate = true;

        // 设置链接文本为可点击
        infoText.MovementMethod = LinkMovementMethod.Instance;

        // 为launchButton设置点击事件
        launchButton.Click += (sender, args) =>
        {
            string errorMessage = VerifyGame.CheckSMAPIInstallation();

            if (errorMessage != null)
            {
                ShowErrorDialog("安装错误", errorMessage);
            }
            else

            {
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve.HandleAssemblyResolve;

                dnlibPatchPatch_StardewValley.dnlibPatch();
                LaunchGame();
            }
        };

        // 为加入群聊和哔哩哔哩链接设置点击事件
        var joinGroupText = view.FindViewById<TextView>(Resource.Id.joinGroupText);
        var bilibiliLinkText = view.FindViewById<TextView>(Resource.Id.bilibiliLinkText);

        joinGroupText.Click += (sender, e) =>
        {
            var uri = Android.Net.Uri.Parse("https://b23.tv/zVL6x1g"); // 这里放加群链接
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        };

        bilibiliLinkText.Click += (sender, e) =>
        {
            var uri = Android.Net.Uri.Parse("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=4guX1RqKVQE7nawKcsnOZ477ntb2nrY3&authKey=oTUbE%2BI4fVMghqGJ4rYwAjTzoJ4d2fI8ixDcsNF6S4NYOTkJ63iBrRGhZaB2XAkH&noverify=0&group_code=781588105"); // 这里放哔哩哔哩链接
            var intent = new Intent(Intent.ActionView, uri);
            StartActivity(intent);
        };

        return view;
    }
    private void SetProgressBarVisibility(bool isVisible)
    {
        MainActivity.mainActivity.RunOnUiThread(() =>
        {
            progressBar.Visibility = isVisible ? ViewStates.Visible : ViewStates.Gone; // 隐藏进度条
            statusText.Text = isVisible ? "正在安装..." : "安装完成！"; // 根据状态修改文本
            descriptionText.Text = isVisible ? "请稍候..." : "请启动游戏"; // 根据状态修改描述文本
        });
    }
    // 更新进度条的进度和文本

    public static string GetPrivateStoragePath()
    {
        return MainActivity.mainActivity.GetExternalFilesDirs((string?)null)[0].ToString();
    }
   
    
    private async void StartInstallationProcess()
    {
        string packageName = "com.chucklefish.stardewvalley";


        if (IsAppInstalled(packageName))
        {



            await ExtractAndSaveApk(packageName);

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

        AndroidX.AppCompat.App.AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.mainActivity);

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


    private void ShowErrorDialog(string title, string message)
    {
        AlertDialog.Builder dialog = new AlertDialog.Builder(MainActivity.mainActivity);
        dialog.SetTitle(title);  // 设置标题
        dialog.SetMessage(message);  // 设置错误消息

        // 添加确认按钮
        dialog.SetPositiveButton("确认", (sender, e) => { });

        // 显示对话框
        dialog.Show();
    }

 
    // Extract APK and save it to the private storage
    private async Task ExtractAndSaveApk(string packageName)
    {
        // 获取 APK 和 split_content.apk 的路径
        string apkPath = GetApkPathByPackageName(packageName);
        string splitContentApkPath = GetSplitContentApkPath(packageName);

        // 如果找到了 APK 文件或 split_content.apk 文件
        if (!string.IsNullOrEmpty(apkPath) || !string.IsNullOrEmpty(splitContentApkPath))
        {
            // 更新 UI 显示提取APK的进度
            MainActivity.mainActivity.RunOnUiThread(() =>
            {
                progressBar.Visibility = ViewStates.Visible;
                progressBar.Progress = 0;
                statusText.Text = "正在提取游戏文件...";
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
                    await CopyFileAsync(apkPath, destinationPath);
                    UpdateProgress(25, "APK文件已复制...");
                }

                // 如果找到 split_content.apk 文件，将其复制到私有存储
                if (!string.IsNullOrEmpty(splitContentApkPath) && File.Exists(splitContentApkPath))
                {
                    string splitContentDestinationPath = Path.Combine(privatePath, "split_content.apk");
                    await CopyFileAsync(splitContentApkPath, splitContentDestinationPath);
                    UpdateProgress(35, "split_content.apk 文件已提取...");
                }

                // 提取 APK 内容（包括主 APK 和附属 APK）
                UpdateProgress(50, "正在提取APK内容...");
                await ExtractAssetsContentToPrivateStorage.ExtractApkAssetsContentAsync();

                UpdateProgress(60, "游戏内容提取完成...");

                // 解压 APK 内部资源
                await App.UncompressFromAPKAsync(destinationPath, "assemblies/");
                UpdateProgress(70, "APK内容已解压...");

                // 解压其他资产
                UpdateProgress(75, "正在安装SMAPI...");
                await Unpacker.UnpackAllFilesFromAssets(MainActivity.mainActivity, "smapi-internal.zip");
                await Unpacker.UnpackAllFilesFromAssets(MainActivity.mainActivity, "dotnet.zip");
                UpdateProgress(85, "SMAPI已解压...");
                await Task.Delay(500);  
                // 移动建筑物文件
                await MoveBuildings.MoveBuildingsFileToTargetDirectory(MainActivity.mainActivity);

                // 更新进度为 100% 完成安装
                UpdateProgress(100, "SMAPI安装完成！");
                SetProgressBarVisibility(true);
                descriptionText.Text = "请启动游戏";
                statusText.Text = "安装完成！"; // 根据状态修改文本
                MainActivity.mainActivity.RunOnUiThread(() =>
                {
                
                });
            }
            catch (IOException e)
            {
                // 显示错误信息弹窗
                ShowErrorDialog("文件复制失败", e.Message);
            }
        }
        else
        {
            // 未找到APK文件或split_content.apk
            Toast.MakeText(MainActivity.mainActivity, "未找到APK文件或split_content.apk", ToastLength.Long).Show();
        }
    }
    private static async Task CopyFileAsync(string sourceFilePath, string destinationFilePath)
    {
        // 确保目标文件的目录存在
        string directoryPath = Path.GetDirectoryName(destinationFilePath);
        if (!Directory.Exists(directoryPath))
        {
            Directory.CreateDirectory(directoryPath);
        }

        // 使用 FileStream 进行异步复制
        using (var sourceStream = new FileStream(sourceFilePath, FileMode.Open, FileAccess.Read))
        using (var destinationStream = new FileStream(destinationFilePath, FileMode.Create, FileAccess.Write))
        {
            await sourceStream.CopyToAsync(destinationStream);  // 异步复制
        }
    }


    // 更新进度的函数
    private void UpdateProgress(int progress, string message)
    {
        // 确保在主线程中更新 UI
        MainActivity.mainActivity.RunOnUiThread(() =>
        {
            progressBar.Progress = progress;  // 更新进度条的进度
            statusText.Text = message;  // 更新状态文本
            descriptionText.Text = $"{progress}%";  // 更新进度百分比显示
        });
    }

    // 获取 APK 路径
    private string GetApkPathByPackageName(string packageName)
    {
        string apkPath = string.Empty;

        try
        {
            // 获取 PackageManager 实例
            PackageManager packageManager = Activity.PackageManager;
            ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);
            apkPath = appInfo.SourceDir;  // 获取应用的 APK 路径
        }
        catch (PackageManager.NameNotFoundException ex)
        {
            // 弹出错误对话框，应用未找到
            ShowErrorDialog("应用未找到", "应用包名无效或应用未安装: " + ex.Message);
        }

        return apkPath;
    }

    // 获取 split_content.apk 路径
    private string GetSplitContentApkPath(string packageName)
    {
        string splitContentApkPath = string.Empty;

        try
        {
            // 获取 PackageManager 实例
            PackageManager packageManager = Activity.PackageManager;
            ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);
            string appSourceDir = appInfo.SourceDir; // 获取应用的 APK 路径

            // 假设 split_content.apk 与主 APK 文件位于同一目录
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







    private void LaunchGame()
    {
        descriptionText.Text = "启动游戏中...";
        progressBar.Visibility = ViewStates.Visible;
        // 启动游戏逻辑
        SetProgressBarVisibility(true);
        Intent intent = new Intent(MainActivity.mainActivity, typeof(SMAPIMainActivity));

        StartActivity(intent); // 启动游戏
        //666

    }



    // Extract assets content to the private storage



}
