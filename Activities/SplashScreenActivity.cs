using System;
using System.Threading;
using Android.App;
using Android.OS;
using Android.Widget;

// Anhand von
// http://www.c-sharpcorner.com/UploadFile/1e050f/creating-splash-screen-for-android-app-in-xamarin/
// 
namespace VorratsUebersicht  
{  
    [Activity(Label="Vorratsübersicht",MainLauncher=true,Theme="@style/Theme.Splash",NoHistory=true,Icon="@drawable/ic_launcher")]  
    public class SplashScreenActivity : Activity  
    {  
        private TextView    progressText;

        protected override void OnCreate(Bundle bundle)  
        {  
            base.OnCreate(bundle);  

            SetContentView(Resource.Layout.SplashScreen);

            this.progressText = FindViewById<TextView>(Resource.Id.SplashScreen_ProgressText);

            new System.Threading.Thread(new ThreadStart(delegate             
            {
                this.InitializeApp();
                StartActivity(typeof(MainActivity));  

            })).Start();
        }  

        private void InitializeApp() 
        {
            /*
            for (int progress=0; progress<100; progress+=10) 
            {
                System.Threading.Thread.Sleep(1000);

                RunOnUiThread(() =>
                {
                    this.progressText.SetText(progress.ToString(), TextView.BufferType.Normal);
                });
            }
            */
        }
    }
}   