
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

            // ȷ��͸��״̬��
            Window.AddFlags(WindowManagerFlags.KeepScreenOn);
            Window.SetFlags(WindowManagerFlags.TranslucentStatus, WindowManagerFlags.TranslucentStatus);





            base.OnCreate(savedInstanceState);
            mainActivity = this;

            ViewGroup root = FindViewById<ViewGroup>(Android.Resource.Id.Content); // ��ȡ����ͼ

            SetContentView(Resource.Layout.activity_main); // ����������ͼ

            Window.SetSoftInputMode(SoftInput.AdjustPan);

            RequestStoragePermissions();


            var bottomNavigationView = FindViewById<BottomNavigationView>(Resource.Id.bottom_navigation);
            bottomNavigationView.ItemSelected += OnNavigationItemSelected; // �޸�Ϊ ItemSelected
            bottomNavigationView.SelectedItemId = Resource.Id.navigation_game; // ����Ĭ��ѡ����Ϊ��������Ϸ��

            bottomNavigationView.Visibility = ViewStates.Visible; // Ensure it's always visible

            // Ĭ�ϼ�����ҳ
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
            Toast.MakeText(this, "�洢Ȩ�������裬��������", ToastLength.Short).Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            if (requestCode == RequestCodeStoragePermission)
            {
                if (grantResults.Length > 0 && grantResults.All(result => result == Permission.Granted))
                {
                    // All permissions granted, continue
                    Toast.MakeText(this, "����Ȩ�������裬��������", ToastLength.Short).Show();
                }
                else
                {
                    // Some permissions were denied
                    ShowErrorDialog("Ȩ������ʧ��", "��Ҫ�����洢Ȩ�޺�Ӧ�ò�ѯȨ�޲��ܼ���");
                }
            }
        }
        private void ShowErrorDialog(string title, string message)
        {
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(title);  // ���ñ���
            dialog.SetMessage(message);  // ���ô�����Ϣ

            // ���ȷ�ϰ�ť
            dialog.SetPositiveButton("ȷ��", (sender, e) => { });

            // ��ʾ�Ի���
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
                        // Ȩ�����裬��������
                        RequestStoragePermissions();
                    }
                    else
                    {
                        // Ȩ��δ���裬�����û�
                        Toast.MakeText(this, "�洢Ȩ��δ���裬���ֶ�����Ȩ��", ToastLength.Long).Show();
                    }
                }
                else
                {
                    // ��������Ȩ�޵Ľ��
                    if (CheckSelfPermission(Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted)
                    {
                        RequestStoragePermissions();
                    }
                    else
                    {
                        Toast.MakeText(this, "�洢Ȩ��δ����", ToastLength.Long).Show();
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
