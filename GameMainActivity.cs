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
using Color = Microsoft.Xna.Framework.Color; // For AlertDialog

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

            Path();
            OnCreatePartTwoPatches();
   

            Console.WriteLine("GameMainActivity OnCreate complete.");
                base.OnCreate(bundle);
            

    
        }
        private void SetupCommandConsole()
        {
            // 获取 MonoGame 的 SpriteBatch 和字体对象
        
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

          

                // 让控制台可见
                
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

                throw;

                return false;
            }
        }

        public  void Path()
        {
            Harmony harmony = new Harmony("com.example.patch");

           
            Assembly externalAssembly = Assembly.Load("FarmTypeManager"); // 加载外部程序集
            Type harmonyPatchType = externalAssembly.GetType("FarmTypeManager.ModEntry+HarmonyPatch_OptimizeMonsterCode");

            if (harmonyPatchType != null)
            {
                // 获取 ApplyPatch 方法
                MethodInfo applyPatchMethod = harmonyPatchType.GetMethod("ApplyPatch");

                if (applyPatchMethod != null)
                {
                    // 创建并应用 Prefix 补丁
                    harmony.Patch(
                        original: applyPatchMethod,
                        prefix: new HarmonyMethod(typeof(GameMainActivity), nameof(HarmonyPrefixForApplyPatch))
                    );

                    Console.WriteLine("已阻止外部程序集 HarmonyPatch_OptimizeMonsterCode.ApplyPatch 方法的执行。");
                }
                else
                {
                    Console.WriteLine("未找到 ApplyPatch 方法！");
                }
            }
            else
            {
                Console.WriteLine("未找到 HarmonyPatch_OptimizeMonsterCode 类！");
            }
        }
        public static bool HarmonyPrefixForApplyPatch()
        {
            Console.WriteLine("已阻止 Harmony 补丁 'HarmonyPatch_OptimizeMonsterCode.ApplyPatch' 的执行");
            return false; // 返回 false 来阻止原方法执行
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
