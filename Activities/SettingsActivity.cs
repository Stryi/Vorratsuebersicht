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
    using static Tools;

    [Activity(Label = "@string/Settings_Title")]
    public class SettingsActivity : Activity
    {
        public static readonly int SelectBackupId = 1000;
        private bool userCategoriesChanged = false;

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
            databasePath.Text = Android_Database.Instance.GetDatabaseInfoText(dbInfoFormat);


            Switch switchToTestDB = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToTestDB);
            switchToTestDB.Click += ButtonTestDB_Click;
            switchToTestDB.Checked = Android_Database.UseTestDatabase;

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                switchToTestDB.Enabled = false;
            }

            Switch switchToAppDB = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToAppDb);
            switchToAppDB.Click += SwitchToAppDb_Click;
            switchToAppDB.Checked = Android_Database.UseAppFolderDatabase;

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                switchToAppDB.Enabled = false;
            }

            Button buttonRestoreSampleDb = FindViewById<Button>(Resource.Id.SettingsButton_RestoreSampleDb);
            buttonRestoreSampleDb.Click += ButtonRestoreSampleDb_Click;

            Button buttonRestoreDb0 = FindViewById<Button>(Resource.Id.SettingsButton_RestoreDb0);
            buttonRestoreDb0.Click += ButtonRestoreDb0_Click; 

            Button buttonCompressDb = FindViewById<Button>(Resource.Id.SettingsButton_Compress);
            buttonCompressDb.Click += ButtonCompressDb_Click;

            Button buttonRepairDb = FindViewById<Button>(Resource.Id.SettingsButton_Repair);
            buttonRepairDb.Click += ButtonRepairDb_Click;

            Button buttonLicenses = FindViewById<Button>(Resource.Id.SettingsButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(LicensesActivity))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            buttonBackup.Click += ButtonBackup_Click;

            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            buttonRestore.Click += ButtonRestore_Click;

            this.ShowUserDefinedCategories();

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            catEdit.TextChanged += delegate { this.userCategoriesChanged = true; };

            this.EnableButtons();

            this.ShowApplicationVersion();

            // Artikelname ist eingetragen. Tastatus anf�nglich ausblenden.
            this.Window.SetSoftInputMode(SoftInput.StateHidden);
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

        public override void OnBackPressed()
        {
            this.SaveUserDefinedCategories();
            base.OnBackPressed();
        }

        private void SaveUserDefinedCategories()
        {
            if (!this.userCategoriesChanged)
                return;

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            MainActivity.SetUserDefinedCategories(catEdit?.Text);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
                return;

            if ((requestCode == SelectBackupId) && (data != null))
            {
                string fileSource = data.GetStringExtra("FullName");
                string fileDestination = Android_Database.Instance.GetProductiveDatabasePath();
                
                string message = string.Format("Backup Datenbank\n\n{0}\n\nwiederherstellen in {1}?",
                    Path.GetFileName(fileSource),
                    Path.GetFileName(fileDestination));

                var builder = new AlertDialog.Builder(this);
                builder.SetMessage(message);
                builder.SetNegativeButton("Abbruch",(s, e) => { });
                builder.SetPositiveButton("Ok", (s, e) => 
                { 
                    var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_BackupAndRestore);
                    new Thread(new ThreadStart(delegate
                    {
                        // L�schen von '*.db3-shn' und '*.db3-wal'
                        //string deleteFile1 = Path.ChangeExtension(fileDestination, "db3-shm");
                        //File.Delete(deleteFile1);

                        string deleteFile2 = Path.ChangeExtension(fileDestination, "db3-wal");

                        if (File.Exists(deleteFile2))
                            File.Delete(deleteFile2);

                        File.Copy(fileSource, fileDestination, true);

                        // Sich neu connecten;
                        Android_Database.SQLiteConnection = null;

                        RunOnUiThread(() =>
                        {
                            this.ShowDatabaseInfo();
                            this.ShowUserDefinedCategories();
                        });

                        this.HideProgressBar(progressDialog);

                    })).Start();

                });
                builder.Create().Show();
            }
        }


        private void ButtonRestoreSampleDb_Click(object sender, EventArgs e)
        {
            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_RestoreSampleDb);
            new Thread(new ThreadStart(delegate
            {
                Android_Database.Instance.RestoreDatabase_Test_Sample(true);

                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                RunOnUiThread(() => 
                {
                    this.ShowDatabaseInfo();
                    this.ShowUserDefinedCategories();
                });

                this.HideProgressBar(progressDialog);

            })).Start();
        }

        private void ButtonRestoreDb0_Click(object sender, EventArgs e)
        {
            Android_Database.Instance.RestoreDatabase_Test_Db0(true);

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
            this.ShowUserDefinedCategories();
        }

        private void ButtonCompressDb_Click(object sender, EventArgs e)
        {
            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_Compress);
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    Android_Database.Instance.CompressDatabase();
                }
                catch(Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        var messageBox = new AlertDialog.Builder(this);
                        messageBox.SetTitle("Fehler aufgetreten!");
                        messageBox.SetMessage(ex.Message);
                        messageBox.SetPositiveButton("OK", (s, evt) => { });
                        messageBox.Create().Show();
                    });
                }

                RunOnUiThread(() => this.ShowDatabaseInfo());
                this.HideProgressBar(progressDialog);

            })).Start();
        }

        private void ButtonRepairDb_Click(object sender, EventArgs e)
        {
            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_Compress);
            new Thread(new ThreadStart(delegate
            {
                try
                {
                    string result = Android_Database.Instance.RepairDatabase();

                    RunOnUiThread(() =>
                    {
                        var messageBox = new AlertDialog.Builder(this);
                        messageBox.SetTitle("Ergebnis der Pr�fung:");
                        messageBox.SetMessage(result);
                        messageBox.SetPositiveButton("OK", (s, evt) => { });
                        messageBox.Create().Show();
                    });
                }
                catch(Exception ex)
                {
                    RunOnUiThread(() =>
                    {
                        var messageBox = new AlertDialog.Builder(this);
                        messageBox.SetTitle("Fehler aufgetreten!");
                        messageBox.SetMessage(ex.Message);
                        messageBox.SetPositiveButton("OK", (s, evt) => { });
                        messageBox.Create().Show();
                    });
                }

                this.HideProgressBar(progressDialog);

            })).Start();
        }

        private void ButtonTestDB_Click(object sender, System.EventArgs e)
        {
            Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;

            if (!Android_Database.Instance.IsCurrentDatabaseExists())
            {
                Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;
            }

            this.SaveUserDefinedCategories();

            Switch switchToTestDB = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToTestDB);
            switchToTestDB.Checked = Android_Database.UseTestDatabase;

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
            this.ShowUserDefinedCategories();
            this.EnableButtons();
        }

        private void SwitchToAppDb_Click(object sender, System.EventArgs e)
        {
            this.SaveUserDefinedCategories();

            Android_Database.UseAppFolderDatabase = !Android_Database.UseAppFolderDatabase;

            Android_Database.Instance.GetDatabasePath();

            if (Android_Database.UseAppFolderDatabase)
            {
                // Ist stattdessen die SD Karten Datenbank?
                if ((Android_Database.IsDatabaseOnSdCard == null) ||(Android_Database.IsDatabaseOnSdCard == true))
                {
                    Android_Database.UseAppFolderDatabase = !Android_Database.UseAppFolderDatabase;
                }
            }
            else
            {
                // Ist stattdessen die SD Karten Datenbank?
                if ((Android_Database.IsDatabaseOnSdCard == null) ||(Android_Database.IsDatabaseOnSdCard == false))
                {
                    Android_Database.UseAppFolderDatabase = !Android_Database.UseAppFolderDatabase;
                }
            }


            Switch switchToAppDb = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToAppDb);
            switchToAppDb.Checked = Android_Database.UseAppFolderDatabase;

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
            this.ShowUserDefinedCategories();
            this.EnableButtons();
        }

        private void ButtonRestore_Click(object sender, EventArgs e)
        {
            // Backups m�ssen sich im Download Verzeichnis befinden.
            var downloadFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                    Android.OS.Environment.DirectoryDownloads).AbsolutePath;

            var selectFile = new Intent(this, typeof(SelectFileActivity));
            selectFile.PutExtra("Text",         "Backup ausw�hlen:");
            selectFile.PutExtra("Path",          downloadFolder);
            selectFile.PutExtra("SearchPattern", "*.VueBak");

            StartActivityForResult(selectFile, SelectBackupId);
        }

        private void ButtonBackup_Click(object sender, EventArgs eventArgs)
        {
            bool isGranted = new SdCardAccess().Grand(this);

            if (!isGranted)
            {
                return;
            }

            // Vor dem Backup ggf. die User-Kategorien ggf. speichern,
            // damit es auch im Backup ist.
            this.SaveUserDefinedCategories();

            var databaseFilePath = Android_Database.Instance.GetProductiveDatabasePath();

            var downloadFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                    Android.OS.Environment.DirectoryDownloads).AbsolutePath;

            string backupFileName;
            string databaseFileName = Path.GetFileNameWithoutExtension(databaseFilePath);

            if (databaseFileName == "Vorraete")
            {
                backupFileName = string.Format("Vue_{0}.VueBak", 
                    DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));
            }
            else
            {
                backupFileName = string.Format(databaseFileName + "_{0}.VueBak", 
                    DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss"));
            }

            var backupFilePath = Path.Combine(downloadFolder, backupFileName);

            Android_Database.Instance.CloseConnection();

            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_BackupAndRestore);
            new Thread(new ThreadStart(delegate
            {
                string message; 
                try
                {
                    File.Copy(databaseFilePath, backupFilePath);
                    message = string.Format(
                        "Datenbank im Download Verzeichnis gesichert als:\n\n {0}" +
                        "\n\nSichern Sie diese Datei auf Google Drive oder auf Ihren PC.",
                        backupFilePath);
                }
                catch(Exception ex)
                {
                    message = ex.Message;
                }

                this.HideProgressBar(progressDialog);

                RunOnUiThread(() =>
                {

                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage(message);
                    builder.SetPositiveButton("Ok", (s, e) => { });
                    builder.Create().Show();
                });
            })).Start();

            return;
        }

        private void EnableButtons()
        {
            Button buttonBackup  = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            Switch switchToAppDB = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToAppDb);

            buttonBackup.Enabled  = !Android_Database.UseTestDatabase;
            buttonRestore.Enabled = !Android_Database.UseTestDatabase;
            switchToAppDB.Enabled = !Android_Database.UseTestDatabase;
        }

        private void ShowUserDefinedCategories()
        {
            string categoryText = string.Empty;

           EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);

            try
            {
                var categories = MainActivity.GetUserDefinedCategories();
                foreach(string category in categories)
                {
                    categoryText += category + ", ";
                }
            }
            catch(Exception e)
            {
                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage("Fehler beim Laden der benutzerspezifischen Kategorien\n\n" + e.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();

                this.FindViewById<TextView>(Resource.Id.Settings_Categories_Text).Enabled = false;
                catEdit.Enabled = false;
            }

            catEdit.Text = categoryText;
            catEdit.SetSelection(categoryText.Length);

            this.userCategoriesChanged = false;
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
            databasePath.Text = Android_Database.Instance.GetDatabaseInfoText(dbInfoFormat);
        }

        private ProgressBar CreateProgressBar(int resourceId)
        {
            var progressBar = FindViewById<ProgressBar>(resourceId);
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