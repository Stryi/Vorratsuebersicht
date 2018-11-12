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
using System.Threading.Tasks;

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
                var databaseFileName = new Android_Database().GetProductiveDatabasePath();

                var downloadFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                        Android.OS.Environment.DirectoryDownloads).AbsolutePath;

                string backupName = string.Format("Vü_{0}.VueBak", 
                    DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));

                var backupFileName = Path.Combine(downloadFolder, backupName);

                var progressDialog = this.CreateProgressBar();
                new Thread(new ThreadStart(delegate
                {
                    File.Copy(databaseFileName, backupFileName);

                    this.HideProgressBar(progressDialog);

                    RunOnUiThread(() =>
                    {
                        string message = string.Format(
                            "Datenbank im Download Verzeichnis gesichert als:\n\n {0}" +
                            "\n\nSichern Sie diese Datei auf Google Drive oder auf Ihren PC.",
                            backupFileName);

                        var builder = new AlertDialog.Builder(this);
                        builder.SetMessage(message);
                        builder.SetPositiveButton("Ok", (s, e) => { });
                        builder.Create().Show();
                    });
                })).Start();

                return;
            };

            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            buttonRestore.Click += delegate  
            {
                // Backups müssen sich im Download Verzeichnis befinden.
                var downloadFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                        Android.OS.Environment.DirectoryDownloads).AbsolutePath;

                var selectFile = new Intent(this, typeof(SelectFileActivity));
                selectFile.PutExtra("Text",         "Backup auswählen:");
                selectFile.PutExtra("Path",          downloadFolder);
                selectFile.PutExtra("SearchPattern", "*.VueBak");

                StartActivityForResult(selectFile, SelectBackupId);
             };

            this.EnableButtons();

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
                string fileSource = data.GetStringExtra("FullName");
                string fileDestination = new Android_Database().GetProductiveDatabasePath();
                
                string message = string.Format("Backup Datenbank zurückspielen?\n\n{0}",
                    Path.GetFileName(fileSource));

                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(message);
                builder.SetNegativeButton("Abbruch",(s, e) => { });
                builder.SetPositiveButton("Ok", (s, e) => 
                { 
                    var progressDialog = this.CreateProgressBar();
                    new Thread(new ThreadStart(delegate
                    {
                        File.Copy(fileSource, fileDestination, true);

                        // Sich neu connecten;
                        Android_Database.SQLiteConnection = null;

                        RunOnUiThread(() => this.ShowDatabaseInfo());

                        this.HideProgressBar(progressDialog);

                    })).Start();

                });
                builder.Create().Show();
            }
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
            this.EnableButtons();
        }

        private void EnableButtons()
        {
            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);

            buttonBackup.Enabled = !Android_Database.UseTestDatabase;
            buttonRestore.Enabled = !Android_Database.UseTestDatabase;       
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