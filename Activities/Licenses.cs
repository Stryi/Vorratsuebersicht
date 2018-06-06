using System;

using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    [Activity(Label = "Open-Source-Lizenzen")]
    public class Licenses : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.Licenses);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            WebView textView = FindViewById<WebView>(Resource.Id.Licenses_Text);
            textView.LoadUrl("file:///android_asset/Licenses.html");
        }
    }
}