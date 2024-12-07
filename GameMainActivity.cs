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
using Android.Content.Res;
using Android.Util; // For AlertDialog

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

         
                AlertDialog.Builder builder = new AlertDialog.Builder(mainActivity);
                builder.SetTitle("SMAPI4.1.8 Android");

            
                TextView logTextView = new TextView(mainActivity)
                {
                    LayoutParameters = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent),
                    TextSize = 14f,
                };

           
                Android.Graphics.Color androidColor = Android.Graphics.Color.Yellow; 
                int[][] states = new int[][] {
            new int[] { Android.Resource.Attribute.StateEnabled },  // 默认状态
        };
                int[] colors = new int[] { Android.Graphics.Color.Yellow }; 

           
                ColorStateList colorStateList = new ColorStateList(states, colors);

        
                logTextView.SetTextColor(colorStateList);

             
                ScrollView scrollView = new ScrollView(mainActivity);
                scrollView.LayoutParameters = new LinearLayout.LayoutParams(
                    LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.MatchParent);
                scrollView.AddView(logTextView);  

              
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

   
                builder.SetCancelable(false);

          
                AlertDialog alertDialog = builder.Create();
                alertDialog.Show();

           
                Window window = alertDialog.Window;
                if (window != null)
                {
              
                    DisplayMetrics displayMetrics = new DisplayMetrics();
                    mainActivity.WindowManager.DefaultDisplay.GetMetrics(displayMetrics);

               
                    window.SetLayout(displayMetrics.WidthPixels, displayMetrics.HeightPixels);
                }

               
                  
                    string logFilePath = System.IO.Path.Combine(GameMainActivity.externalFilesDir, "applog.txt");

                
                    if (File.Exists(logFilePath))
                    {
                        string[] logLines = File.ReadAllLines(logFilePath); 

                
                        StringBuilder logText = new StringBuilder();
                        foreach (var line in logLines)
                        {
                            logText.AppendLine(line);
                        }

                    
                        mainActivity.RunOnUiThread(() =>
                        {
                            logTextView.Text = logText.ToString(); 

                      
                            scrollView.FullScroll(FocusSearchDirection.Down);
                        });
                    }
                    else
                    {
              
                        mainActivity.RunOnUiThread(() =>
                        {
                            logTextView.Text = "日志文件不存在。";
                        });
                    }
                

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

             

                dnlibModify.TargetFrameworkPatch();
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

        

  
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestStoragePermission)
            {
                if (grantResults.Length > 0 && grantResults.All(result => result == Permission.Granted))
                {
                    
                    Toast.MakeText(this, "所有权限已授予，继续操作", ToastLength.Short).Show();
                }
                else
                {
        
                    Toast.MakeText(this, "需要完整存储权限", ToastLength.Long).Show();
                }
            }
        }
    }
}
