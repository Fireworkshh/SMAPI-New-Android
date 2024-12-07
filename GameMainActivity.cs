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
using Color = Microsoft.Xna.Framework.Color;
using SMAPIStardewValley; // For AlertDialog

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
            HarmonyPatch_OptimizeMonsterCode.HarmonyPatch();

            Console.WriteLine("GameMainActivity OnCreate complete.");
                base.OnCreate(bundle);
            

    
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

                throw;

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
