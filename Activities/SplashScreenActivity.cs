using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text;
using System.Globalization;

using Android.App;
using Android.OS;
using Android.Widget;
using Android.Support.V7.App;
using Android.Content;
using Android.Content.PM;

// Anhand von
// http://www.c-sharpcorner.com/UploadFile/1e050f/creating-splash-screen-for-android-app-in-xamarin/
// 
namespace VorratsUebersicht  
{
    using static Tools;
    using AlertDialog = Android.App.AlertDialog;

    [Activity(Label="@string/App_Name",MainLauncher=true,Theme="@style/Theme.Splash",NoHistory=true,Icon="@drawable/ic_launcher")]  
    public class SplashScreenActivity : AppCompatActivity
    {  
        private TextView    progressText;
        private ProgressBar progressBar;

        protected override void OnCreate(Bundle bundle)  
        {  
            base.OnCreate(bundle);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException      += TaskScheduler_UnobservedTaskException;

            SetContentView(Resource.Layout.SplashScreen);

            this.progressText = FindViewById<TextView>(Resource.Id.SplashScreen_ProgressText);
            this.progressBar  = FindViewById<ProgressBar>(Resource.Id.SplashScreen_ProgressBar);

            this.ProtocolAppInfo();

            bool selected = this.SelectDatabase();
            if (selected)
            {
                this.CheckAndMoveArticleImages();
                StartActivity(typeof(MainActivity));
            }
        }

