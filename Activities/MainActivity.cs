﻿using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Support.V7.App;
using AndroidX.Core.Content;

namespace VorratsUebersicht
{
    using static Tools;
    using AlertDialog = Android.App.AlertDialog;

    [Activity(Label = " Vorratsübersicht", Icon = "@drawable/ic_launcher", Theme = "@style/Theme.AppCompat")]
    public class MainActivity : AppCompatActivity
    {
        // Debug-Konstanten
        private static readonly bool debug_date_picker = false;

        public static readonly int EditStorageItemQuantityId = 1001;
        public static readonly int OptionsId = 1002;
        public static readonly int ArticleListId = 1003;
        public static readonly int ContinueScanMode = 1004;
        public static readonly int EditStorageQuantity = 1005;
        public static readonly int EANScanID = 1006;
        public static readonly int ManageDatabases = 1007;

        public static string Strings_Manufacturer;
        public static string Strings_Size;
        public static string Strings_WarnenInTagen;
        public static string Strings_Calories;
        public static string Strings_Category;
        public static string Strings_SubCategory;
        public static string Strings_Supermarket;
        public static string Strings_Price;
        public static string Strings_Storage;
        public static string Strings_MinQuantity;
        public static string Strings_PrefQuantity;
        public static string Strings_EANCode;
        public static string Strings_Amount;
        public static string Strings_Notes;

        private static DateTime preLaunchTestEndDay;

