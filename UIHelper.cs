using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Android.Content;
using Google.Android.Material.FloatingActionButton;
using AndroidX.RecyclerView.Widget;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using SMAPI_Installation;


public static class UIHelper
{
    // 创建工具栏
    public static Toolbar CreateToolbar(MainActivity activity, int toolbarResource)
    {
        var toolbar = activity.FindViewById<Toolbar>(toolbarResource); // 查找工具栏视图
        activity.SetSupportActionBar(toolbar); // 设置为支持的工具栏
        return toolbar; // 返回工具栏实例
    }

    // 创建浮动按钮
    public static FloatingActionButton CreateFAB(Activity activity, int fabResource, View.IOnClickListener listener)
    {
        var fab = activity.FindViewById<FloatingActionButton>(fabResource); // 查找浮动按钮
        fab.Click += (s, e) => listener.OnClick(fab); // 注册点击事件处理
        return fab; // 返回浮动按钮实例
    }

    // 创建进度条
    public static ProgressBar CreateProgressBar(Activity activity)
    {
        var progressBar = new ProgressBar(activity)
        {
            Indeterminate = true // 设置为不确定状态
        };
        var layoutParams = new FrameLayout.LayoutParams(
            ViewGroup.LayoutParams.WrapContent,
            ViewGroup.LayoutParams.WrapContent,
            GravityFlags.Center // 使进度条在父布局中居中
        );
        progressBar.LayoutParameters = layoutParams; // 设置布局参数
        return progressBar; // 返回进度条实例
    }

    // 创建文本视图
    public static TextView CreateTextView(Activity activity, string text, int textSize, int color, GravityFlags gravity)
    {
        var textView = new TextView(activity)
        {
            Text = text, // 设置文本
            TextSize = textSize, // 设置文本大小
            LayoutParameters = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.WrapContent,
                ViewGroup.LayoutParams.WrapContent) // 设置布局参数
        };
        textView.SetTextColor(new Android.Graphics.Color(color)); // 设置文本颜色
        textView.Gravity = gravity; // 设置重力(对齐方式)
        return textView; // 返回文本视图实例
    }

    // 创建按钮
    public static Button CreateButton(Activity activity, string text, View.IOnClickListener listener)
    {
        var button = new Button(activity) { Text = text }; // 创建按钮并设置文本
        button.Click += (s, e) => listener.OnClick(button); // 注册点击事件处理
        return button; // 返回按钮实例
    }

    // 创建编辑文本框
    public static EditText CreateEditText(Activity activity, string hint)
    {
        var editText = new EditText(activity) { Hint = hint }; // 创建编辑文本框并设置提示文本
        return editText; // 返回编辑文本框实例
    }

    // 创建 RecyclerView
    public static RecyclerView CreateRecyclerView(Activity activity)
    {
        var recyclerView = new RecyclerView(activity);
        var layoutManager = new LinearLayoutManager(activity); // 创建线性布局管理器
        recyclerView.SetLayoutManager(layoutManager); // 设置布局管理器
        return recyclerView; // 返回 RecyclerView 实例
    }

    // 创建图片视图
    public static ImageView CreateImageView(Activity activity, int imageResource)
    {
        var imageView = new ImageView(activity);
        imageView.SetImageResource(imageResource); // 设置图片资源
        return imageView; // 返回图片视图实例
    }

    // 注册按钮点击事件的方法
    public static void RegisterButtonClick(Button button, View.IOnClickListener listener)
    {
        button.Click += (s, e) => listener.OnClick(button); // 注册点击事件处理
    }

    // 注册浮动按钮的点击事件
    public static void RegisterButtonClick(FloatingActionButton fab, View.IOnClickListener listener)
    {
        fab.Click += (s, e) => listener.OnClick(fab); // 注册点击事件处理
    }
}
