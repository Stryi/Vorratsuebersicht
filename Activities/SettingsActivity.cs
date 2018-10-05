using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Runtime;
using Android.Provider;
using System.IO;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Settings_Title")]
    public class SettingsActivity : Activity
    {
        public static readonly int SelectBackupId = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            string dbInfoFormat = Resources.GetString(Resource.String.Settings_Datenbank);

            TextView databasePath = FindViewById<TextView>(Resource.Id.SettingsButton_DatabasePath);
            databasePath.Text = new Android_Database().GetDatabaseInfoText(dbInfoFormat);


            Android.Widget.Switch buttonTestDB = FindViewById<Android.Widget.Switch>(Resource.Id.SettingsButton_SwitchToTestDB);
            buttonTestDB.Click += ButtonTestDB_Click;
            buttonTestDB.Checked = Android_Database.UseTestDatabase;

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                buttonTestDB.Enabled = false;
            }

            Button buttonRestoreSampleDb = FindViewById<Button>(Resource.Id.SettingsButton_RestoreSampleDb);
            buttonRestoreSampleDb.Click += ButtonRestoreSampleDb_Click;

            Button buttonRestoreDb0 = FindViewById<Button>(Resource.Id.SettingsButton_RestoreDb0);
            buttonRestoreDb0.Click += ButtonRestoreDb0_Click; 

            Button buttonCompressDb = FindViewById<Button>(Resource.Id.SettingsButton_Compress);
            buttonCompressDb.Click += ButtonCompressDb_Click;

            Button buttonLicenses = FindViewById<Button>(Resource.Id.SettingsButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(LicensesActivity))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            buttonBackup.Click += delegate  
            {
                var path = new Android_Database().GetDatabasePath();

                var shareUri = Android.Net.Uri.Parse("file://" + path);

                Intent intent = new Intent(Intent.ActionSend);
                intent.SetType("*/*");
                //intent.SetType("application/db3");
                intent.PutExtra(Intent.ExtraStream, shareUri);
                this.StartActivity(Intent.CreateChooser(intent, "Share file"));
            };

            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            buttonRestore.Click += delegate  
            {
                var intent = new Intent();
                intent.SetType("*/*");
                //intent.SetType("application/db3");
                //intent.SetType("application/*");
                intent.SetAction(Intent.ActionGetContent);
                StartActivityForResult(Intent.CreateChooser(intent, "Select backup"), SelectBackupId);
            };

            this.ShowApplicationVersion();
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
                return;

            if ((requestCode == SelectBackupId) && (data != null))
            {
                var builder = new AlertDialog.Builder(this);
                builder.SetTitle(Resource.String.Settings_RestoreBackup);
                builder.SetMessage("Ist noch nicht implementiert.\n\n" + data.DataString);
                builder.SetPositiveButton("Ok", (s, e) => { });
                builder.Create().Show();
                
                /*
                 * TODO: Implementieren
                if (!data.DataString.EndsWith(".db3"))
                {                    
                    // Keine Datenbank Datei ausgewählt.
                    return;
                }

                Android.Net.Uri uri = data.Data;
                string fileSource = this.GetPathToFile(uri);

                var path = new Android_Database().GetDatabasePath();
                string destinationPath = Path.GetDirectoryName(path);
                string fileDestination = Path.Combine(destinationPath, "Vorraete_Restore.db3");

                //File.Copy(fileSource, fileDestination, true);
                */
            }
        }

        private string GetPathToFile(Android.Net.Uri uri)
        {
            var cursor = this.ContentResolver.Query(uri, null, null, null, null);
            cursor.MoveToFirst();
            string document_id = cursor.GetString(0);
            document_id = document_id.Split(':')[1];
            cursor.Close();

            
            string path = string.Empty;

            // SD Karten oder interner Speicher
            if (uri.Host == "com.android.externalstorage.documents")
            {
                if (uri.Path.Contains("/primary:"))
                {
                    // Pfad: /storage/emulated/0/_Backup
                    // Interner Speicher
                    //path = Android.OS.Environment.DataDirectory.AbsolutePath;

                    path = Path.Combine(Android.OS.Environment.RootDirectory.Path,
                        Android.OS.Environment.DataDirectory.Path);

                    var test1 = Android.OS.Environment.DataDirectory;
                    var test2 = Android.OS.Environment.RootDirectory;
                    var test3 = Android.OS.Environment.DataDirectory;
                    var test4 = Android.OS.Environment.DirectoryDownloads;

                    var test5 = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads);
                }
                else
                {
                    // SD Karte
                    string sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;

                    //path = "/storage/sdcard/";
                }
            }
                    
            path = Path.Combine(path, document_id);

            return path;
        }

        private void ButtonRestoreSampleDb_Click(object sender, EventArgs e)
        {
            var progressDialog = this.CreateProgressBar();
            new Thread(new ThreadStart(delegate
            {
                new Android_Database().RestoreDatabase_Test_Sample(true);

                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                RunOnUiThread(() => this.ShowDatabaseInfo());

                this.HideProgressBar(progressDialog);

            })).Start();
        }

        private void ButtonRestoreDb0_Click(object sender, EventArgs e)
        {
            new Android_Database().RestoreDatabase_Test_Db0(true);

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
        }

        private void ButtonCompressDb_Click(object sender, EventArgs e)
        {
            var progressDialog = this.CreateProgressBar();
            new Thread(new ThreadStart(delegate
            {
                new Android_Database().CompressDatabase();

                RunOnUiThread(() => this.ShowDatabaseInfo());
                this.HideProgressBar(progressDialog);

            })).Start();
        }

        private void ButtonTestDB_Click(object sender, System.EventArgs e)
        {
            Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;

            if (!new Android_Database().IsCurrentDatabaseExists())
            {
                Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;
            }

            Android.Widget.Switch buttonTestDB = FindViewById<Android.Widget.Switch>(Resource.Id.SettingsButton_SwitchToTestDB);
            buttonTestDB.Checked = Android_Database.UseTestDatabase;

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
        }

        private void ShowApplicationVersion()
        {
            Context context = this.ApplicationContext;
            PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

            TextView versionInfo = FindViewById<TextView>(Resource.Id.SettingsButton_Version);
            versionInfo.Text = string.Format("Version {0} (Code Version {1})", info.VersionName, info.VersionCode);
        }

        private void ShowDatabaseInfo()
        {
            string dbInfoFormat = Resources.GetString(Resource.String.Settings_Datenbank);

            TextView databasePath = FindViewById<TextView>(Resource.Id.SettingsButton_DatabasePath);
            databasePath.Text = new Android_Database().GetDatabaseInfoText(dbInfoFormat);
        }

        private ProgressBar CreateProgressBar()
        {
            var progressBar = FindViewById<ProgressBar>(Resource.Id.ProgressBar);
            progressBar.Visibility = ViewStates.Visible;
            this.Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
            return progressBar;
        }

        private void HideProgressBar(ProgressBar progressBar)
        {
            RunOnUiThread(() =>
            {
                progressBar.Visibility = ViewStates.Invisible;
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            });
        }

    }
}