        protected override void OnCreate(Bundle bundle)
        {
            MainActivity.Strings_Manufacturer  = Resources.GetString(Resource.String.ArticleDetails_Manufacturer);
            MainActivity.Strings_Size          = Resources.GetString(Resource.String.ArticleDetails_Size);
            MainActivity.Strings_WarnenInTagen = Resources.GetString(Resource.String.ArticleDetails_WarningInDays);
            MainActivity.Strings_Calories      = Resources.GetString(Resource.String.ArticleDetails_Calories);
            MainActivity.Strings_Category      = Resources.GetString(Resource.String.ArticleDetails_Category);
            MainActivity.Strings_SubCategory   = Resources.GetString(Resource.String.ArticleDetails_SubCategory);
            MainActivity.Strings_Supermarket   = Resources.GetString(Resource.String.ArticleDetails_SupermarketLabel);
            MainActivity.Strings_Price         = Resources.GetString(Resource.String.ArticleDetails_Price);
            MainActivity.Strings_Storage       = Resources.GetString(Resource.String.ArticleDetails_StorageLabel);
            MainActivity.Strings_MinQuantity   = Resources.GetString(Resource.String.ArticleDetails_MinQuantityLabel);
            MainActivity.Strings_PrefQuantity  = Resources.GetString(Resource.String.ArticleDetails_PrefQuantityLabel);
            MainActivity.Strings_EANCode       = Resources.GetString(Resource.String.ArticleDetails_EANCode);
            MainActivity.Strings_Amount        = Resources.GetString(Resource.String.ArticleDetails_Amount);
            MainActivity.Strings_Notes         = Resources.GetString(Resource.String.ArticleDetails_Notes);

            // Damit Pre-Launch von Google Play Store nicht immer wieder
            // in die EAN Scan "Falle" tappt und da nicht wieder rauskommt.
            // (meistens nächster Tag)
            MainActivity.preLaunchTestEndDay = new DateTime(2021, 05, 22);

            // Zusammen mit minSdkVersion="19" verhindert das den Fehler: Android.Content.Res.Resources+NotFoundException: 'File res/drawable/abc_vector_test.xml from drawable resource ID
            //AppCompatDelegate.CompatVectorFromResourcesEnabled = true;

            base.OnCreate(bundle);

            Xamarin.Essentials.Platform.Init(this, bundle);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            this.SupportActionBar.SetBackgroundDrawable(backgroundPaint);
            this.SupportActionBar.SetDisplayShowHomeEnabled(true);

            // Datenbanken erstellen
            Android_Database.Instance.RestoreSampleDatabaseFromResources(this);

            List<string> databases;
            Android_Database.LoadDatabaseFileListSafe(this, out databases);

            if ((databases.Count > 0) && (string.IsNullOrEmpty(Android_Database.SelectedDatabaseName)))
            {
                string lastSelectedDatabase = Settings.GetString("LastSelectedDatabase", null);

                if (string.IsNullOrEmpty(lastSelectedDatabase))
                {
                    // Datenbanken auswählen
                    this.SwitchDatabase();
                }
                else
                {
                    Android_Database.SelectedDatabaseName = lastSelectedDatabase;
                }
            }

            // Zugriff auf die SD Karte anfordern
            new SdCardAccess().Grand(this);

            // Klick auf den "abgelaufen" Text bringt die Liste der (bald) abgelaufender Artieln.
            FindViewById<TextView>(Resource.Id.Main_Text).Click  += ArticlesNearExpiryDate_Click;
            FindViewById<TextView>(Resource.Id.Main_Text1).Click += ArticlesNearExpiryDate_Click;
            FindViewById<TextView>(Resource.Id.Main_Text2).Click += ArticlesNearExpiryDate_Click;

            // Auswahl nach Kategorien
            Button buttonKategorie = FindViewById<Button>(Resource.Id.MainButton_Kategorie);
            buttonKategorie.Click += delegate { this.ShowCategoriesSelection();};

            // Lagerbestand
            Button buttonLagerbestand = FindViewById<Button>(Resource.Id.MainButton_Lagerbestand);
            buttonLagerbestand.Click += delegate { StartActivityForResult (new Intent (this, typeof(StorageItemListActivity)), EditStorageItemQuantityId);};

            // Artikeldaten
            Button buttonArticle = FindViewById<Button>(Resource.Id.MainButton_Artikeldaten);
            buttonArticle.Click += delegate { StartActivityForResult (new Intent (this, typeof(ArticleListActivity)), ArticleListId);};

            // Einkaufsliste
            Button buttonShoppingList = FindViewById<Button>(Resource.Id.MainButton_ShoppingList);
            buttonShoppingList.Click += delegate { StartActivity (new Intent (this, typeof(ShoppingListActivity)));};

            // Barcode scannen
            Button buttonBarcode = FindViewById<Button>(Resource.Id.MainButton_Barcode);
            buttonBarcode.Click += ButtonBarcode_Click;

            // Einstellungen für Warnhinweis beim OpenFoodFacts.org
            ArticleDetailsActivity.showCostMessage = Settings.GetBoolean("ShowOpenFoodFactsInternetCostsMessage", true);

            StorageItemQuantityActivity.UseAltDatePicker = Settings.GetBoolean("UseAltDatePicker", false);

            // DatePicker-DEBUG
            if (debug_date_picker)
            {
                Android_Database.UseTestDatabase = true;
                Button b = new Button(this.ApplicationContext)
                {
                    Text = "Test DP"
                };
                b.Click += delegate
                {
                    AltDatePickerFragment frag = AltDatePickerFragment.NewInstance(delegate (DateTime? time) { b.Text = time!=null ? time.Value.ToShortDateString() : "Kein Datum"; }, DateTime.Today);
                    frag.ShowsDialog = true;
                    frag.Show(this.SupportFragmentManager, AltDatePickerFragment.TAG);
                };
                FindViewById<LinearLayout>(Resource.Id.Main_LinearLayout).AddView(b);
                AltDatePickerFragment frag2 = AltDatePickerFragment.NewInstance(delegate (DateTime? time) { b.Text = time != null ? time.Value.ToShortDateString() : "Kein Datum"; }, DateTime.Today);
                frag2.ShowsDialog = true;
                frag2.Show(this.SupportFragmentManager, AltDatePickerFragment.TAG);
            }


            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                Android_Database.UseTestDatabase = true;
            }
            else
            {
                this.ShowInfoAufTestdatenbank();
            }

            // Datenbankverbindung initialisieren
            this.InitializeDatabase();

            // Hinweis bei Pre-Launch Untersuchung
            this.ShowInfoAufTestModus();

            // Backup erstellen?
            this.CreateBackup();
        }

