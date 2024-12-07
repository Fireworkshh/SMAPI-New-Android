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
        private Button launchButton; // ����������Ϸ��ť
        private TextView infoText; // �����ı��ؼ�
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            SetContentView(Resource.Layout.activity_main);  // ���ز����ļ�

            mainActivity = this;
            infoText = FindViewById<TextView>(Resource.Id.infoText); // ��ȡ��Ϣ�ı��ؼ�

            // ��ȡ UI �ؼ�����
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            statusText = FindViewById<TextView>(Resource.Id.statusText);
            descriptionText = FindViewById<TextView>(Resource.Id.descriptionText);
            installButton = FindViewById<Button>(Resource.Id.installButton);
            launchButton = FindViewById<Button>(Resource.Id.launchButton); // ��ȡ������ť
            launchButton.Visibility = ViewStates.Invisible; // Ĭ������������ť
            SetProgressBarVisibility(false);
            infoText.Visibility = ViewStates.Visible;
            string text = "1. ���ڹȸ�������ϷȻ������װ����װ���������ɡ�\n" +
                       "2. �������ô�ϵͳ�Ż�����Ҫ�ر�ϵͳ�Ż�����������޷���ȡ��װ����\n" +
                       "3. ģ�����/storage/emulated/0/Android/data/app.SMAPIStardew/Mods��\n" +
                       "4. ���κ��������Ⱥ 985754557 ѯ�ʡ�\n" +
                       "5. ��Ŀ��ַ�Ѿ���Դ https://github.com/Fireworkshh/SMAPI-New-Android��\n" +
                       "6. �������� UP ����è�����Ŷ��";

            // ʹ�� SpannableString ���ó�����
            SpannableString spannableString = new SpannableString(text);

            // ���õڶ��к͵�����Ϊ������
            int start = text.IndexOf("���κ��������Ⱥ 985754557 ѯ��");
            int end = start + "���κ��������Ⱥ 985754557 ѯ��".Length;
            spannableString.SetSpan(new URLSpan("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=4guX1RqKVQE7nawKcsnOZ477ntb2nrY3&authKey=oTUbE%2BI4fVMghqGJ4rYwAjTzoJ4d2fI8ixDcsNF6S4NYOTkJ63iBrRGhZaB2XAkH&noverify=0&group_code=781588105"), start, end, SpanTypes.ExclusiveExclusive);

            start = text.IndexOf("�������� UP ����è�����Ŷ");
            end = start + "�������� UP ����è�����Ŷ".Length;
            spannableString.SetSpan(new URLSpan("https://b23.tv/zVL6x1g"), start, end, SpanTypes.ExclusiveExclusive);

            // ���� TextView ���ı�
            infoText.TextFormatted = spannableString;

            // ���ó����ӵ��֧��
            infoText.MovementMethod = LinkMovementMethod.Instance;


            // �趨��ť����¼�

            if (IsSMAPIInstalled())
            {
                installButton.Visibility = ViewStates.Gone;

                statusText.Visibility = ViewStates.Visible;

                launchButton.Visibility = ViewStates.Visible; // Ĭ������������ť
                ShowLaunchButton(); 


            }
            else
            {
                statusText.Visibility = ViewStates.Visible;
               
                statusText.Text = "�밲װSMAPI...";
                installButton.Click += async (sender, e) =>
                {

                    SetProgressBarVisibility(true);


                    StartInstallationProcess();
                 
                };


            }


         


            // ����洢Ȩ��
            RequestPermissions(new string[] { Android.Manifest.Permission.WriteExternalStorage }, RequestCodeStoragePermission);
        }
        private  async void  StartInstallationProcess()
        {
            string packageName = "com.chucklefish.stardewvalley";

            // �����Ϸ�Ƿ��Ѱ�װ
            if (IsAppInstalled(packageName))
            {

            
                // ��Ϸ�Ѱ�װ����ȡ������APK
                await  ExtractAndSaveApk(packageName);
             
            }
            else
            {
                // ��Ϸδ��װ����ʾ�û���װ
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
            // �����Ի��򹹽���
            AlertDialog.Builder dialog = new AlertDialog.Builder(this);

            // ���öԻ�����Ϣ
            dialog.SetMessage("��Ϸδ��װ���Ƿ�������װ��");

            // ���á���װ����ť���������ת�� Google Play �̵�
            dialog.SetPositiveButton("��װ", (sender, e) =>
            {
                // ���� Intent����ת�� Google Play �̵�
                string googlePlayUrl = "https://play.google.com/store/apps/details?id=" + packageName;
                Intent intent = new Intent(Intent.ActionView, Android.Net.Uri.Parse(googlePlayUrl));
                StartActivity(intent);

            });

            // ���á�ȡ������ť����������κβ���
            dialog.SetNegativeButton("ȡ��", (sender, e) => { });

            // ��ʾ�Ի���
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
                 //   Toast.MakeText(this, "����Ȩ�������裬��������", ToastLength.Short).Show();
                }
                else
                {
                    // Some permissions were denied
                    Toast.MakeText(this, "��Ҫ�����洢Ȩ�޺�Ӧ�ò�ѯȨ�޲��ܼ���", ToastLength.Long).Show();
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
                    statusText.Text = "������ȡAPK...";
                    descriptionText.Text = "���Ժ�...";
                });
                // Get the private storage directory
                string privatePath = GetPrivateStoragePath();

                // Create destination path for saving the APK
                string destinationPath = Path.Combine(privatePath, "stardewvalley.apk");

                try
                {
                    // Copy the APK file to private storage
                    File.Copy(apkPath, destinationPath, true); // true means overwrite if exists
                    UpdateProgress(25, "���ڰ�װ��ѹAPK...");
                  

                    UpdateProgress(50, "������ȡ��Ϸ����...");


                   ExtractAssetsContentToPrivateStorage.ExtractAssetsContentAsync(privatePath);
                    // Call a function to decompress APK and extract content
                    App.UncompressFromAPK(destinationPath, "assemblies/");

                    // Unpack additional assets

                    UpdateProgress(75, "���ڰ�װSMAPI...");
                 
                    Unpacker.UnpackAllFilesFromAssets(this, "smapi-internal.zip");
                    MoveBuildings.MoveBuildingsFileToTargetDirectory(this);
                    // Update progress to 100% after everything is done
                    UpdateProgress(100, "SMAPI��װ��ɣ�");
              
                    RunOnUiThread(() =>
                    {
                        descriptionText.Text = "��������Ϸ";
                        // Show installation complete message
                        if (IsSMAPIInstalled())
                        {
                            installButton.Visibility = ViewStates.Invisible;

                            statusText.Visibility = ViewStates.Visible;

                            launchButton.Visibility = ViewStates.Visible; // Ĭ������������ť
                            ShowLaunchButton();


                        }
                    });
                }
                catch (IOException e)
                {
                    Toast.MakeText(this, "�ļ�����ʧ��: " + e.Message, ToastLength.Long).Show();
                    throw;
                }
            }
            else
            {
                Toast.MakeText(this, "δ�ҵ�APK�ļ�", ToastLength.Long).Show();
            }
        }
        private bool IsSMAPIInstalled()
        {
            // ��ȡ˽�д洢·��
            string privateStoragePath = GetPrivateStoragePath();

            // ������ļ����Ƿ����
            string[] directoriesToCheck = new string[]
            {
        Path.Combine(privateStoragePath, "Content"),
     //   Path.Combine(privateStoragePath, "dotnet"),
        Path.Combine(privateStoragePath, "smapi-internal")
            };

            // �����ļ��У�����Ƿ����
            foreach (string directory in directoriesToCheck)
            {
                if (!Directory.Exists(directory))
                {
                    return false; // �������һ���ļ��в����ڣ�����false
                }
            }

            return true; // �����ļ��ж����ڣ�����true
        }

        private void ShowLaunchButton()
        {
           
          
      

            // ��Ӱ�ť����ʽ�͵���¼�
          
            launchButton.SetBackgroundColor(Android.Graphics.Color.Rgb(34, 193, 195));
            launchButton.SetTextColor(Android.Graphics.Color.White);
            launchButton.TextSize = 18;
            launchButton.SetPadding(30, 20, 30, 20);
            launchButton.Click += LaunchGame; // ��ӵ���¼�
        }
       
        private void LaunchGame(object sender, EventArgs e)
        {
            descriptionText.Text = "������Ϸ��...";
            progressBar.Visibility = ViewStates.Visible;
            // ������Ϸ�߼�
            SetProgressBarVisibility(true);
            Intent intent = new Intent(this, typeof(GameMainActivity));
       
                StartActivity(intent); // ������Ϸ
            
        
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