        /// <summary>
        /// Datenbankauswahl (bei mehreren Datenbanken)
        /// </summary>
        /// <returns>true - Datenbank wurde ausgewählt. false - Datenbankauswahl wird angezeigt.</returns>
        private bool SelectDatabase() 
        {
            List<string> fileList;

            Exception ex = Android_Database.LoadDatabaseFileListSafe(this, out fileList);

            if (ex != null)
            {
                TRACE(ex);

                string text = "Fehler beim Ermitteln der Datenbanken.";
                TRACE("SplashScreen: {0}", ex.Message);
                TRACE("SplashScreen: {0}", text);

                text = ex.Message + "\n\n" + text;

                Toast.MakeText(this, text, ToastLength.Long).Show();
            }

            if (fileList.Count == 1)
            {
                Android_Database.TryOpenDatabase(fileList[0]);
                return true;
            }

            if (fileList.Count == 0)
            {
                return true;
            }

            string[] databaseNames = new string[fileList.Count];

            for(int i = 0; i < fileList.Count; i++)
            {
                databaseNames[i] = Path.GetFileNameWithoutExtension(fileList[i]);
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(this.Resources.GetString(Resource.String.Main_OpenDatabase));
            builder.SetItems(databaseNames, (sender2, args) =>
            {
                string database = fileList[args.Which];

                Android_Database.TryOpenDatabase(database);

                this.ConvertAndStartMainScreen();
            });

            builder.SetOnCancelListener(new ActionDismissListener(() =>
            {
                Android_Database.TryOpenDatabase(fileList[0]);

                this.ConvertAndStartMainScreen();
            }));

            builder.Show();

            return false;

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

            //var packageName       = AppInfo.PackageName;                                // "de.stryi.Vorratsuebersicht"
            //var appDataDirectory  = Xamarin.Essentials.FileSystem.AppDataDirectory;     // "/data/user/0/de.stryi.Vorratsuebersicht/files"
            //var cacheDirectory    = Xamarin.Essentials.FileSystem.CacheDirectory;       // "/data/user/0/de.stryi.Vorratsuebersicht/cache"

            //var downloadDirectory = Android.OS.Environment.DirectoryDownloads;                      // "Download"
            //var internerStorage   = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;   // "/storage/emulated/0"

            //TRACE("DirectoryDcim        => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDcim));            // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/DCIM
            //TRACE("MediaUnmounted       => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.MediaUnmounted));           // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/unmounted
            //TRACE("MediaUnmountable     => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.MediaUnmountable));         // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/unmountable
            //TRACE("MediaShared          => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.MediaShared));              // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/shared
            //TRACE("MediaShared          => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.MediaShared));              // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/shared
            //TRACE("MediaMounted         => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.MediaMounted));             // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/mounted
            //TRACE("DirectoryDocuments   => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryDocuments));       // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/Documents
            //TRACE("DirectoryPictures    => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryPictures));        // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/Pictures
            //TRACE("DirectoryScreenshots => {0}", Application.Context.GetExternalFilesDir(Android.OS.Environment.DirectoryScreenshots));     // /storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/Screenshots

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

        private void ConvertAndStartMainScreen()
        {
            new System.Threading.Thread(new ThreadStart(delegate             
            {
                this.CheckAndMoveArticleImages();
                StartActivity(typeof(MainActivity));

            })).Start();
        }

        //
        // Bilder in die neue Tabelle übertragen
        //
        private bool CheckAndMoveArticleImages()
        {
            // Nur, wenn bereits eine Datenbank vorhanden ist
            if (Android_Database.SQLiteConnection == null)
                return true;

            var databaseConnection = Android_Database.SQLiteConnection;

            // Artikelbilder ermitteln, die noch nicht übertragen wurden.
            var articleImagesToCopy = Database.GetArticlesToCopyImages();

            if (articleImagesToCopy.Count == 0)
                return true;

            string message = string.Format("Übertrage {0} Artikelbilder...", articleImagesToCopy.Count);
            TRACE(message);
            RunOnUiThread(() =>
            {
                this.progressText.SetText(message, TextView.BufferType.Normal);
                Thread.Sleep(500);
            });

            // Einzelne Bilder kopieren, damit die Datenbankgröße nicht zu stark wächst.
            string cmdCopyImages = 
                "INSERT INTO ArticleImage (ArticleId, Type, ImageSmall, ImageLarge, CreatedAt)" +
                " SELECT ArticleId, 0,  Image AS ImageSmall, ImageLarge, DATETIME('now')" +
                " FROM Article" +
                " WHERE ArticleId = ?";

            // Bilder löschen, da sie schon übernommen wurden
            string cmdClearImages = 
                "UPDATE Article" +
                " SET Image = NUll, ImageLarge = NULL" +
                " WHERE ArticleId IN (SELECT ArticleId FROM ArticleImage)";

            int count = 0;
            int max = articleImagesToCopy.Count;

            Exception exception = null;
            this.progressBar.Max = max;

            foreach(Article article in articleImagesToCopy)
            {
                try
                {
                    databaseConnection.Execute(cmdCopyImages, article.ArticleId);
                    databaseConnection.Execute(cmdClearImages);
                }
                catch(Exception ex)
                {
                    TRACE(ex);

                    exception = ex;
                    break;
                }
                count++;

                RunOnUiThread(() =>
                {
                    message = string.Format("Übertrage Artikelbild {0} von {1}\n{2}",
                        count,
                        max,
                        article.Name);

                    this.progressBar.Progress = count;
                    this.progressText.SetText(message, TextView.BufferType.Normal);
                    Thread.Sleep(200);
                });
            }

            if (exception != null)
            {
                RunOnUiThread(() =>
                {
                    message = string.Format("Fehler '{0}' beim Übertragen der {1} Bilder in die neue Tabelle.\n\n" +
                        "Bitte beenden Sie die Anwendung, verschaffen mehr Platz auf der SD Karte und dem internen Speicher " +
                        "und starten Sie die App erneut",
                        exception.Message,
                        articleImagesToCopy.Count);

                    this.progressBar.Visibility = Android.Views.ViewStates.Invisible;

                    var dialog = new AlertDialog.Builder(this);
                    dialog.SetMessage(message);
                    dialog.SetTitle(Resource.String.App_Name);
                    dialog.SetIcon(Resource.Drawable.ic_launcher);
                    dialog.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => 
                    { 
                        StartActivity(typeof(MainActivity));
                        
                    });
                    dialog.Create().Show();
                });

                return false;
            }

            return true;
        }

        private void ProtocolAppInfo()
        {
            var build = Build.RadioVersion;

            StringBuilder text = new StringBuilder();
            text.AppendFormat("--- Application Start ---\n");
            text.AppendFormat("{0}\n", this.GetApplicationVersion());
            text.AppendFormat("Android Version: {0}\n",  Build.VERSION.Release);
            text.AppendFormat("Android SDK: {0}\n",      Build.VERSION.SdkInt);
            text.AppendFormat("CurrentCulture: {0}\n",   CultureInfo.CurrentCulture.DisplayName);
            text.AppendFormat("CurrentUICulture: {0}\n", CultureInfo.CurrentUICulture.DisplayName);

            TRACE(text.ToString());
        }

        private string GetApplicationVersion()
        {
            string versionInfo = string.Empty;
            try
            {
                Context context = this.ApplicationContext;
                PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

                versionInfo += string.Format("Version {0}",         info.VersionName);
                versionInfo += string.Format(" (Code Version {0})", info.LongVersionCode);
            }
            catch { }

            return versionInfo;
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", e.Exception);
            TRACE(newExc);
            throw newExc;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", e.ExceptionObject as Exception);
            TRACE(newExc);
            throw newExc;
        }
    }
}   