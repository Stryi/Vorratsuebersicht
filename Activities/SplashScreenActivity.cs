using System;
using System.IO;
using System.Threading;
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

            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            //TaskScheduler.UnobservedTaskException      += TaskScheduler_UnobservedTaskException;

            SetContentView(Resource.Layout.SplashScreen);

            this.progressText = FindViewById<TextView>(Resource.Id.SplashScreen_ProgressText);
            this.progressBar  = FindViewById<ProgressBar>(Resource.Id.SplashScreen_ProgressBar);

            this.ProtocolAppInfo();

            ServerDatabase.Initialize(
                //"Azure;http://stryi.westeurope.cloudapp.azure.com:5000;3" + System.Environment.NewLine +
                //"Lokal;http://localhost:5000"     + System.Environment.NewLine +
                //"LAN;http://192.168.0.157:5000" + System.Environment.NewLine +       // IP-Adresse ohne VPN am 'Kabel' hängend
                "W-LAN;http://192.168.0.139:5000" + System.Environment.NewLine,        // IP-Adresse über W-LAN
                "Vorraete");

            this.progressText.SetText("Ermittle Datenbanken...", TextView.BufferType.Normal);
            this.progressText.Visibility = Android.Views.ViewStates.Visible;
            this.progressBar.Visibility = Android.Views.ViewStates.Visible;
            this.progressBar.Indeterminate = true;

            new Thread(new ThreadStart(delegate
            {
                bool selected = this.SelectDatabase();

                if (selected)
                {
                    this.CheckAndMoveArticleImages();
                    StartActivity(typeof(MainActivity));
                }
            })).Start();
        }

        /// <summary>
        /// Datenbankauswahl (bei mehreren Datenbanken)
        /// </summary>
        /// <returns>true - Datenbank wurde ausgewählt. false - Datenbankauswahl wird angezeigt.</returns>
        private bool SelectDatabase() 
        {
            Exception ex = null;
            var fileList = DatabaseService.GetDatabases(this, ref ex);

            if (ex != null)
            {
                TRACE(ex);

                string text = "Fehler beim Ermitteln der Datenbanken.";
                TRACE("SplashScreen: {0}", ex.Message);
                TRACE("SplashScreen: {0}", text);

                text = ex.Message + "\n\n" + text;

                RunOnUiThread(() =>
                {
                    Toast.MakeText(this, text, ToastLength.Long).Show();
                });
            }

            
            var testDatabaseName = Android_Database.GetTestDatabaseFileName(this);
            DatabaseService.Database.RemoveDatabaseFromList(ref fileList, testDatabaseName);

            if (fileList.Count == 1)
            {
                DatabaseService.TryOpenDatabase(fileList[0]);
                return true;
            }

            if (fileList.Count == 0)
            {
                return true;
            }


            string[] databaseNames = new string[fileList.Count];

            for(int i = 0; i < fileList.Count; i++)
            {
                databaseNames[i] = fileList[i].Name;
            }

            RunOnUiThread(() =>
            {
                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle(this.Resources.GetString(Resource.String.Main_OpenDatabase));
                builder.SetItems(databaseNames, (sender2, args) =>
                {
                    var database = fileList[args.Which];

                    DatabaseService.TryOpenDatabase(database);
                    this.ConvertAndStartMainScreen();
                });

                builder.SetOnCancelListener(new ActionDismissListener(() =>
                {
                    DatabaseService.TryOpenDatabase(fileList[0]);
                    this.ConvertAndStartMainScreen();
                }));

                builder.Show();

                this.progressText.SetText("Öffne Datenbanken...", TextView.BufferType.Normal);

            });

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
            if (DatabaseService.Instance == null)
                return true;

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
                    DatabaseService.Instance.ExecuteNonQuery(cmdCopyImages, article.ArticleId);
                    DatabaseService.Instance.ExecuteNonQuery(cmdClearImages);
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
            catch(Exception e)
            {
                TRACE("SettingsActivity.ShowApplicationVersion() - {0}", e.Message);
            }

            return versionInfo;
        }

        /*
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            var newExc = new Exception("TaskSchedulerOnUnobservedTaskException", e.Exception);
            TRACE(newExc);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var newExc = new Exception("CurrentDomainOnUnhandledException", e.ExceptionObject as Exception);
            TRACE(newExc);
        }
        */
    }
}   