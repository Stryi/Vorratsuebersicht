using System;
using System.IO;

using Android.App;
using Android.OS;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "Open-Source-Lizenzen")]
    public class Licenses : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.Licenses);

            string text = string.Empty;

            using (var br = new StreamReader(Application.Context.Assets.Open("Licenses.txt")))
            {
                text += br.ReadToEnd();
            }

            TextView textView = FindViewById<TextView>(Resource.Id.Licenses_Text);
            textView.Text = text;
        }
    }
}