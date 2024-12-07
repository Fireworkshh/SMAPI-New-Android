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

            // ��ȡ UI �ؼ�����
            progressBar = FindViewById<ProgressBar>(Resource.Id.progressBar);
            statusText = FindViewById<TextView>(Resource.Id.statusText);
            descriptionText = FindViewById<TextView>(Resource.Id.descriptionText);
            installButton = FindViewById<Button>(Resource.Id.installButton);
            launchButton = FindViewById<Button>(Resource.Id.launchButton); 
            launchButton.Visibility = ViewStates.Invisible; 
            SetProgressBarVisibility(false);
            infoText.Visibility = ViewStates.Visible;
            string text = "1. ���ڹȸ�������ϷȻ������װ����װ���������ɡ�\n" +
                       "2. �������ô�ϵͳ�Ż�����Ҫ�ر�ϵͳ�Ż�����������޷���ȡ��װ����\n" +
                       "3. ģ�����/storage/emulated/0/Android/data/app.SMAPIStardew/Mods��\n" +
                       "4. ���κ��������Ⱥ 985754557 ѯ�ʡ�\n" +
                       "5. ��Ŀ��ַ�Ѿ���Դ https://github.com/Fireworkshh/SMAPI-New-Android��\n" +
                       "6. �������� UP ����è�����Ŷ��";

           
            SpannableString spannableString = new SpannableString(text);

         
            int start = text.IndexOf("���κ��������Ⱥ 985754557 ѯ��");
            int end = start + "���κ��������Ⱥ 985754557 ѯ��".Length;
            spannableString.SetSpan(new URLSpan("http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=4guX1RqKVQE7nawKcsnOZ477ntb2nrY3&authKey=oTUbE%2BI4fVMghqGJ4rYwAjTzoJ4d2fI8ixDcsNF6S4NYOTkJ63iBrRGhZaB2XAkH&noverify=0&group_code=781588105"), start, end, SpanTypes.ExclusiveExclusive);

            start = text.IndexOf("�������� UP ����è�����Ŷ");
            end = start + "�������� UP ����è�����Ŷ".Length;
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
        private string GetApkPathByPackageName(string packageName)
        {
            try
            {
                // ��ȡPackageManagerʵ��
                PackageManager packageManager = PackageManager;

                // ʹ�ð�����ȡӦ�õ�ApplicationInfo
                ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);

                // ��ȡӦ�õ�APK·��
                string apkPath = appInfo.SourceDir;

                return apkPath;
            }
            catch (PackageManager.NameNotFoundException ex)
            {
                // Ӧ�ð�����Ч��Ӧ��δ��װ�������쳣�����ؿ��ַ���
                Toast.MakeText(this, "Ӧ��δ�ҵ�: " + ex.Message, ToastLength.Long).Show();
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
                    statusText.Text = "������ȡAPK...";
                    descriptionText.Text = "���Ժ�...";
                });

                // ��ȡ˽�д洢·��
                string privatePath = GetPrivateStoragePath();

                // ����Ŀ��·���Ա��� APK �ļ�
                string destinationPath = Path.Combine(privatePath, "stardewvalley.apk");

                try
                {
                    // ����ҵ��� APK �ļ������临�Ƶ�˽�д洢
                    if (!string.IsNullOrEmpty(apkPath))
                    {
                        File.Copy(apkPath, destinationPath, true); // true ��ʾ�����Ѵ����ļ�
                    }




                    //666   if (File.Exists(splitContentapk))
                    {


                        string splitContentDestinationPath = Path.Combine(privatePath, "split_content.apk");

                        if (File.Exists(splitContentApkPath))
                        {
                            File.Copy(splitContentApkPath, splitContentDestinationPath, true);
                        }


                        UpdateProgress(25, "���ڰ�װ��ѹAPK...");

                        // ��ȡ APK ����
                        UpdateProgress(50, "������ȡ��Ϸ����...");
                        await ExtractAssetsContentToPrivateStorage.ExtractAssetsContentAsync();

                        string splitContentapks = Path.Combine(privatePath, "split_content.apk");

                        if (!string.IsNullOrEmpty(splitContentapks))
                        {

                            await ExtractAssetsContentToPrivateStorage.ExtractapksAssetsContentAsync();
                        }
                        // ��ѹ APK �ڲ���Դ
                        App.UncompressFromAPK(destinationPath, "assemblies/");

                        // ��ѹ�����ʲ�
                        UpdateProgress(75, "���ڰ�װSMAPI...");
                        Unpacker.UnpackAllFilesFromAssets(this, "smapi-internal.zip");

                        // �ƶ��������ļ�
                        MoveBuildings.MoveBuildingsFileToTargetDirectory(this);

                        // ���½���Ϊ 100% ��ɰ�װ
                        UpdateProgress(100, "SMAPI��װ��ɣ�");

                        RunOnUiThread(() =>
                        {
                            descriptionText.Text = "��������Ϸ";
                            // ��ʾ��װ�����Ϣ
                            if (IsSMAPIInstalled())
                            {
                                installButton.Visibility = ViewStates.Invisible;

                                statusText.Visibility = ViewStates.Visible;

                                launchButton.Visibility = ViewStates.Visible; // Ĭ������������ť
                                ShowLaunchButton();
                            }
                        });
                    }
                }
                catch (IOException e)
                {
                    //Toast.MakeText(this, "�ļ�����ʧ��: " + e.Message, ToastLength.Long).Show();
                    throw;

                }
            }
            else
            {
                Toast.MakeText(this, "δ�ҵ�APK�ļ���split_content.apk", ToastLength.Long).Show();
            }
        }

        // ��ȡ�Ѱ�װӦ�õ�·����split_content.apk·��
        private string GetSplitContentApkPath(string packageName)
        {
            string splitContentApkPath = string.Empty;

            try
            {
                // ��ȡPackageManagerʵ��
                PackageManager packageManager = PackageManager;
                ApplicationInfo appInfo = packageManager.GetApplicationInfo(packageName, 0);
                string appSourceDir = appInfo.SourceDir; // ��ȡӦ�õ�APK·��

                // ����split_content.apk����APK�ļ�λ��ͬһĿ¼
                string directory = Path.GetDirectoryName(appSourceDir);
                splitContentApkPath = Path.Combine(directory, "split_content.apk");
            }
            catch (PackageManager.NameNotFoundException ex)
            {
                // Ӧ�ð�����Ч��Ӧ��δ��װ
                Toast.MakeText(this, "Ӧ��δ�ҵ�: " + ex.Message, ToastLength.Long).Show();
            }

            return splitContentApkPath;
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
    
        // Get the application's private storage path
        public static string GetPrivateStoragePath()
        {
            return mainActivity.GetExternalFilesDirs((string?)null)[0].ToString();
        }
    }
}
