
using SMAPIStardewValley;
using StardewModdingAPI;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xamarin.Android.Tools.DecompressAssemblies;
using Android.Text.Method;
using Google.Android.Material.BottomNavigation;
using AndroidX.AppCompat.App;
using Android.Views;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace SMAPI_Installation
{
    [Activity(Label = "SMAPIStardew Valley ", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        private const int RequestCodeStoragePermission = 1;
        public static MainActivity mainActivity;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.SetFlags(WindowManagerFlags.Fullscreen, WindowManagerFlags.Fullscreen);
            RequestWindowFeature(WindowFeatures.NoTitle);

            // 确保透明状态栏
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.SetFlags(WindowManagerFlags.TranslucentStatus, WindowManagerFlags.TranslucentStatus);





            base.OnCreate(savedInstanceState);
            mainActivity = this;

            ViewGroup root = FindViewById<ViewGroup>(Android.Resource.Id.Content); // 获取根视图

            SetContentView(Resource.Layout.activity_main); // 设置内容视图

            Window.SetSoftInputMode(SoftInput.AdjustPan);

            RequestStoragePermissions();


            var bottomNavigationView = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            bottomNavigationView.ItemSelected += OnNavigationItemSelected; // 修改为 ItemSelected
            bottomNavigationView.SelectedItemId = Resource.Id.navigation_game; // 设置默认选中项为“启动游戏”

            bottomNavigationView.Visibility = ViewStates.Visible; // Ensure it's always visible

            // 默认加载首页
            LoadFragment(new GameFragment());

        }



        private void OnNavigationItemSelected(object sender, BottomNavigationView.ItemSelectedEventArgs e)
        {
            AndroidX.Fragment.App.Fragment selectedFragment = e.Item.ItemId switch
            {
                Resource.Id.navigation_setting => new SettingFragment(),
                Resource.Id.navigation_game => new GameFragment(),
                Resource.Id.navigation_mod => new ModFragment(),
                _ => null
            };

            LoadFragment(selectedFragment);
        }

        private void LoadFragment(AndroidX.Fragment.App.Fragment fragment)
        {
            var transaction = SupportFragmentManager.BeginTransaction();
            transaction.Replace(Resource.Id.fragment_container, fragment);
            transaction.Commit();
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
    


        // Get APK path by package name using PackageManager

        // Get the application's private storage path
        public static string GetPrivateStoragePath()
        {
            return mainActivity.GetExternalFilesDirs((string?)null)[0].ToString();
        }
    }
}
