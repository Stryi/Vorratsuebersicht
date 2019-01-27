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
    using static Tools;

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
            bool emulator = Android.OS.Environment.IsExternalStorageEmulated;
            string status = Android.OS.Environment.ExternalStorageState;
            bool canWrite = Android.OS.Environment.ExternalStorageDirectory.CanWrite();
            string sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            string databasePath = Android_Database.Instance.GetDatabasePath();
            
            TRACE("********************************************");
            TRACE("Is SD card emulated : {0}", emulator);
            TRACE("SD card state       : {0}", status);
            TRACE("Can write SD card   : {0}", canWrite);
            TRACE("SD card path        : {0}", sdCardPath);
            TRACE("Database path       : {0}", databasePath);
            TRACE("Database on SD card : {0}", Android_Database.IsDatabaseOnSdCard);
            TRACE("********************************************");

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