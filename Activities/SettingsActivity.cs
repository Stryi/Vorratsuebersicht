using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Android.Support.V4.Content;
using Android.Views;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Settings_Title")]
    public class SettingsActivity : Activity
    {
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

            Button buttonRestoreSampleDb = FindViewById<Button>(Resource.Id.SettingsButton_RestoreSampleDb);
            buttonRestoreSampleDb.Click += ButtonRestoreSampleDb_Click;

            Button buttonRestoreDb0 = FindViewById<Button>(Resource.Id.SettingsButton_RestoreDb0);
            buttonRestoreDb0.Click += ButtonRestoreDb0_Click; 

            Button buttonCompressDb = FindViewById<Button>(Resource.Id.SettingsButton_Compress);
            buttonCompressDb.Click += ButtonCompressDb_Click;

            Button buttonDeleteDb = FindViewById<Button>(Resource.Id.SettingsButton_DeleteDatabase);
            buttonDeleteDb.Click += ButtonDeleteDb_Click;

            Button buttonLicenses = FindViewById<Button>(Resource.Id.SettingsButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(LicensesActivity))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_BackupAndRestore);
            buttonBackup.Click += delegate  
            {
                string text = Resources.GetString(Resource.String.Settings_BackupAndRestoreInfo);

                var path = new Android_Database().GetDatabasePath();
                text = string.Format(text, path);

                var message = new AlertDialog.Builder(this);
                message.SetMessage(text);
                message.SetTitle(Resource.String.App_Name);
                message.SetIcon(Resource.Drawable.ic_launcher);
                message.SetPositiveButton(Resource.String.App_Ok, (s, e) => { });
                message.Create().Show();
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


        private void ButtonDeleteDb_Click(object sender, EventArgs e)
        {
            var message = new AlertDialog.Builder(this);
            message.SetMessage(Resource.String.Settings_DeleteProdDbQuestion);
            message.SetTitle(Resource.String.App_Name);
            message.SetIcon(Resource.Drawable.ic_launcher);
            message.SetPositiveButton(Resource.String.App_Yes, (s, ev) => 
                { 
                    new Android_Database().DeleteDatabase();
                });
            message.SetNegativeButton(Resource.String.App_No, (s, ev) => { });
            message.Create().Show();

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