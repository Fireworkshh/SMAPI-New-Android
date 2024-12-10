using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using System.Collections.Generic;
using System.IO;
using Android.Provider;
using ICSharpCode.SharpZipLib.Zip;
using AndroidX.Core.App;
using AndroidX.Core.Content;
using SMAPI_Installation;
using System.IO.Compression;

namespace SMAPIStardewValley
{
    public class ModFragment : AndroidX.Fragment.App.Fragment
    {
        private TextView infoOutput;
        private Button importButton;
        private Button deleteButton;
        private ListView modListView;
        private List<string> modList;
        private ArrayAdapter<string> modAdapter;
        private ProgressBar progressBar;
        private const int PICK_FILE_REQUEST = 1; // 文件选择请求码
        private const string MODS_DIR = "Mods"; // 模组存储路径

        public static ModFragment commandFragment { get; private set; }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            commandFragment = this;
            View view = inflater.Inflate(Resource.Layout.fragment_mod, container, false);
            view.SetBackgroundColor(Android.Graphics.Color.White); // 设置背景颜色

        /*    // Find views
            infoOutput = view.FindViewById<TextView>(Resource.Id.infoOutput);
            importButton = view.FindViewById<Button>(Resource.Id.importButton);
            deleteButton = view.FindViewById<Button>(Resource.Id.deleteButton);
            modListView = view.FindViewById<ListView>(Resource.Id.modListView);
            progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar); // 获取进度条控件

            // Initialize mod list
            modList = new List<string>();
            modAdapter = new ArrayAdapter<string>(Activity, Android.Resource.Layout.SimpleListItem1, modList);
            modListView.Adapter = modAdapter;

            // Setup buttons click events
            importButton.Click += ImportMod;
            deleteButton.Click += DeleteMod;

            // Load installed mods
            LoadMods();

            // Adjust soft input mode
            Activity.Window.SetSoftInputMode(SoftInput.AdjustPan);
            */

            return view;
        }

        // Load mods from storage (e.g., a folder where the mods are stored)
  
    }
}