        private void InitializeDatabase()
        {
            string databaseName = Android_Database.Instance.GetDatabasePath();
            if (databaseName == null)
            {
                this.SetInfoText("Keine Datenbank gefunden!");
                this.SetInfoText("Bitte erlauben Sie den Zugriff auf den Speicher beim Starten der App, damit die Datenbank dort sicher angelegt werden kann.", false);
                this.EnableButtons(false);
                return;
            }

            // Sich neu connecten;
            Android_Database.SQLiteConnection = null;

            this.ShowDatabaseInfoText();

            this.ShowDatabaseName();
        }

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

            TRACE("Permission granted to external storage.");

            var exception = Android_Database.Instance.CreateDatabaseOnExternalStorage();
            if (exception != null)
            {
                Toast.MakeText(this, exception.Message, ToastLength.Long).Show();
                return;
            }

            this.InitializeDatabase();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.Main_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            IMenuItem itemSelectDatabase = menu.FindItem(Resource.Id.Main_Menu_SelectDatabase);
            itemSelectDatabase.SetVisible(!Android_Database.UseTestDatabase);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Main_Menu_Options:
                    StartActivityForResult(new Intent(this, typeof(SettingsActivity)), OptionsId);

                    return true;

                case Resource.Id.Main_Menu_SelectDatabase:

                    this.SwitchDatabase();

