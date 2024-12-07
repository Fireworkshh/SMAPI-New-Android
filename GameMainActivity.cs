using Android.Content.PM;
using Android.Views;
using HarmonyLib;
using SMAPIStardewGame;
using StardewModdingAPI.Framework;
using StardewValley;
using StardewValley.Mobile;
using System.Reflection;
using System.IO;
using Android.Widget;
using Xamarin.Android.Tools.DecompressAssemblies;
using Android.App;
using Android.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Microsoft.Xna.Framework;

using SMAPIStardewValley;
using System.Text;
using Android.Content.Res; // For AlertDialog

namespace StardewModdingAPI
{
    [Activity(Label = "SMAPIStardew Valley", Icon = "@drawable/ic_launcher", Theme = "@style/Theme.Splash", Name = "com.chucklefish.stardewvalley.GameMainActivity", MainLauncher = false, AlwaysRetainTaskState = true, LaunchMode = LaunchMode.SingleInstance, ScreenOrientation = ScreenOrientation.SensorLandscape, ConfigurationChanges = (ConfigChanges.Keyboard | ConfigChanges.KeyboardHidden | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.ScreenSize | ConfigChanges.UiMode))]
    public class GameMainActivity : MainActivity
    {
        private const int RequestStoragePermission = 1;
        public static string externalFilesDir;
        public static string logFilePath;
        public static MainActivity mainActivity;
        public static GameMainActivity gameMainActivity;
        public static CommandConsole commandConsole;
        protected override void OnCreate(Bundle bundle)
        {
            mainActivity = this;
            gameMainActivity = this;
            externalFilesDir = GetExternalFilesDirs((string?)null)[0].ToString();

        
            logFilePath = System.IO.Path.Combine(externalFilesDir, "applog.txt");

         
            LogManager.SetupLog(logFilePath);
            PermissionsManager.RequestStoragePermissions(this, RequestStoragePermission);

            
            
       
            
                AppDomain.CurrentDomain.AssemblyResolve += AssemblyResolve.HandleAssemblyResolve;
                PatchManager.ApplyAllPatches();

          
            OnCreatePartTwoPatches();
            ApplyPatch();
            HarmonyPatch_OptimizeMonsterCode.HarmonyPatch();

            Console.WriteLine("GameMainActivity OnCreate complete.");
                base.OnCreate(bundle);
            

    
        }

        public static void ApplyPatch()
        {
            Harmony harmony = new Harmony("com.example.patch");
            var original = AccessTools.Method(typeof(MainActivity), "CheckStorageMigration");
            var postfix = new HarmonyMethod(typeof(GameMainActivity), nameof(CheckStorageMigrationPostfix));
            harmony.Patch(original, postfix: postfix);
        }
        public static void CheckStorageMigrationPostfix(bool __result)
        {
            if (!__result)
            {
                mainActivity.IsDoingStorageMigration = true;

                // 创建AlertDialog
                AlertDialog.Builder builder = new AlertDialog.Builder(mainActivity);
                builder.SetTitle("SMAPI4.1.8 Android");

                // 读取applog.txt文件内容
                string logFilePath = System.IO.Path.Combine(GameMainActivity.externalFilesDir, "applog.txt");
                string[] logLines = File.ReadAllLines(logFilePath);  // 读取所有行

                // 创建一个TextView来显示日志内容
                TextView logTextView = new TextView(mainActivity)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent),
                    TextSize = 14f,
                };

                // 定义 TextView 的颜色 (例如，黄色)
                Android.Graphics.Color androidColor = Android.Graphics.Color.Yellow; // 使用 Android.Graphics.Color.Yellow，或者选择任何你想要的颜色
                int[][] states = new int[][] {
            new int[] { Android.Resource.Attribute.StateEnabled },  // 默认状态
        };
                int[] colors = new int[] { Android.Graphics.Color.Yellow };  // 直接使用 Color.Yellow，而不需要调用 ToArgb()

                // 创建 ColorStateList
                ColorStateList colorStateList = new ColorStateList(states, colors);

                // 设置 TextView 的颜色
                logTextView.SetTextColor(colorStateList);

                // 设置TextView的内容
                StringBuilder logText = new StringBuilder();
                foreach (var line in logLines)
                {
                    logText.AppendLine(line);
                }
                logTextView.Text = logText.ToString();

                // 创建一个 ScrollView 来包含 TextView，使其能够滚动
                ScrollView scrollView = new ScrollView(mainActivity);
                scrollView.LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
                scrollView.AddView(logTextView);  // 将 TextView 添加到 ScrollView 中

                // 将 ScrollView 设置为 AlertDialog 的视图
                builder.SetView(scrollView);

                // 设置按钮
                string continueText = "确定";
                string cancelText = "取消";

                builder.SetPositiveButton(continueText, delegate
                {
                    mainActivity.IsDoingStorageMigration = false;
                });
                builder.SetNegativeButton(cancelText, delegate
                {
                    mainActivity.IsDoingStorageMigration = false;
                });

                // 禁止对话框关闭
                builder.SetCancelable(false);

                // 显示AlertDialog
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();

                __result = false;
            }
        }

        public static void OnCreatePartTwoPatches()
        {
            Harmony harmony = new Harmony("com.example.patch");
            MethodInfo original = AccessTools.Method(typeof(MainActivity), "OnCreatePartTwo");
            HarmonyMethod prefix = new HarmonyMethod(typeof(GameMainActivity), "OnCreatePartTwo_Prefix");
            harmony.Patch(original, prefix);
            Console.WriteLine("Prefix applied!");
        }

        public static bool OnCreatePartTwo_Prefix()
        {
            try
            {

          

             
                MobileDisplay.SetupDisplaySettings();
                mainActivity.SetPaddingForMenus();
                Program.Main();
                SCore core = new SCore(Constants.DefaultModsPath, false, (bool?)false);

                core.RunInteractively();

             

                TargetFramework.TargetFrameworkPatch();
                mainActivity.SetContentView((View)core.Game.Services.GetService(typeof(View)));
                GameRunner.instance = core.Game;
             
             
                mainActivity._game1 = core.Game.gamePtr;

               
                core.Game.Run();
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("OnCreatePartTwo_Prefix 发生了错误：");
                Console.WriteLine("异常消息: " + ex.Message);
                Console.WriteLine("堆栈跟踪: " + ex.StackTrace);

             

                return false;
            }
        }

        

        // 权限请求结果回调
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestStoragePermission)
            {
                if (grantResults.Length > 0 && grantResults.All(result => result == Permission.Granted))
                {
                    // 所有权限已授予，继续操作
                    Toast.MakeText(this, "所有权限已授予，继续操作", ToastLength.Short).Show();
                }
                else
                {
                    // 一些权限被拒绝
                    Toast.MakeText(this, "需要完整存储权限和应用查询权限才能继续", ToastLength.Long).Show();
                }
            }
        }
    }
}
