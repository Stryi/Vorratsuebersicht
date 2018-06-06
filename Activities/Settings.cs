using System;
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    [Activity(Label = "Settings")]
    public class Settings : Activity
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.Settings);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            string dbInfoFormat = Resources.GetString(Resource.String.Main_Datenbank);

            TextView databasePath = FindViewById<TextView>(Resource.Id.MainButton_DatabasePath);
            databasePath.Text = new Android_Database().GetDatabaseInfoText(dbInfoFormat);


            Android.Widget.Switch buttonTestDB = FindViewById<Android.Widget.Switch>(Resource.Id.MainButton_SwitchToTestDB);
            buttonTestDB.Click += ButtonTestDB_Click;
            buttonTestDB.Checked = Android_Database.UseTestDatabase;

            Button buttonRestoreSampleDb = FindViewById<Button>(Resource.Id.MainButton_RestoreSampleDb);
            buttonRestoreSampleDb.Click += delegate
            {
                var progressDialog = ProgressDialog.Show(this, "Bitte warten...", "Test Datenbank wird zurückgesetzt...", true);
                new Thread(new ThreadStart(delegate
                {
                    new Android_Database().RestoreDatabase_Test_Sample(true);

                    // Sich neu connecten;
                    Android_Database.SQLiteConnection = null;

                    RunOnUiThread(() => this.ShowDatabaseInfo());

                    RunOnUiThread(() => progressDialog.Hide());

                })).Start();
            };

            Button buttonRestoreDb0 = FindViewById<Button>(Resource.Id.MainButton_RestoreDb0);
            buttonRestoreDb0.Click += delegate
            {
                new Android_Database().RestoreDatabase_Test_Db0(true);

                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                this.ShowDatabaseInfo();
            };

            Button buttonCompress = FindViewById<Button>(Resource.Id.MainButton_Compress);
            buttonCompress.Click += delegate
            {
                var progressDialog = ProgressDialog.Show(this, "Bitte warten...", "Datenbank wird komprimiert...", true);
                new Thread(new ThreadStart(delegate
                {
                    new Android_Database().CompressDatabase();

                    RunOnUiThread(() => this.ShowDatabaseInfo());
                    RunOnUiThread(() => progressDialog.Hide());

                })).Start();
            };

            Button buttonLicenses = FindViewById<Button>(Resource.Id.MainButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(Licenses))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.MainButton_Backup);
            buttonBackup.Click += delegate
            {
                //new Android_Database().CopyDatabaseToSDCard(false, "NewName");
            };

            this.ShowApplicationVersion();
        }

        private void ButtonTestDB_Click(object sender, System.EventArgs e)
        {
            Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;

            if (!new Android_Database().IsCurrentDatabaseExists())
            {
                Android_Database.UseTestDatabase = !Android_Database.UseTestDatabase;
            }

            Android.Widget.Switch buttonTestDB = FindViewById<Android.Widget.Switch>(Resource.Id.MainButton_SwitchToTestDB);
            buttonTestDB.Checked = Android_Database.UseTestDatabase;

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfo();
        }

        private void ShowApplicationVersion()
        {
            Context context = this.ApplicationContext;
            PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

            TextView versionInfo = FindViewById<TextView>(Resource.Id.MainButton_Version);
            versionInfo.Text = string.Format("Version {0}", info.VersionName);
        }

        private void ShowDatabaseInfo()
        {
            string dbInfoFormat = Resources.GetString(Resource.String.Main_Datenbank);

            TextView databasePath = FindViewById<TextView>(Resource.Id.MainButton_DatabasePath);
            databasePath.Text = new Android_Database().GetDatabaseInfoText(dbInfoFormat);
        }


    }
}