                    return true;
            }

            return false;
        }


        private async void SwitchDatabase()
        {
            string database = await MainActivity.SelectDatabase(this, "Datenbank öffnen:");
            if (string.IsNullOrEmpty(database))
            {
                return;
            }

            Android_Database.TryOpenDatabase(database);
                
            Settings.PutString("LastSelectedDatabase", database);

            this.CheckAndMoveArticleImages();

            this.ShowDatabaseName();

            this.ShowDatabaseInfoText();
        }

        private void CheckAndMoveArticleImages()
        {
            // Nur, wenn bereits eine Datenbank vorhanden ist
            if (Android_Database.SQLiteConnection == null)
                return;

            var picturesToMove = Database.GetArticlesToCopyImages();

            if (picturesToMove.Count == 0)
                return;

            string message = string.Format(
                "Es müsen {0} Bilder übetragen werden.\n\n" +
                "Beenden Sie die App ganz, starten Sie diese neu und wählen Sie beim Starten diese Datenbank.",
                picturesToMove.Count);

            var dialog = new AlertDialog.Builder(this);
            dialog.SetMessage(message);
            dialog.SetTitle(Resource.String.App_Name);
            dialog.SetIcon(Resource.Drawable.ic_launcher);
            dialog.SetPositiveButton("OK", (s1, e1) => { });
            dialog.Create().Show();
        }

        private void ArticlesNearExpiryDate_Click(object sender, EventArgs e)
        {
            var storageitemList = new Intent(this, typeof(StorageItemListActivity));
            storageitemList.PutExtra("OderByToConsumeDate", true);
            StartActivity(storageitemList);
        }

        private void ShowCategoriesSelection()
        {
            string[] categories = Database.GetCategoriesInUse();

            if (categories.Length == 0)
            {
                Toast.MakeText(this, Resource.String.NoArticleCatagories, ToastLength.Long).Show();
                return;
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.ArticleCatagoriesSelect);
            builder.SetItems(categories, (sender, args) =>
            {
                string kategorie = categories[args.Which];

                var subCategory = new Intent(this, typeof(SubCategoryActivity));
                subCategory.PutExtra("Category", kategorie);
                StartActivity(subCategory);
            });
            builder.Show();
        }

        private void CreateBackup()
        {
            /*
            // Einstellungen löschen
            Database.ClearSettings("LAST_BACKUP");
            Settings.Clear("BACKUP_NOT_TODAY");
            */

            bool askForBackup = Settings.GetBoolean("AskForBackup", true);;
            if (!askForBackup)
            {
                return;
            }

            // Noch keine Datenbankverbindung?
            if (Android_Database.SQLiteConnection == null)
                return;

            decimal articleCount = Database.GetArticleCount();
            if (articleCount < 5)
                return;

            DateTime? lastBackupDay = Database.GetSettingsDate("LAST_BACKUP");

            // Backup nur alle 7 Tage vorschlagen
            if ((lastBackupDay != null) && (lastBackupDay.Value.AddDays(7) >= DateTime.Today))
                return;

            // Heute nicht mehr fragen?
            DateTime? notToday = Settings.GetDate("BACKUP_NOT_TODAY");
            if ((notToday != null) && (notToday.Value.Date == DateTime.Today))
                return;

            string messageText = "Backup der Datenbank erstellen?";

            if (lastBackupDay != null)
            {
                messageText += "\r\n\r\n";
                messageText += "Letzter Backup: " + lastBackupDay.Value.ToShortDateString();
            }

            AlertDialog.Builder message = new AlertDialog.Builder(this);
            message.SetIcon(Resource.Drawable.ic_launcher);
            message.SetMessage(messageText);
            message.SetPositiveButton("Ja", (s, e) => 
                { 
                    var settingsActivity = new Intent(this, typeof(SettingsActivity));
                    settingsActivity.PutExtra("CreateBackup", true);
                    StartActivity(settingsActivity);
                });
            message.SetNegativeButton("Später",      (s, e) => { });
            message.SetNeutralButton("Nicht heute",  (s, e) => 
                { 
                    Settings.PutDate("BACKUP_NOT_TODAY", DateTime.Today);
                });

            message.Show();
        }

        private void ShowDatabaseError(string error)
        {
            string text = string.Empty;

            if (!string.IsNullOrEmpty(error))
                text = "Fehler beim Zugriff auf die Datenbank:\n\n" + error;
            
            this.SetInfoText(text);
        }

        private void ShowInfoAufTestModus()
        {
            string message = string.Empty;

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                message = "Die App befindet sich im Testmodus. Folgende Einschränkung bestehen bis zum {0}:\n\n";
                message += "- Nur Testdatenbank\n";
                message += "- Kein EAN Scan\n";
                message = string.Format(message, MainActivity.preLaunchTestEndDay.AddDays(-1).ToShortDateString());

                var messageDialog = new AlertDialog.Builder(this);
                messageDialog.SetMessage(message);
                messageDialog.SetTitle(Resource.String.App_Name);
                messageDialog.SetIcon(Resource.Drawable.ic_launcher);
                messageDialog.SetPositiveButton(Resource.String.App_Ok, (s, e) => {});
                messageDialog.Create().Show();

                return;
            }
        }

        private void ShowInfoAufTestdatenbank()
        {
            bool firstRun = Settings.GetBoolean("FirstRun", true);

            if (firstRun)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(Resource.String.Start_TestDbQuestion);
                message.SetTitle(Resource.String.App_Name);
                message.SetIcon(Resource.Drawable.ic_launcher);
                message.SetPositiveButton("Test starten", (s, e) => 
                    { 
                        Android_Database.UseTestDatabase = true;

                        this.InitializeDatabase();
                    });
                message.SetNegativeButton("OK", (s, e) => { });
                message.Create().Show();
            }

            Settings.PutBoolean("FirstRun", false);
        }

        private void ShowDatabaseInfoText()
        {
            try
            {
                this.ShowStorageInfoText();
                this.ShowDatabaseError(string.Empty);
                this.EnableButtons(true);
            }
            catch(Exception ex)
            {
                this.ShowDatabaseError(ex.Message);
                this.EnableButtons(false);
            }
        }

        /// <summary>
        /// Information über abgelaufene Lagerpositionen und die Positionen, 
        /// bei denen das Ablaufdatum innerhalb vom Warnungsdatum liegt.
        /// </summary>
        private void ShowStorageInfoText()
        {
            TextView text1 = FindViewById<TextView>(Resource.Id.Main_Text1);
            text1.Visibility = ViewStates.Gone;

            TextView text2 = FindViewById<TextView>(Resource.Id.Main_Text2);
            text2.Visibility = ViewStates.Gone;

            TextView text = FindViewById<TextView>(Resource.Id.Main_Text);
            text.Visibility = ViewStates.Gone;

            decimal abgelaufen = Database.GetArticleCount_Abgelaufen();

            if (abgelaufen > 0)
            {
                string value = Resources.GetString(Resource.String.Main_ArticlesWithExpiryDate);
                text1.Text = string.Format(value, abgelaufen);
                text1.Visibility = ViewStates.Visible;
            }

            decimal kurzDavor = Database.GetArticleCount_BaldZuVerbrauchen();

            if (kurzDavor > 0)
            {
                string value = Resources.GetString(Resource.String.Main_ArticlesNearExpiryDate);
                text2.Text = string.Format(value, kurzDavor);
                text2.Visibility = ViewStates.Visible;
            }

            if ((abgelaufen > 0) || (kurzDavor > 0))
            {
                text.Visibility = ViewStates.Visible;
            }
        }

        private void SetInfoText(string text, bool overrideText = true)
        {
            TextView textView = FindViewById<TextView>(Resource.Id.Main_TextInfo);

            if (string.IsNullOrEmpty(text))
            {
                textView.Text = string.Empty;
                textView.Visibility = ViewStates.Gone;
            }
            else
            {
                TRACE(text);

                if (overrideText)
                {
                    textView.Text = text;
                }
                else
                {
                    // Ggf. eine neue Zeile mit Abstand anfangen, wenn schon Text da ist.
                    if (!string.IsNullOrEmpty(textView.Text))
                    {
                        if (!textView.Text.EndsWith("\n"))
                            textView.Text += "\n\n";

                        if (!textView.Text.EndsWith("\n\n"))
                            textView.Text += "\n\n";
                    }

                    textView.Text += text;
                }
                textView.Visibility = ViewStates.Visible;
            }
        }

        private void ShowDatabaseName()
        {
            string databaseName = Android_Database.Instance.GetDatabasePath();
            if (databaseName == null)
            {
                this.SupportActionBar.Subtitle = "Keine Datenbank ausgewählt";
                return;
            }

            string dbFileName = Path.GetFileNameWithoutExtension(databaseName);
            if (dbFileName != "Vorraete")
            {
                this.SupportActionBar.Subtitle = " Datenbank: " + dbFileName;
                return;
            }
            this.SupportActionBar.Subtitle = null;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (requestCode == EditStorageItemQuantityId)
            {
                this.ShowDatabaseInfoText();
            }

            if (requestCode == OptionsId)
            {
                // Sich neu connecten;
                this.InitializeDatabase();
            }

            if (requestCode == ContinueScanMode)
            {
                Button buttonBarcode = FindViewById<Button>(Resource.Id.MainButton_Barcode);
                buttonBarcode.PerformClick();
            }

            if ((requestCode == EditStorageQuantity) && (resultCode == Result.Ok) && (data != null))
            {
                int id = data.GetIntExtra("ArticleId", -1);
                if (id == -1)
                    return;

                Database.RemoveFromShoppingList(id);
            }

            if ((requestCode == EANScanID) && (resultCode == Result.Ok) && (data != null))
            {
                string eanCode = data.GetStringExtra("EANCode");
                this.SearchEANCode(eanCode);
            }
        }

        private void EnableButtons(bool enable)
        {
            this.InvalidateOptionsMenu();

            Button buttonKategorie = FindViewById<Button>(Resource.Id.MainButton_Kategorie);
            buttonKategorie.Enabled = enable;

            // Lagerbestand
            Button buttonLagerbestand = FindViewById<Button>(Resource.Id.MainButton_Lagerbestand);
            buttonLagerbestand.Enabled = enable;

            // Artikeldaten
            Button buttonArticle = FindViewById<Button>(Resource.Id.MainButton_Artikeldaten);
            buttonArticle.Enabled = enable;

            // Einkaufsliste
            Button buttonShoppingList = FindViewById<Button>(Resource.Id.MainButton_ShoppingList);
            buttonShoppingList.Enabled = enable;

            // Barcode scannen
            Button buttonBarcode = FindViewById<Button>(Resource.Id.MainButton_Barcode);
            if (!MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                buttonBarcode.Enabled = enable;
            }
        }

        private void ButtonBarcode_Click(object sender, System.EventArgs e)
        {
            // this.SearchEANCode("4058172637117");
            StartActivityForResult(typeof(ZXingFragmentActivity), EANScanID);
        }

        private void SearchEANCode(string eanCode)
        { 
            TRACE("Scanned Barcode: {0}", eanCode);

            var result = Database.GetArticlesByEanCode(eanCode);
            if (result.Count == 0)
            {
                // Neuanlage Artikel
                var articleDetails = new Intent (this, typeof(ArticleDetailsActivity));
                articleDetails.PutExtra("EANCode", eanCode);
                StartActivityForResult(articleDetails, ContinueScanMode);
                return;
            }

            List<string> actions;
            AlertDialog.Builder selectDialog = new AlertDialog.Builder(this);

            if (result.Count == 1)          // Artikel eindeutig gefunden
            {                
                int articleId = result[0].ArticleId;
                
                string zusatzInfo = string.Empty;

                decimal quantityInStorage = Database.GetArticleQuantityInStorage(articleId);
                if (quantityInStorage > 0)
                {
                    zusatzInfo += string.Format("- Bestand: {0:#,0.######}", quantityInStorage);
                }

                decimal shoppingListQuantiy = Database.GetShoppingListQuantiy(articleId);
                if (shoppingListQuantiy > 0)
                {
                    if (!string.IsNullOrEmpty(zusatzInfo))
                    {
                        zusatzInfo += "\n";
                    }
                    zusatzInfo += string.Format("- Auf Einkaufsliste: {0:#,0.######}", shoppingListQuantiy);
                }

                actions =  new List<string>()
                    {
                        Resources.GetString(Resource.String.Main_Button_Lagerbestand),
                        Resources.GetString(Resource.String.Main_Button_Artikelangaben),
                        Resources.GetString(Resource.String.Main_Button_AufEinkaufsliste)
                    };

                var shoppingItemCount = Database.GetShoppingListQuantiy(articleId, -1);
                if (shoppingItemCount >= 0)
                {
                    // Von der Einkaufsliste direkt ins Lager.
                    actions.Add(Resources.GetString(Resource.String.Main_Button_InLagerbestand));
                }

                if (!string.IsNullOrEmpty(zusatzInfo))
                {
                    TextView info = new TextView(this);
                    //info.SetBackgroundColor(Android.Graphics.Color.LightGray);
                    info.SetTextColor(Android.Graphics.Color.Gray);
                    info.SetPadding(25, 0, 0, 0);
                    info.Text = zusatzInfo;

                    selectDialog.SetView(info);
                }

                selectDialog.SetTitle("Aktion wählen...");
                selectDialog.SetItems(actions.ToArray(), (sender2, args) =>
                {
                    switch(args.Which)
                    {
                        case 0: // Lagerbestand bearbeiten
                            var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                            storageDetails.PutExtra("ArticleId", articleId);
                            storageDetails.PutExtra("EditMode",  true);

                            this.StartActivityForResult(storageDetails, ContinueScanMode);
                            break;

                        case 1:
                            // Artikelstamm bearbeiten
                            var articleDetails = new Intent(this, typeof(ArticleDetailsActivity));
                            articleDetails.PutExtra("ArticleId", articleId);
                            StartActivityForResult(articleDetails, ContinueScanMode);
                            break;
                        case 2:
                            // Auf die Einkaufsliste
                            AddToShoppingListDialog.ShowDialog(this, articleId);
                            break;
                        case 3:
                            if (shoppingItemCount == 0)
                                shoppingItemCount = 1;

                            // Aus Einkaufsliste ins Lager
                            var storageDetails2 = new Intent(this, typeof(StorageItemQuantityActivity));
                            storageDetails2.PutExtra("ArticleId", articleId);
                            storageDetails2.PutExtra("EditMode",  true);
                            storageDetails2.PutExtra("Quantity",  (double)shoppingItemCount);

                            this.StartActivityForResult(storageDetails2, EditStorageQuantity);
                            break;
                    }

                    return;
                });
                selectDialog.Show();

                return;
            }

            actions = new List<string>()
                {
                    Resources.GetString(Resource.String.Main_Button_LagerbestandListe),
                    Resources.GetString(Resource.String.Main_Button_ArtikelListe)
                };

            selectDialog.SetTitle("Aktion wählen...");
            selectDialog.SetItems(actions.ToArray(), (sender2, args) =>
            {
                switch(args.Which)
                {
                    case 0: // Lagerbestand Liste
                        var storageDetails = new Intent(this, typeof(StorageItemListActivity));
                        storageDetails.PutExtra("EANCode", eanCode);
                        this.StartActivityForResult(storageDetails, ContinueScanMode);
                        break;
                    case 1:
                        // Artikel Liste
                        var articleDetails = new Intent(this, typeof(ArticleListActivity));
                        articleDetails.PutExtra("EANCode", eanCode);
                        this.StartActivityForResult(articleDetails, ContinueScanMode);
                        break;
                }
                return;
            });
            selectDialog.Show();

            return;
        }

        public static bool IsGooglePlayPreLaunchTestMode
        {
            get
            {
                return DateTime.Now.Date < MainActivity.preLaunchTestEndDay;
            }
        }

        internal static ICollection<string> GetUserDefinedCategories()
        {
            string userCategories = Database.GetSettingsString("USER_CATEGORIES");
            if (userCategories == null)
                return new Collection<string>();

            userCategories = userCategories.Trim().Trim(',');
            if (string.IsNullOrEmpty(userCategories))
                return new Collection<string>();

            var result = new List<string>();

            foreach(string category in userCategories.Split(','))
            {
                if (string.IsNullOrEmpty(category))
                    continue;

                result.Add(category.Trim());
            }

            return result;
        }

        internal static List<string> GetDefinedCategories(string[] defaultCategories)
        {
            // Fest definierte Kategorien
            List<string> categories = new List<string>(defaultCategories);

            // Noch keine Datenbankverbindung?
            if (Android_Database.SQLiteConnection == null)
                return categories;

            var userCategories = MainActivity.GetUserDefinedCategories();
            foreach(string userCategory in userCategories)
            {
                if (categories.Contains(userCategory))      // Doppelte verhindern
                    continue;

                categories.Add(userCategory);
            }

            categories.Sort();

            return categories;
        }

        internal static void SetUserDefinedCategories(string newCategories)
        {
            Database.SetSettings("USER_CATEGORIES", newCategories);
        }

        internal static void SetDefaultCategory(string newDefaultCategory)
        {
            Database.SetSettings("DEFAULT_CATEGORY", newDefaultCategory);
        }

        internal static Task<string> SelectDatabase(Context context, string title, string except = null)
        {
            var tcs = new TaskCompletionSource<string>();

            Exception ex = Android_Database.LoadDatabaseFileListSafe(context, out List<string> fileList);
            if (ex != null)
            {
                Toast.MakeText(context, ex.Message, ToastLength.Long).Show();
            }

            if (!string.IsNullOrEmpty(except))
            {
                if (fileList.Contains(except))
                    fileList.Remove(except);
            }

            if (fileList.Count == 0)
            {
                tcs.TrySetResult(null);
                return tcs.Task;
            }

            string[] databaseNames = new string[fileList.Count];

            for(int i = 0; i < fileList.Count; i++)
            {
                databaseNames[i] = Path.GetFileNameWithoutExtension(fileList[i]);
            }

            using(AlertDialog.Builder builder = new AlertDialog.Builder(context))
            {
                builder.SetTitle(title);
                builder.SetItems(databaseNames, (sender2, args)          => { tcs.TrySetResult(fileList[args.Which]); });
                builder.SetOnCancelListener(new ActionDismissListener(() => { tcs.TrySetResult(null); }));
                builder.Show();
            }

            return tcs.Task;
        }

        internal static Task<string> InputTextAsync(Context context, string title, string message, string name, string positiveButton, string negativeButton)
        {
            var tcs = new TaskCompletionSource<string>();

            var builder = new AlertDialog.Builder(context);

            var dialog = builder.Create();
            dialog.SetTitle(title);
            dialog.SetMessage(message);
            dialog.Window.SetSoftInputMode(Android.Views.SoftInput.StateVisible);
            EditText input = new EditText(context);
            input.InputType = Android.Text.InputTypes.ClassText;
            input.Text = name;
            input.RequestFocus();
            dialog.SetView(input);
            dialog.SetButton(positiveButton,  (s, e) => { tcs.TrySetResult(input.Text); });
            dialog.SetButton2(negativeButton, (s, e) => { tcs.TrySetResult(null);       });
            dialog.Show();

            return tcs.Task;
        }


    }
}

