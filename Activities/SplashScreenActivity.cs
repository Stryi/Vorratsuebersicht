using Android.App;
using Android.OS;

// Anhand von
// http://www.c-sharpcorner.com/UploadFile/1e050f/creating-splash-screen-for-android-app-in-xamarin/
// 
namespace VorratsUebersicht  
{  
    [Activity(Label="Vorratsübersicht",MainLauncher=true,Theme="@style/Theme.Splash",NoHistory=true,Icon="@drawable/ic_launcher")]  
    public class SplashScreenActivity : Activity  
    {  
        protected override void OnCreate(Bundle bundle)  
        {  
            base.OnCreate(bundle);  
            StartActivity(typeof(MainActivity));  
        }  
    }  
}   