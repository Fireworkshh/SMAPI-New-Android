using Android.Content;
using Android.OS;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using AndroidX.Fragment.App;

using System;

public class SettingFragment : AndroidX.Fragment.App.Fragment
{


    public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
    {
        View view = inflater.Inflate(SMAPIStardewValley.Resource.Layout.fragment_setting, container, false);

    
       

        return view;
    }

  
}
