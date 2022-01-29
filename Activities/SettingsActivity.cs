using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Content.PM;
using Android.Widget;
using Android.Support.V4.Content;
using Android.Views;
using Android.Runtime;

using Xamarin.Essentials;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "@string/Settings_Title")]
    public class SettingsActivity : Activity
    {
        public static readonly int SelectBackupId = 1000;
        private bool userCategoriesChanged = false;
        private bool additionalDatabasePathChanged = false;
        private bool isInitialize = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            this.isInitialize = true;

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


            Switch useFrontCamera = FindViewById<Switch>(Resource.Id.SettingsButton_EANScan_FrontCamera);
            useFrontCamera.Click += UseFrontCamera_Click; ;
            useFrontCamera.Checked = Settings.GetBoolean("UseFrontCameraForEANScan", false);

            Switch switchToTestDB = FindViewById<Switch>(Resource.Id.SettingsButton_SwitchToTestDB);
            switchToTestDB.Click += ButtonTestDB_Click;
            switchToTestDB.Checked = Android_Database.UseTestDatabase;

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                switchToTestDB.Enabled = false;
            }

            Switch switchCostMessage = FindViewById<Switch>(Resource.Id.SettingsButton_ShowOFFCostMessage);
            switchCostMessage.Click += SwitchCostMessage_Click;
            switchCostMessage.Checked = ArticleDetailsActivity.showCostMessage;

            Switch switchAltDatePicker = FindViewById<Switch>(Resource.Id.SettingsButton_AltDatePicker);
            switchAltDatePicker.Click += SwitchAltDatePicker_Click;
            switchAltDatePicker.Checked = StorageItemQuantityActivity.UseAltDatePicker;
            
            Switch checkDatabaseFilesToMove = FindViewById<Switch>(Resource.Id.SettingsButton_CheckDatabaseFilesToMove);
            checkDatabaseFilesToMove.Click += CheckDatabaseFilesToMove_Click;
            checkDatabaseFilesToMove.Checked = Settings.GetBoolean("CheckDatabaseFilesToMove", true);
            
            bool compress = Settings.GetBoolean("CompressPictures", true);
            int compressMode = Settings.GetInt("CompressPicturesMode", 1);

            Switch compressPicturesSwitch = FindViewById<Switch>(Resource.Id.SettingsButton_CompressPictures);

            compressPicturesSwitch.Click += CompressPictures_Click;
            compressPicturesSwitch.Checked = compress;


            RadioButton radioButton1 = FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Small);
            radioButton1.Enabled = compress;
            radioButton1.Checked = (compressMode == 1);
            radioButton1.Click  += CompressMode_Click;

            RadioButton radioButton2 = FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Middle);
            radioButton2.Enabled = compress;
            radioButton2.Checked = (compressMode == 2);
            radioButton2.Click  += CompressMode_Click;

            RadioButton radioButton3 = FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Big);
            radioButton3.Enabled = compress;
            radioButton3.Checked = (compressMode == 3);
            radioButton3.Click  += CompressMode_Click;

            RadioButton radioButton4 = FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Huge);
            radioButton4.Enabled = compress;
            radioButton4.Checked = (compressMode == 4);
            radioButton4.Click  += CompressMode_Click;


            Button buttonRestoreSampleDb = FindViewById<Button>(Resource.Id.SettingsButton_RestoreSampleDb);
            buttonRestoreSampleDb.Click += ButtonRestoreSampleDb_Click;

            Button buttonRestoreDb0 = FindViewById<Button>(Resource.Id.SettingsButton_RestoreDb0);
            buttonRestoreDb0.Click += ButtonRestoreDb0_Click; 

            Button buttonCompressDb = FindViewById<Button>(Resource.Id.SettingsButton_Compress);
            buttonCompressDb.Click += ButtonCompressDb_Click;

            Button buttonRepairDb = FindViewById<Button>(Resource.Id.SettingsButton_Repair);
            buttonRepairDb.Click += ButtonRepairDb_Click;

            // Datenbanken verwalten

            Button buttonNewDb = FindViewById<Button>(Resource.Id.SettingsButton_DatabaseNew);
            buttonNewDb.Click += ButtonNewDb_Click;

            Button buttonRenameDb = FindViewById<Button>(Resource.Id.SettingsButton_DatabaseRename);
            buttonRenameDb.Click += ButtonRenameDb_Click;

            Button buttonDeleteDb = FindViewById<Button>(Resource.Id.SettingsButton_DatabaseDelete);
            buttonDeleteDb.Click += ButtonDeleteDb_Click;

            Button buttonImportDb = FindViewById<Button>(Resource.Id.SettingsButton_DatabaseImport);
            buttonImportDb.Click += ButtonImportDb_Click;
            

            Button buttonSendLogFile =  FindViewById<Button>(Resource.Id.SettingsButton_SendLogFile);
            buttonSendLogFile.Click += ButtonSendLogFile_Click;
                        
            EditText addDbPath = FindViewById<EditText>(Resource.Id.SettingsButton_AdditionalDatabasePath);
            
            addDbPath.Text = Settings.GetString("AdditionslDatabasePath", string.Empty);

            addDbPath.TextChanged += delegate { this.additionalDatabasePathChanged = true; };

            Button buttonLicenses = FindViewById<Button>(Resource.Id.SettingsButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(LicensesActivity))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            buttonBackup.Click += ButtonBackup_Click;

            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            buttonRestore.Click += ButtonRestore_Click;

            Switch switchAskForBackup = FindViewById<Switch>(Resource.Id.SettingsButton_AskForBackup);
            switchAskForBackup.Click += AskForBackup_Click;
            switchAskForBackup.Checked = Settings.GetBoolean("AskForBackup", true);

            EditText editTextBackupPath = FindViewById<EditText>(Resource.Id.SettingsButton_BackupPath);
            editTextBackupPath.Text = this.GetBackupPath();
            editTextBackupPath.TextChanged += delegate
            {
                Settings.PutString("BackupPath", editTextBackupPath.Text);
            };

            Button buttonCsvExportArticles = FindViewById<Button>(Resource.Id.SettingsButton_CsvExportArticles);
            buttonCsvExportArticles.Click += ButtonCsvExportArticles_Click;

            Button buttonCsvExportStorageItems = FindViewById<Button>(Resource.Id.SettingsButton_CsvExportStorageItems);
            buttonCsvExportStorageItems.Click += ButtonCsvExportStorageItems_Click;

            this.ShowUserDefinedCategories();

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            catEdit.TextChanged += delegate { this.userCategoriesChanged = true; };

            // Fest definierte Kategorien
            string[] defaultCategories = Resources.GetStringArray(Resource.Array.ArticleCatagories);

            // Frei definierte Kategorien zusätzlich laden.
            var categories = MainActivity.GetDefinedCategories(defaultCategories);

            ArrayAdapter<String> categoryAdapter = new ArrayAdapter<String>(this, Resource.Layout.SpinnerItem, categories);
            categoryAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);
            
            Spinner categorySpinner = this.FindViewById<Spinner>(Resource.Id.Settings_DefaultCategory);
            categorySpinner.Adapter = categoryAdapter;
            categorySpinner.ItemSelected += CategorySpinner_ItemSelected;

            if (Android_Database.SQLiteConnection != null)
            {
                string defaultCategory = Database.GetSettingsString("DEFAULT_CATEGORY");

                int position = categoryAdapter.GetPosition(defaultCategory);

                if (position < 0)
                    position = 0;

                categorySpinner.SetSelection(position);

                this.EnableButtons();

                this.ShowLastBackupDay();
            }

            this.ShowApplicationVersion();


            bool createBackup = Intent.GetBooleanExtra("CreateBackup", false);

            if (createBackup)
            {
                this.CreateBackup();
            }

            this.Window.SetSoftInputMode(SoftInput.StateHidden);

            this.isInitialize = false;
        }

        private void UseFrontCamera_Click(object sender, EventArgs e)
        {
            Switch toggle = sender as Switch;
            
            Settings.PutBoolean("UseFrontCameraForEANScan", toggle.Checked);
        }

        private void CategorySpinner_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            if (this.isInitialize)
                return;

            try
            {
                Spinner spinner = sender as Spinner;
                var item = (String)spinner.Adapter.GetItem(e.Position);

                MainActivity.SetDefaultCategory(item);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private void ButtonCsvExportArticles_Click(object sender, EventArgs e)
        {
            try
            {
                CsvExport.ExportArticles(this);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private void ButtonCsvExportStorageItems_Click(object sender, EventArgs e)
        {
            try
            {
                CsvExport.ExportStorageItems(this);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();
            }
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
            if (!this.SaveAdditionalDatabasePath())
            {
                return;
            }
            base.OnBackPressed();
        }

        private bool SaveAdditionalDatabasePath()
        {
            if (!this.additionalDatabasePathChanged)
                return true;

            EditText dbPath = this.FindViewById<EditText>(Resource.Id.SettingsButton_AdditionalDatabasePath);

            if (!string.IsNullOrEmpty(dbPath.Text))
            {
                bool dirExists = Directory.Exists(dbPath.Text);
                if (!dirExists)
                {
                    string message = string.Format("Der eingegebene zusätzlicher Datenbankpfad '{0}' existiert nicht oder Sie haben kein Zugriff dadrauf.", dbPath.Text);

                    var messageBox = new AlertDialog.Builder(this);
                    messageBox.SetMessage(message);
                    messageBox.SetPositiveButton("OK", (s, evt) => { });
                    messageBox.Create().Show();

                    return false;
                }
            }

            Settings.PutString("AdditionslDatabasePath", dbPath.Text);

            return true;
        }

        private void SaveUserDefinedCategories()
        {
            if (!this.userCategoriesChanged)
                return;

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            MainActivity.SetUserDefinedCategories(catEdit?.Text);
        }

        internal string GetBackupFileName(string databaseFilePath)
        {
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

            var downloadFolder = this.GetBackupPath();

            var backupFilePath = Path.Combine(downloadFolder, backupFileName);

            return backupFilePath;
        }

        private string GetBackupPath()
        {
            string downloadFolder = Settings.GetString("BackupPath", string.Empty);

            if (string.IsNullOrEmpty(downloadFolder))
            {
                downloadFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(
                                    Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            }

            return downloadFolder;
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
                        // Sich neu connecten;
                        Android_Database.Instance.CloseConnection();

                        // Löschen von '*.db3-shn' und '*.db3-wal'
                        //string deleteFile1 = Path.ChangeExtension(fileDestination, "db3-shm");
                        //File.Delete(deleteFile1);

                        string deleteFile2 = Path.ChangeExtension(fileDestination, "db3-wal");

                        if (File.Exists(deleteFile2))
                            File.Delete(deleteFile2);

                        File.Copy(fileSource, fileDestination, true);

                        // Sich neu connecten;
                        Android_Database.SQLiteConnection = null;

                        var picturesToMove = Database.GetArticlesToCopyImages();

                        if (picturesToMove.Count > 0)
                        {

                            RunOnUiThread(() =>
                            {
                                message = string.Format(
                                    "Es müsen {0} Bilder übetragen werden.\n\n" +
                                    "Beenden Sie die App ganz und starten Sie diese neu.",
                                    picturesToMove.Count);

                                var dialog = new AlertDialog.Builder(this);
                                dialog.SetMessage(message);
                                dialog.SetTitle(Resource.String.App_Name);
                                dialog.SetIcon(Resource.Drawable.ic_launcher);
                                dialog.SetPositiveButton("OK", (s1, e1) => { });
                                dialog.Create().Show();
                            });

                        }

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
                    TRACE(ex);
                    
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
                        messageBox.SetTitle("Ergebnis der Prüfung:");
                        messageBox.SetMessage(result);
                        messageBox.SetPositiveButton("OK", (s, evt) => { });
                        messageBox.Create().Show();
                    });
                }
                catch(Exception ex)
                {
                    TRACE(ex);

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

        private async void ButtonNewDb_Click(object sender, EventArgs e)
        {
            string newDatabaseName = await MainActivity.InputTextAsync(
                this,
                "Neue Datenbank erstellen",
                "\n\nName eingeben:",
                string.Empty,
                "Erstellen",
                "Abbrechen");

            if (string.IsNullOrEmpty(newDatabaseName))
                return;

            Exception ex = Android_Database.Instance.CreateDatabaseOnAppStorage(this, newDatabaseName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton("OK", (s, e) => { });
                message.Create().Show();
            }
        }

        private void ButtonDeleteDb_Click(object sender, EventArgs e)
        {
            List<string> fileList;
            
            Exception ex = Android_Database.LoadDatabaseFileListSafe(this, out fileList);
            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton("OK", (s, e) => { });
                message.Create().Show();
            }

            if (fileList.Count == 0)
            {
                return;
            }

            string currentDatabaseName = Android_Database.Instance.GetDatabasePath();
            if (!string.IsNullOrEmpty(currentDatabaseName))
            {
                fileList.Remove(currentDatabaseName);
            }

            if (fileList.Count == 0)
            {
                return;
            }

            string[] databaseNames = new string[fileList.Count];

            for(int i = 0; i < fileList.Count; i++)
            {
                databaseNames[i] = Path.GetFileNameWithoutExtension(fileList[i]);
            }

            string selectedDatabasePath = null;

            var builder = new AlertDialog.Builder(this);
            builder.SetTitle("Datenbank löschen");
            builder.SetSingleChoiceItems(databaseNames, -1,
                new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs e) 
                {
                    // Get reference to AlertDialog
                    var d = (sender as Android.App.AlertDialog);
  
                    // Auswahl merken
                    selectedDatabasePath = fileList[e.Which];
                }));
            builder.SetPositiveButton("LÖSCHEN", (s, e) => 
                {
                    if (string.IsNullOrEmpty(selectedDatabasePath))
                        return;

                    Exception ex = Android_Database.Instance.DeleteDatabase(selectedDatabasePath);

                    if (ex != null)
                    {
                        var message = new AlertDialog.Builder(this);
                        message.SetMessage(ex.Message);
                        message.SetPositiveButton("OK", (s, e) => { });
                        message.Create().Show();
                    }
                });

            builder.SetNegativeButton("Abbruch", (s, e) => { });
            builder.Show();
        }

        private async void ButtonRenameDb_Click(object sender, EventArgs e)
        {
            string proDatabase = Android_Database.Instance.GetProductiveDatabasePath();

            string databasePath = await MainActivity.SelectDatabase(this, "Datenbank umbenennen:", proDatabase); ;

            if (string.IsNullOrEmpty(databasePath))
                return;

            string databaseName = Path.GetFileNameWithoutExtension(databasePath);

            string newDatabaseName = await MainActivity.InputTextAsync(
                this,
                "Datenbank umbenennen",
                "\n" + databasePath + "\n\nNeuen Name eingeben:",
                databaseName,
                "Umbenennen",
                "Abbrechen");

            if (string.IsNullOrEmpty(newDatabaseName))
                return;

            Exception ex = Android_Database.Instance.RenameDatabase(this, databasePath,  newDatabaseName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton("OK", (s, e) => { });
                message.Create().Show();
            }
        }


        private async void ButtonImportDb_Click(object sender, EventArgs e)
        {
            PickOptions options = new PickOptions();
            options.PickerTitle = "Datenbank oder Backup auswählen:";

            var file = await FilePicker.PickAsync(options);
            if (file == null)
                return;

            Exception ex = Android_Database.Instance.ImportDatabase(this, file.FullPath, file.FileName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton("OK", (s, e) => { });
                message.Create().Show();
            }

            return;
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

            try
            {
                Android_Database.Instance.GetConnection();
                
            }
            catch(Exception ex)
            {
                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();
            }

            this.ShowDatabaseInfo();
            this.ShowUserDefinedCategories();
            this.ShowLastBackupDay();
            this.EnableButtons();
        }

        private void ButtonSendLogFile_Click(object sender, EventArgs eventArgs)
        {
            if (MainActivity.IsGooglePlayPreLaunchTestMode)
                return;

            string message = "LOG Einträge an den Entwickler schicken?";
            message += "\n\nIhre E-Mail Adresse wird dem Entwickler als 'Absender' angezeigt. ";
            message += "Es werden keine private Daten versendet. ";
            message += "Vor dem Senden können Sie die Daten noch betrachten.";

            var dialog = new AlertDialog.Builder(this);
            dialog.SetMessage(message);
            dialog.SetPositiveButton("Ja", (s, e) => 
                {
                    Context context = this.ApplicationContext;
                    PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

                    var build = Build.RadioVersion;

                    StringBuilder text = new StringBuilder();
                    text.AppendFormat("{0}\n", this.GetApplicationVersion());
                    text.AppendFormat("Current Database: {0}\n", Android_Database.SQLiteConnection?.DatabasePath);
                    text.AppendFormat("Android Version: {0}\n",  Build.VERSION.Release);
                    text.AppendFormat("Android SDK: {0}\n",      Build.VERSION.SdkInt);
                    text.AppendFormat("Hersteller: {0}\n",       Build.Manufacturer);
                    text.AppendFormat("Modell: {0}\n",           Build.Model);
                    text.AppendFormat("CurrentCulture: {0}\n",   CultureInfo.CurrentCulture.DisplayName);
                    text.AppendFormat("CurrentUICulture: {0}\n", CultureInfo.CurrentUICulture.DisplayName);

                    text.AppendLine();
                    text.AppendFormat(Logging.GetLogFileText());

                    System.Diagnostics.Trace.WriteLine(text.ToString());

                    string subject = "Vue_LOG_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");

                    var emailIntent = new Intent(Intent.ActionSend);
                    emailIntent.PutExtra(Android.Content.Intent.ExtraEmail,   new[] { "cstryi@freenet.de" });
                    emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, subject);
                    emailIntent.PutExtra(Android.Content.Intent.ExtraText,    text.ToString());
                    //emailIntent.SetType("message/rfc822");
                    emailIntent.SetType("text/plain");
                    StartActivity(Intent.CreateChooser(emailIntent, "E-Mail an Entwickler senden mit..."));
                });
            dialog.SetNegativeButton("Nein", (s, e) => { });
            dialog.Create().Show();
        }

        private void SwitchCostMessage_Click(object sender, EventArgs e)
        {
            var switchCostMessage = sender as Switch;
            ArticleDetailsActivity.showCostMessage = switchCostMessage.Checked;
            Settings.PutBoolean("ShowOpenFoodFactsInternetCostsMessage", ArticleDetailsActivity.showCostMessage);
        }

        private void SwitchAltDatePicker_Click(object sender, EventArgs e)
        {
            var switchAltDatePicker = sender as Switch;
            StorageItemQuantityActivity.UseAltDatePicker = switchAltDatePicker.Checked;
            Settings.PutBoolean("UseAltDatePicker", StorageItemQuantityActivity.UseAltDatePicker);
        }

        private void CheckDatabaseFilesToMove_Click(object sender, EventArgs e)
        {
            var switchMoveDb = sender as Switch;
            bool isChecked = switchMoveDb.Checked;
            Settings.PutBoolean("CheckDatabaseFilesToMove", isChecked);
        }


        private void CompressPictures_Click(object sender, EventArgs e)
        {
            var switchCompressPictures = sender as Switch;
            Settings.PutBoolean("CompressPictures", switchCompressPictures.Checked);

            FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Small) .Enabled = switchCompressPictures.Checked;
            FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Middle).Enabled = switchCompressPictures.Checked;
            FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Big)   .Enabled = switchCompressPictures.Checked;
            FindViewById<RadioButton>(Resource.Id.SettingsButton_CompressPictures_Huge)  .Enabled = switchCompressPictures.Checked;
        }
        
        private void CompressMode_Click(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.Id == Resource.Id.SettingsButton_CompressPictures_Small)
            {
                Settings.PutInt("CompressPicturesMode", 1);
            }
            if (radioButton.Id == Resource.Id.SettingsButton_CompressPictures_Middle)
            {
                Settings.PutInt("CompressPicturesMode", 2);
            }
            if (radioButton.Id == Resource.Id.SettingsButton_CompressPictures_Big)
            {
                Settings.PutInt("CompressPicturesMode", 3);
            }
            if (radioButton.Id == Resource.Id.SettingsButton_CompressPictures_Huge)
            {
                Settings.PutInt("CompressPicturesMode", 4);
            }
        }


        private void ButtonRestore_Click(object sender, EventArgs e)
        {
            // Backups müssen sich im Download Verzeichnis befinden.
            var downloadFolder = this.GetBackupPath();
            var databasePath = Android_Database.SelectedDatabaseName;

            if (string.IsNullOrEmpty(databasePath))
            {
                Toast.MakeText(this, "Keine Datenbank ausgewählt.", ToastLength.Long).Show();
                return;
            }

            var selectFile = new Intent(this, typeof(BackupFileManageActivity));
            selectFile.PutExtra("Text",         "Backup auswählen:");
            selectFile.PutExtra("Path",          downloadFolder);
            selectFile.PutExtra("SearchPattern", "*.VueBak");

            StartActivityForResult(selectFile, SelectBackupId);
        }

        private void AskForBackup_Click(object sender, EventArgs e)
        {
            var switchAskForBackup = sender as Switch;
            Settings.PutBoolean("AskForBackup", switchAskForBackup.Checked);
        }
 
        private void CreateBackup()
        {
            bool isGranted = new SdCardAccess().Grand(this);

            if (!isGranted)
            {
                Toast.MakeText(this, "Keine Berechtigung auf Dateien und Medien für diese App vergeben.", ToastLength.Long).Show();
                return;
            }

            this.CreateBackupInternal();
        }

        private void CreateBackupInternal()
        {
            // Vor dem Backup ggf. die User-Kategorien ggf. speichern,
            // damit es auch im Backup ist.
            this.SaveUserDefinedCategories();

            DateTime? lastBackupDay = Database.GetSettingsDate("LAST_BACKUP");

            // Datum vom Backup in der Datenbank speichern.
            Database.SetSettingsDate("LAST_BACKUP", DateTime.Today);

            this.ShowLastBackupDay();

            var databaseFilePath = Android_Database.Instance.GetProductiveDatabasePath();

            string backupFilePath = this.GetBackupFileName(databaseFilePath);

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
                    TRACE(ex);

                    message = ex.Message;

                    if (lastBackupDay != null)
                    {
                        // Datum vom wieder zurückspielen.
                        Database.SetSettingsDate("LAST_BACKUP", lastBackupDay.Value);
                    }

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

            buttonBackup.Enabled  = !Android_Database.UseTestDatabase;
            buttonRestore.Enabled = !Android_Database.UseTestDatabase;

            TextView lastBackup = FindViewById<TextView>(Resource.Id.Settings_LastBackupDay);
            lastBackup.Enabled = !Android_Database.UseTestDatabase;
        }

        private void ShowUserDefinedCategories()
        {
            string categoryText = string.Empty;

            // Nur, wenn Datenbankverbindung besteht
            if (Android_Database.SQLiteConnection != null)
            {
                var categories = MainActivity.GetUserDefinedCategories();
                foreach(string category in categories)
                {
                    categoryText += category + ", ";
                }
            }

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            catEdit.Text = categoryText;
            catEdit.SetSelection(categoryText.Length);
            catEdit.Enabled = Android_Database.SQLiteConnection != null;

            this.userCategoriesChanged = false;
        }

        private void ShowLastBackupDay()
        {
            string lastBackupDay = string.Empty;
            try
            {
                lastBackupDay = Database.GetSettingsDate("LAST_BACKUP")?.ToShortDateString();
            }
            catch(Exception ex)
            {
                TRACE(ex);
            }

            TextView lastBackupDayView = FindViewById<TextView>(Resource.Id.Settings_LastBackupDay);
            lastBackupDayView.Text = string.Format("Letzter Backup am: {0}", lastBackupDay);
        }


        private void ShowApplicationVersion()
        {
            TextView versionInfo = FindViewById<TextView>(Resource.Id.SettingsButton_Version);
            versionInfo.Text = this.GetApplicationVersion();
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

            try
            {
                var prozessoren = String.Join<string>(',', Android.OS.Build.SupportedAbis);
                versionInfo += string.Format("\nProzessor: {0}", prozessoren);
            }
            catch(Exception e)
            {
                TRACE("SettingsActivity.ShowApplicationVersion() - Prozessoren Exception: {0}", e.Message);
            }

            return versionInfo;
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

        #region private events

        private void ButtonBackup_Click(object sender, EventArgs eventArgs)
        {
            try
            {
                this.CreateBackup();
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle("Fehler aufgetreten!");
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton("OK", (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        #endregion

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            bool canWriteExternalStorage = false;

            for(int i = 0; i < permissions.Length; i++)
            {
                string permission      = permissions[i];
                Permission grantResult = grantResults[i];

                if (permission.Equals(Android.Manifest.Permission.WriteExternalStorage)
                    && grantResult == Permission.Granted)
                {
                    canWriteExternalStorage = true;
                }
            }

            if (!canWriteExternalStorage)
            {
                TRACE("Permission to external storage is not granted.");
                return;
            }

            this.CreateBackupInternal();
        }
    }
}