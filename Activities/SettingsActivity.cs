using System;
using System.IO;
using System.Text;
using System.Drawing;
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
using Android.Graphics.Drawables;
using Android.Support.V4.Graphics;
using Android.Service.Controls.Actions;

using Xamarin.Essentials;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "@string/Settings_Title")]
    public class SettingsActivity : Activity
    {
        public static readonly int SelectBackupId = 1000;
        public static readonly int ShareFileId    = 1001;

        private string shareFileName;

        private bool userCategoriesChanged = false;
        private bool additionalDatabasePathChanged = false;
        private bool isInitialize = false;

        private int textViewColor;

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

            Switch switchCostMessage = FindViewById<Switch>(Resource.Id.SettingsButton_ShowOFFCostMessage);
            switchCostMessage.Click += SwitchCostMessage_Click;
            switchCostMessage.Checked = ArticleDetailsActivity.showCostMessage;

            Switch switchAltDatePicker = FindViewById<Switch>(Resource.Id.SettingsButton_AltDatePicker);
            switchAltDatePicker.Click += SwitchAltDatePicker_Click;
            switchAltDatePicker.Checked = StorageItemQuantityActivity.UseAltDatePicker;
            
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

            TextView logFile = FindViewById<TextView>(Resource.Id.Settings_LogFile);
            logFile.Text = String.Format("LOG: " + Tools.GetLogFileName());
            logFile.Click += LogFile_Click;
                        
            EditText addDbPath = FindViewById<EditText>(Resource.Id.Settings_AdditionalDatabasePath);
            
            addDbPath.Text = Settings.GetString("AdditionslDatabasePath", string.Empty);

            addDbPath.TextChanged += delegate { this.additionalDatabasePathChanged = true; };

            Button buttonLicenses = FindViewById<Button>(Resource.Id.SettingsButton_Licenses);
            buttonLicenses.Click += delegate { StartActivity(new Intent(this, typeof(LicensesActivity))); };

            Button buttonBackup = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            buttonBackup.Click += ButtonBackup_Click;

            Button buttonBackupToFile = FindViewById<Button>(Resource.Id.SettingsButton_BackupToFile);
            buttonBackupToFile.Click += ButtonBackupToFile_Click;

            Button buttonRestore = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            buttonRestore.Click += ButtonRestore_Click;

            Button buttonRestoreFromFile = FindViewById<Button>(Resource.Id.SettingsButton_RestoreFromFile);
            buttonRestoreFromFile.Click += ButtonRestoreFromFile_Click;

            this.textViewColor = FindViewById<TextView>(Resource.Id.Settings_BackupFileCount).CurrentTextColor;

            Switch switchAskForBackup = FindViewById<Switch>(Resource.Id.SettingsButton_AskForBackup);
            switchAskForBackup.Click += AskForBackup_Click;
            switchAskForBackup.Checked = Settings.GetBoolean("AskForBackup", true);

            var backupPathList = Android_Database.GetStoragesPaths(this, Android.OS.Environment.DirectoryDownloads);

            ArrayAdapter<String> backupPathsAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, backupPathList);

            var editTextBackupPath = FindViewById<AutoCompleteTextView>(Resource.Id.SettingsButton_BackupPath);
            editTextBackupPath.Adapter = backupPathsAdapter;
            editTextBackupPath.Threshold = 1;
            editTextBackupPath.Text = this.GetBackupPath();
            editTextBackupPath.TextChanged += delegate
            {
                Settings.PutString("BackupPath", editTextBackupPath.Text);
            };

            FindViewById<Button>(Resource.Id.SettingsButton_SelectBackupPath).Click += SelectBackupPath_Click;

            Button buttonCsvExportArticles = FindViewById<Button>(Resource.Id.SettingsButton_CsvExportArticles);
            buttonCsvExportArticles.Click += ButtonCsvExportArticles_Click;

            Button buttonCsvExportStorageItems = FindViewById<Button>(Resource.Id.SettingsButton_CsvExportStorageItems);
            buttonCsvExportStorageItems.Click += ButtonCsvExportStorageItems_Click;

            int csvSeparatorType = Settings.GetInt("CsvExportSeparator", 1);

            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Comma).Checked     = (csvSeparatorType == 1);
            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Semicolon).Checked = (csvSeparatorType == 2);
            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Tab).Checked       = (csvSeparatorType == 3);

            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Comma).Click      += CsvSeparatorType_Click;
            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Semicolon).Click  += CsvSeparatorType_Click;
            FindViewById<RadioButton>(Resource.Id.Settings_CSVSeparator_Tab).Click        += CsvSeparatorType_Click;
            
            this.ShowUserDefinedCategories();

            EditText catEdit = this.FindViewById<EditText>(Resource.Id.Settings_Categories);
            catEdit.TextChanged += delegate { this.userCategoriesChanged = true; };

            // Fest definierte Kategorien
            string[] defaultCategories = Resources.GetStringArray(Resource.Array.ArticleCatagories);

            // Frei definierte Kategorien zusätzlich laden.
            var categories = MainActivity.GetDefinedCategories(defaultCategories);
            
            var textResource = this.GetTextResource();
            ArrayAdapter<String> categoryAdapter = new ArrayAdapter<String>(this, textResource, categories);
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
                if ((int)Build.VERSION.SdkInt >= 33)
                {
                    this.ShareBackup();
                }
                else
                {
                    this.CreateBackup();
                }
            }

            this.Window.SetSoftInputMode(SoftInput.StateHidden);

            if ((int)Build.VERSION.SdkInt >= 33)
            {
                buttonBackup.Visibility = ViewStates.Gone;
                buttonRestore.Visibility = ViewStates.Gone;

                FindViewById<TextView>(Resource.Id.Settings_BackupFileCount).Visibility = ViewStates.Gone;
                FindViewById<TextView>(Resource.Id.Settings_BackupPath).Visibility = ViewStates.Gone;
                FindViewById<RelativeLayout>(Resource.Id.Settings_BackupPath_Region).Visibility = ViewStates.Gone;

                FindViewById<TextView>(Resource.Id.SettingsLabel_AdditionalDatabasePath).Visibility = ViewStates.Gone;
                FindViewById<EditText>(Resource.Id.Settings_AdditionalDatabasePath).Visibility = ViewStates.Gone;
            }
            else
            {
                this.DetectBackupsCount();
            }

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                switchToTestDB.Enabled = false;
                buttonSendLogFile.Enabled = false;
                buttonImportDb.Enabled = false;
                buttonCsvExportArticles.Enabled = false;
                buttonCsvExportStorageItems.Enabled = false;

                FindViewById<TextView>(Resource.Id.Settings_Contact_EmailTextView).Enabled = false;
                FindViewById<TextView>(Resource.Id.Settings_AppWiki_TextView).Enabled = false;
                FindViewById<TextView>(Resource.Id.Settings_DatenschutTextView).Enabled = false;
            }


            this.isInitialize = false;
        }

        private void LogFile_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder messageBox = new AlertDialog.Builder(this);
            messageBox.SetTitle("Absturz Test");
            messageBox.SetMessage("Dieser Testabsturz dienst nur für interne Tests des Entwicklers und wird im Normalfall nicht gebraucht (bitte auf 'Abbrechen' klicken).");
            messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => { });
            messageBox.SetNegativeButton("ABSTURZ", (s, e) => 
            {
                throw new ApplicationException("Das ist ein Testabsturz.");
            });
            messageBox.SetNeutralButton("LOG Datei löschen", (s,e) => 
            {
                Logging.DeleteCurrentLogFile();
            });
            messageBox.Show();
        }

        private int GetTextResource()
        {
            var textResource = Resource.Layout.Spinner_Gray;

            var backgroundColor = this.GeBackgroundColor();
            if (backgroundColor != null)
            {
                textResource = Resource.Layout.Spinner_Black;

                if (this.IstFarbeDunklerAlsGray(backgroundColor.Value))
                {
                    textResource = Resource.Layout.Spinner_White;
                }
            }

            return textResource;
        }

        private void SelectBackupPath_Click(object sender, EventArgs e)
        {
            var backupPathList = Android_Database.GetStoragesPaths(this, Android.OS.Environment.DirectoryDownloads);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.Settings_BackupPath);
            builder.SetItems(backupPathList.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.SettingsButton_BackupPath);
                textView.Text = backupPathList[a.Which];

                this.DetectBackupsCount();
            });
            builder.Show();
        }

        private Android.Graphics.Color? GeBackgroundColor()
        {
            ColorDrawable backgroundColor = this.Window.DecorView.Background as ColorDrawable;
            if (backgroundColor != null)
            {
                return backgroundColor.Color;
            }

            Drawable backgroundColor2 = this.Window.DecorView.Background as Drawable;
            if (backgroundColor2 != null)
            {
                ColorDrawable currentDrawable = backgroundColor2.Current as ColorDrawable;
                return currentDrawable.Color;
            }

            return null;
        }

        private void DetectBackupsCount()
        {
            TextView backupCount = FindViewById<TextView>(Resource.Id.Settings_BackupFileCount);
            string backupInfo = Resources.GetString(Resource.String.Settings_BackupFileCount);

            string sizeText  = "-";
            string fileCount = "-";

            backupCount.Text = string.Format(backupInfo, fileCount, sizeText);

            try
            {
                string backupPath = this.GetBackupPath();

                var databaseFilePath = Android_Database.Instance.GetProductiveDatabasePath();
                string databaseFileName = Path.GetFileNameWithoutExtension(databaseFilePath);

                string backupFileName = databaseFileName + "_*.VueBak";

                if (databaseFileName == "Vorraete")
                {
                    backupFileName = "Vue_*.VueBak";
                }

                FileInfo[] fileListUnsorted = new FileInfo[0];

                try
                {
                    fileListUnsorted = new DirectoryInfo(backupPath).GetFiles(backupFileName);
                }
                catch
                {
                    // Exception, wenn noch kein Zugriff gewährt wurde.
                }

                long sumSize = 0;
                foreach(var file in fileListUnsorted)
                {
                    sumSize += file.Length;
                }

                sizeText  = Tools.ToFuzzyByteString(sumSize);
                fileCount = fileListUnsorted.Length.ToString();

                if (fileListUnsorted.Length > 5)
                {
                    backupCount.SetTypeface(null, Android.Graphics.TypefaceStyle.Bold);
                    backupCount.SetTextColor(Android.Graphics.Color.Red);
                }
                else
                {
                    backupCount.SetTypeface(null, Android.Graphics.TypefaceStyle.Normal);
                    backupCount.SetTextColor(new Android.Graphics.Color(this.textViewColor));
                }
            }
            catch(Exception ex)
            {
                TRACE("Error detecting backups count and size.");
                TRACE(ex);

                sizeText  = "?";
                fileCount = "?";
            }

            backupCount.Text = string.Format(backupInfo, fileCount, sizeText);
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
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private void ButtonCsvExportArticles_Click(object sender, EventArgs e)
        {
            try
            {
                this.shareFileName = CsvExport.ExportArticles(this, this.GetCsvSeparator(), ShareFileId);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private void ButtonCsvExportStorageItems_Click(object sender, EventArgs e)
        {
            try
            {
                this.shareFileName = CsvExport.ExportStorageItems(this, this.GetCsvSeparator(), ShareFileId);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }
        }


        private void CsvSeparatorType_Click(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            if (radioButton.Id == Resource.Id.Settings_CSVSeparator_Comma)
            {
                Settings.PutInt("CsvExportSeparator", 1);
            }
            if (radioButton.Id == Resource.Id.Settings_CSVSeparator_Semicolon)
            {
                Settings.PutInt("CsvExportSeparator", 2);
            }
            if (radioButton.Id == Resource.Id.Settings_CSVSeparator_Tab)
            {
                Settings.PutInt("CsvExportSeparator", 3);
            }
        }

        private string GetCsvSeparator()
        {
            int csvSeparatorType = Settings.GetInt("CsvExportSeparator", 1);
            switch (csvSeparatorType)
            {
                case 1: return ",";
                case 2: return ";";
                case 3: return "\t";
            }

            return ",";
        }


        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void Finish()
        {
            this.SaveUserDefinedCategories();
            if (!this.SaveAdditionalDatabasePath())
            {
                return;
            }
            base.Finish();
        }

        private bool SaveAdditionalDatabasePath()
        {
            if (!this.additionalDatabasePathChanged)
                return true;

            EditText dbPath = this.FindViewById<EditText>(Resource.Id.Settings_AdditionalDatabasePath);

            if (!string.IsNullOrEmpty(dbPath.Text))
            {
                bool dirExists = Directory.Exists(dbPath.Text);
                if (!dirExists)
                {
                    string message = this.Resources.GetString(Resource.String.Settings_ErrorSavingAdditionalDatabasePath);
                    message = string.Format(message, dbPath.Text);

                    var messageBox = new AlertDialog.Builder(this);
                    messageBox.SetMessage(message);
                    messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
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

        internal string GetBackupFileName(string databaseFilePath, bool tempFileFolder = false)
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

            var downloadFolder = this.GetBackupPath(tempFileFolder);

            var backupFilePath = Path.Combine(downloadFolder, backupFileName);

            return backupFilePath;
        }

        private string GetBackupPath(bool tempFileFolder = false)
        {
            if (tempFileFolder)
            {
                return System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
            }

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

            if (requestCode == SelectBackupId)
            {
                this.DetectBackupsCount();
            }

            if (requestCode == ShareFileId)
            {
                this.DeleteShareFile();
            }

            if ((resultCode == Result.Ok) && (requestCode == SelectBackupId) && (data != null))
            {
                string fileSource = data.GetStringExtra("FullName");

                this.RestoreDatabase(fileSource);
            }

        }

        private void DeleteShareFile()
        {
            if (String.IsNullOrEmpty(this.shareFileName))
            {
                return;
            }

            try
            {
                var sharePath = Path.GetDirectoryName(this.shareFileName);

                foreach (var file in Directory.GetFiles(sharePath))
                {
                    TRACE("- {0}", file);
                }

                TRACE("Die Datei '{0}' löschen, die gerade versendet wurde (Share).", this.shareFileName);

                File.Delete(this.shareFileName);
                this.shareFileName = null;

                // Ggf. alle liegengebliebene Backups zum Versenden löschen?
                // Die sind teilweise sehr groß.
                var filesList = Directory.GetFiles(sharePath, "*.VueBak");
                foreach (var file in filesList)
                {
                    var extension = Path.GetExtension(file);
                    if (extension == ".VueBak")
                    {
                        TRACE("Backup Datei '{0}' löschen, die beim Versenden liegengeblieben ist (z.B. wegen Abbruch).", file);
                        File.Delete(file);
                    }
                }

            }
            catch (Exception ex)
            {
                TRACE(ex);
            }
        }

        private async void RestoreDatabase(string fileSource)
        {
            string fileDestination = Android_Database.Instance.GetProductiveDatabasePath();
                
            string databaseName = Tools.GetBackupDatabaseName(fileSource);

            string newDatabaseName = await MainActivity.InputTextAsync(
                this,
                this.Resources.GetString(Resource.String.Settings_DatabaseRestore),
                this.Resources.GetString(Resource.String.Settings_DatabaseRestoreName),
                databaseName,
                this.Resources.GetString(Resource.String.App_Ok),
                this.Resources.GetString(Resource.String.App_Cancel));

            if (string.IsNullOrEmpty(newDatabaseName))
            {
                return;
            }

            newDatabaseName = newDatabaseName + ".db3";

            fileDestination = Path.GetDirectoryName(fileDestination);
            fileDestination = Path.Combine(fileDestination, newDatabaseName);

            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_BackupAndRestore);
            new Thread(new ThreadStart(delegate
            {
                // Sich neu connecten;
                Android_Database.Instance.CloseConnection();

                string deleteFile2 = Path.ChangeExtension(fileDestination, "db3-wal");

                try
                {
                    if (File.Exists(deleteFile2))
                        File.Delete(deleteFile2);

                    File.Copy(fileSource, fileDestination, true);

                }
                catch (Exception ex)
                {
                    TRACE(ex);
                    RunOnUiThread(() =>
                    {
                        var text = string.Format("{0}\n\n{1}",
                            ex.Message, 
                            this.Resources.GetString(Resource.String.App_CheckPermissions));

                        var message = new AlertDialog.Builder(this);
                        message.SetMessage(text);
                        message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                        message.SetNegativeButton("App Info", (s, evt) => { AppInfo.ShowSettingsUI();});
                        message.Create().Show();
                
                        this.HideProgressBar(progressDialog);
                    });

                    return;
                }


                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                var picturesToMove = Database.GetArticlesToCopyImages();

                if (picturesToMove.Count > 0)
                {

                    RunOnUiThread(() =>
                    {
                        string message = this.Resources.GetString(Resource.String.Settings_ConvertDatabaseForPicture);
                        message = string.Format(message, picturesToMove.Count);

                        var dialog = new AlertDialog.Builder(this);
                        dialog.SetMessage(message);
                        dialog.SetTitle(Resource.String.App_Name);
                        dialog.SetIcon(Resource.Drawable.ic_launcher);
                        dialog.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s1, e1) => { });
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
                        messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                        messageBox.SetMessage(ex.Message);
                        messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
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
                        messageBox.SetTitle(this.Resources.GetString(Resource.String.Settings_CheckReport));
                        messageBox.SetMessage(result);
                        messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                        messageBox.Create().Show();
                    });
                }
                catch(Exception ex)
                {
                    TRACE(ex);

                    RunOnUiThread(() =>
                    {
                        var messageBox = new AlertDialog.Builder(this);
                        messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                        messageBox.SetMessage(ex.Message);
                        messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
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
                this.Resources.GetString(Resource.String.Settings_DatabaseNewDialogTitle),
                this.Resources.GetString(Resource.String.Settings_DatabaseNewDialogMessage),
                string.Empty,
                this.Resources.GetString(Resource.String.App_Create),
                this.Resources.GetString(Resource.String.App_Cancel));

            if (string.IsNullOrEmpty(newDatabaseName))
                return;

            Exception ex = Android_Database.Instance.CreateDatabaseOnAppStorage(this, newDatabaseName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
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
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
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
            builder.SetTitle(this.Resources.GetString(Resource.String.Settings_DeleteDatabase));
            builder.SetSingleChoiceItems(databaseNames, -1,
                new EventHandler<DialogClickEventArgs>(delegate (object sender, DialogClickEventArgs e) 
                {
                    // Get reference to AlertDialog
                    var d = (sender as Android.App.AlertDialog);
  
                    // Auswahl merken
                    selectedDatabasePath = fileList[e.Which];
                }));
            builder.SetPositiveButton(this.Resources.GetString(Resource.String.App_DeleteBig), (s, e) => 
                {
                    if (string.IsNullOrEmpty(selectedDatabasePath))
                        return;

                    Exception ex = Android_Database.Instance.DeleteDatabase(selectedDatabasePath);

                    if (ex != null)
                    {
                        var message = new AlertDialog.Builder(this);
                        message.SetMessage(ex.Message);
                        message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
                        message.Create().Show();
                    }
                });

            builder.SetNegativeButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => { });
            builder.Show();
        }

        private async void ButtonRenameDb_Click(object sender, EventArgs e)
        {
            string proDatabase = Android_Database.Instance.GetProductiveDatabasePath();

            string databasePath = await MainActivity.SelectDatabase(this,
                this.Resources.GetString(Resource.String.Settings_DatabaseRenameDialogTitle), 
                proDatabase);

            if (string.IsNullOrEmpty(databasePath))
                return;

            string databaseName = Path.GetFileNameWithoutExtension(databasePath);

            string newDatabaseName = await MainActivity.InputTextAsync(
                this,
                this.Resources.GetString(Resource.String.Settings_DatabaseRenameDialogTitle),
                "\n" + databasePath + 
                this.Resources.GetString(Resource.String.Settings_DatabaseRenameDialogMessage),
                databaseName,
                this.Resources.GetString(Resource.String.App_Rename),
                this.Resources.GetString(Resource.String.App_Cancel));

            if (string.IsNullOrEmpty(newDatabaseName))
                return;

            Exception ex = Android_Database.Instance.RenameDatabase(this, databasePath,  newDatabaseName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
                message.Create().Show();
            }
        }


        private async void ButtonImportDb_Click(object sender, EventArgs e)
        {
            PickOptions options = new PickOptions();
            options.PickerTitle = this.Resources.GetString(Resource.String.Settings_SelectBackupOrImport);

            var file = await FilePicker.PickAsync(options);
            if (file == null)
                return;

            string databaseName = Tools.GetBackupDatabaseName(file.FullPath);

            string newDatabaseName = await MainActivity.InputTextAsync(
                this,
                this.Resources.GetString(Resource.String.Settings_DatabaseImport),
                this.Resources.GetString(Resource.String.Settings_DatabaseImportName),
                databaseName,
                this.Resources.GetString(Resource.String.App_Ok),
                this.Resources.GetString(Resource.String.App_Cancel));

            if (string.IsNullOrEmpty(newDatabaseName))
            {
                return;
            }

            Exception ex = Android_Database.Instance.ImportDatabase(this, file.FullPath, newDatabaseName);

            if (ex != null)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(ex.Message);
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
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
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }

            this.ShowDatabaseInfo();
            this.ShowUserDefinedCategories();
            this.ShowLastBackupDay();
            this.EnableButtons();
        }

        private void ButtonSendLogFile_Click(object sender, EventArgs eventArgs)
        {
            string message = this.Resources.GetString(Resource.String.Settings_SendLogFileMessage);

            var dialog = new AlertDialog.Builder(this);
            dialog.SetMessage(message);
            dialog.SetPositiveButton(this.Resources.GetString(Resource.String.App_Yes), (s, e) => 
                {
                    Context context = this.ApplicationContext;
                    PackageInfo info = context.PackageManager.GetPackageInfo(context.PackageName, 0);

                    var build = Build.RadioVersion;

                    StringBuilder text = new StringBuilder();
                    text.AppendFormat("{0}\n", this.GetApplicationVersion());
                    text.AppendFormat("Current Database: {0}\n", Android_Database.SQLiteConnection?.DatabasePath);
                    text.AppendFormat("Android Version: {0}\n",  Build.VERSION.Release);
                    text.AppendFormat("Android SDK: {0}\n",      Build.VERSION.SdkInt);
                    text.AppendFormat("Manufacturer: {0}\n",     Build.Manufacturer);
                    text.AppendFormat("Modell: {0}\n",           Build.Model);
                    text.AppendFormat("CurrentCulture: {0}\n",   CultureInfo.CurrentCulture.DisplayName);
                    text.AppendFormat("CurrentUICulture: {0}\n", CultureInfo.CurrentUICulture.DisplayName);

                    text.AppendLine();
                    text.AppendLine(Logging.GetLogFileText());

                    System.Diagnostics.Trace.WriteLine(text.ToString());

                    string subject = "Vue_LOG_" + DateTime.Now.ToString("yyyy-MM-dd_HH.mm.ss");

                    var emailIntent = new Intent(Intent.ActionSend);
                    emailIntent.PutExtra(Android.Content.Intent.ExtraEmail,   new[] { "cstryi@freenet.de" });
                    emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, subject);
                    emailIntent.PutExtra(Android.Content.Intent.ExtraText,    text.ToString());
                    //emailIntent.SetType("message/rfc822");
                    emailIntent.SetType("text/plain");
                    StartActivity(Intent.CreateChooser(emailIntent, this.Resources.GetString(Resource.String.Settings_SendLogFile)));
                });
            dialog.SetNegativeButton(this.Resources.GetString(Resource.String.App_No), (s, e) => { });
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
                Toast.MakeText(this, Resource.String.Settings_NoDatabaseSelected, ToastLength.Long).Show();
                return;
            }

            var selectFile = new Intent(this, typeof(BackupFileManageActivity));
            selectFile.PutExtra("Text",         this.Resources.GetString(Resource.String.Settings_SelectBackup));
            selectFile.PutExtra("Path",          downloadFolder);
            selectFile.PutExtra("SearchPattern", "*.VueBak");

            StartActivityForResult(selectFile, SelectBackupId);
        }

        public void ButtonBackupToFile_Click(object sender, EventArgs e)
        {
            try
            {
                this.ShareBackup();
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private void ShareBackup()
        {
            // Vor dem Backup ggf. die User-Kategorien ggf. speichern,
            // damit es auch im Backup ist.
            this.SaveUserDefinedCategories();

            DateTime? lastBackupDay = Database.GetSettingsDate("LAST_BACKUP");
            if (lastBackupDay == null)
            {
                lastBackupDay = Database.GetSettingsDate("LAST_BACKUP_TIME");
            }

            // Datum vom Backup in der Datenbank speichern.
            // Datum und Uhrzeit getrennt, damit auch die vorherige Version 
            // (kennt nur Datum) das auslesen kann.
            var now = DateTime.Now;

            Database.SetSettingsDate    ("LAST_BACKUP",      now);  // Für die Abwärtskompatibilität
            Database.SetSettingsDateTime("LAST_BACKUP_TIME", now);

            this.ShowLastBackupDay();

            var databaseFilePath = Android_Database.Instance.GetProductiveDatabasePath();

            string backupFilePath = this.GetBackupFileName(databaseFilePath, true);

            Android_Database.Instance.CloseConnection();

            var progressDialog = this.CreateProgressBar(Resource.Id.ProgressBar_BackupAndRestore);
            new Thread(new ThreadStart(delegate
            {
                string message; 
                try
                {
                    File.Copy(databaseFilePath, backupFilePath);
                    this.shareFileName = backupFilePath;
                }
                catch(Exception ex)
                {
                    TRACE(ex);

                    message = ex.Message;

                    if (lastBackupDay != null)
                    {
                        // Datum vom wieder zurückspielen.
                        Database.SetSettingsDate    ("LAST_BACKUP",      lastBackupDay.Value);
                        Database.SetSettingsDateTime("LAST_BACKUP_TIME", lastBackupDay.Value);
                    }
                }

                this.HideProgressBar(progressDialog);

                Java.IO.File filelocation = new Java.IO.File(backupFilePath);
                var path = Android.Support.V4.Content.FileProvider.GetUriForFile(this, "de.stryi.exportcsv.fileprovider", filelocation);

                Intent intentsend = new Intent();
                intentsend.SetAction(Intent.ActionSend);
                intentsend.SetType("application/octet-stream");
                intentsend.PutExtra(Intent.ExtraStream, path);

                this.StartActivityForResult(Intent.CreateChooser(intentsend, "Backup Datei senden"), ShareFileId);

            })).Start();

            return;
        }

        private async void ButtonRestoreFromFile_Click(object sender, EventArgs e)
        {
            PickOptions options = new PickOptions();
            options.PickerTitle = this.Resources.GetString(Resource.String.Settings_SelectBackupOrImport);

            try
            {
                var file = await FilePicker.PickAsync(options);
                if (file == null)
                    return;

                this.RestoreDatabase(file.FullPath);
            }
            catch (Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }

            return;
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
                Toast.MakeText(this, "No permission has been granted to files and media for this app.", ToastLength.Long).Show();
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
            if (lastBackupDay == null)
            {
                lastBackupDay = Database.GetSettingsDate("LAST_BACKUP_TIME");
            }

            int changesCounter = Database.GetChangeCounter();

            // Datum vom Backup in der Datenbank speichern.
            // Datum und Uhrzeit getrennt, damit auch die vorherige Version das auslesen kann
            // (Kann nur Datum auslesen)
            var now = DateTime.Now;

            Database.SetSettingsDate    ("LAST_BACKUP",      now);  // Für Abwärtskompatibilität
            Database.SetSettingsDateTime("LAST_BACKUP_TIME", now);
            Database.ResetChangeCounter();

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

                    message = string.Format(this.Resources.GetString(Resource.String.Settings_BackupDone), backupFilePath);

                    RunOnUiThread(() =>
                    {
                        this.DetectBackupsCount();
                    });
                }
                catch(Exception ex)
                {
                    TRACE(ex);

                    message = ex.Message;

                    if (lastBackupDay != null)
                    {
                        // Datum vom wieder zurückspielen.
                        Database.SetSettingsDate    ("LAST_BACKUP",      lastBackupDay.Value);
                        Database.SetSettingsDateTime("LAST_BACKUP_TIME", lastBackupDay.Value);
                    }

                    Database.SetChangeCounter(changesCounter);
                }

                this.HideProgressBar(progressDialog);

                RunOnUiThread(() =>
                {
                    var builder = new AlertDialog.Builder(this);
                    builder.SetMessage(message);
                    builder.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
                    builder.Create().Show();
                });
            })).Start();

            return;
        }

        private void EnableButtons()
        {
            Button buttonBackup   = FindViewById<Button>(Resource.Id.SettingsButton_Backup);
            Button buttonBackup2  = FindViewById<Button>(Resource.Id.SettingsButton_BackupToFile);
            Button buttonRestore  = FindViewById<Button>(Resource.Id.SettingsButton_Restore);
            Button buttonRestore2 = FindViewById<Button>(Resource.Id.SettingsButton_RestoreFromFile);

            buttonBackup.Enabled   = !Android_Database.UseTestDatabase;
            buttonBackup2.Enabled  = !Android_Database.UseTestDatabase;
            buttonRestore.Enabled  = !Android_Database.UseTestDatabase;
            buttonRestore2.Enabled = !Android_Database.UseTestDatabase;

            TextView lastBackup = FindViewById<TextView>(Resource.Id.Settings_LastBackupDay);
            lastBackup.Enabled = !Android_Database.UseTestDatabase;

            TextView changesLastBackup = FindViewById<TextView>(Resource.Id.Settings_ChangesSinceLastBackup);
            changesLastBackup.Enabled = !Android_Database.UseTestDatabase;
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

            var dateTime = Database.GetSettingsDate("LAST_BACKUP_TIME");
            if (dateTime == null)
            {
                dateTime = Database.GetSettingsDate("LAST_BACKUP");
            }

            if (dateTime != null)
            {
                lastBackupDay = Tools.ToHumanText(dateTime.Value);
            }

            TextView lastBackupDayView = FindViewById<TextView>(Resource.Id.Settings_LastBackupDay);
            lastBackupDayView.Text = string.Format(this.Resources.GetString(Resource.String.Settings_LastBackupOn), lastBackupDay);

            int changesCount = Database.GetChangeCounter();

            TextView changesLastBackup = FindViewById<TextView>(Resource.Id.Settings_ChangesSinceLastBackup);
            changesLastBackup.Text = string.Format(this.Resources.GetString(Resource.String.Settings_ChangesSinceLastBackup), changesCount);
            
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
                versionInfo += string.Format("Version {0}",         AppInfo.VersionString);
                versionInfo += string.Format(" (Code Version {0})", AppInfo.BuildString);
            }
            catch  { }

            try
            {
                #pragma warning disable CS0618 // Type or member is obsolete

                var cpu1 = Android.OS.Build.CpuAbi;
                var cpu2 = Android.OS.Build.CpuAbi2;

                #pragma warning restore CS0618 // Type or member is obsolete

                var list = new List<string>();
                if (cpu1 != null) { list.Add(cpu1); }
                if (cpu2 != null) { list.Add(cpu2); }

                if (list.Count > 0)
                {
                    // This field was deprecated in API level 21.
                    var prozessoren = String.Join(',', list);
                    versionInfo += string.Format("\nProzessor: {0}", prozessoren);
                }
            }
            catch { }

            try
            {
                var prozessoren = String.Join<string>(',', Android.OS.Build.SupportedAbis);
                versionInfo += string.Format("\nProzessor: {0}", prozessoren);
            }
            catch { }

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
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();
            }
        }

        private bool IstFarbeDunklerAlsGray(Android.Graphics.Color color)
        {
            int durchschnittsRGB = (color.R + color.G + color.B) / 3;
            int durchschnittsRGBGray = (Color.Gray.R + Color.Gray.G + Color.Gray.B) / 3;

            return durchschnittsRGB < durchschnittsRGBGray;
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