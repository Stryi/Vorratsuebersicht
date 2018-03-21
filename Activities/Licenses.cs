using System;
using System.IO;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Webkit;
using Android.Graphics;

namespace VorratsUebersicht
{
    [Activity(Label = "Open-Source-Lizenzen")]
    public class Licenses : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.Licenses);

            WebView textView = FindViewById<WebView>(Resource.Id.Licenses_Text);
            textView.LoadUrl("file:///android_asset/Licenses.html");
        }
    }
}