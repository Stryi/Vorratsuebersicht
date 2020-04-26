using System;
using System.IO;
using System.Globalization;
using System.Diagnostics;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.PM;
using Android.Content.Res;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    using System.Collections.ObjectModel;
    using static Tools;

    [Activity(Label = " Vorratsübersicht", Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity
    {
        // Debug-Konstanten
        private static bool debug_date_picker = false;

        public static readonly int EditStorageItemQuantityId = 1001;
        public static readonly int OptionsId = 1002;
        public static readonly int ArticleListId = 1003;

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
            MainActivity.preLaunchTestEndDay = new DateTime(2020, 4, 14);

            base.OnCreate(bundle);

            if (Debugger.IsAttached)
            {
                ShoppingListHelper.UnitTest();
            }

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayShowHomeEnabled(true);

            // Datenbanken erstellen
            Android_Database.Instance.RestoreSampleDatabaseFromResources();

            // Initialisierung für EAN Scanner
            ZXing.Mobile.MobileBarcodeScanner.Initialize (Application);

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
                Button b = new Button(this.ApplicationContext);
                b.Text = "Test DP";
                b.Click += delegate
                {
                    AltDatePickerFragment frag = AltDatePickerFragment.NewInstance(delegate (DateTime? time) { b.Text = time!=null ? time.Value.ToShortDateString() : "Kein Datum"; }, DateTime.Today);
                    frag.ShowsDialog = true;
                    frag.Show(FragmentManager, AltDatePickerFragment.TAG);
                };
                FindViewById<LinearLayout>(Resource.Id.Main_LinearLayout).AddView(b);
                AltDatePickerFragment frag2 = AltDatePickerFragment.NewInstance(delegate (DateTime? time) { b.Text = time != null ? time.Value.ToShortDateString() : "Kein Datum"; }, DateTime.Today);
                frag2.ShowsDialog = true;
                frag2.Show(FragmentManager, AltDatePickerFragment.TAG);
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
            this.ShowInfoAufTestversion();
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

            string error = this.ShowStorageInfoText();
            this.ShowDatabaseError(error);
            this.ShowDatabaseName();

            this.EnableButtons(string.IsNullOrEmpty(error));
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
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

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.Main_Menu_Options:
                    StartActivityForResult(new Intent(this, typeof(SettingsActivity)), OptionsId);

                    return true;
            }

            return false;
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

        private void ShowDatabaseError(string error)
        {
            string text = string.Empty;

            if (!string.IsNullOrEmpty(error))
                text = "Fehler beim Zugriff auf die Datenbank:\n\n" + error;
            
            this.SetInfoText(text);
        }

        private void ShowInfoAufTestversion()
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

            string lastRunDay = Settings.GetString("LastRunDay", string.Empty);
            int startInfoNr   = Settings.GetInt("StartInfoNumber", 0);

            DateTime lastRun = new DateTime();
            DateTime today = DateTime.Today;

            if (!string.IsNullOrEmpty(lastRunDay))
            {
                lastRun = DateTime.ParseExact(lastRunDay, "yyyy.MM.dd", CultureInfo.InvariantCulture);
            }

            // Zum Debuggen
            // lastRun = new DateTime(1900,01,01);
            // startInfoNr = 0;

            if (today != lastRun)
            {
                startInfoNr++;
                if (startInfoNr > 3)
                    startInfoNr = 1;

                switch(startInfoNr)
                {
                    case 1:
                        message = Resources.GetString(Resource.String.Start_TestVersionInfo1);
                        break;

                    case 2:
                        message = Resources.GetString(Resource.String.Start_TestVersionInfo2);
                        break;

                    case 3:
                        message = Resources.GetString(Resource.String.Start_TestVersionInfo3);
                        break;

                    case 4:
                        message = Resources.GetString(Resource.String.Start_TestVersionInfo4);
                        break;
                }
            }

            if (!string.IsNullOrEmpty(message))
            {
                this.SetInfoText(message, false);
            }

            Settings.PutString("LastRunDay",   today.ToString("yyyy.MM.dd"));
            Settings.PutInt("StartInfoNumber", startInfoNr);
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

        /// <summary>
        /// Information über abgelaufene Lagerpositionen und die Positionen, bei denen das Ablaufdatum
        /// innerhalb vom Warnungsdatum liegt.
        /// </summary>
        private string ShowStorageInfoText()
		{
            decimal abgelaufen;

            try
            {
                abgelaufen = Database.GetArticleCount_Abgelaufen();
            }
            catch(Exception ex)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return ex.Message;
            }

            TextView text = FindViewById<TextView>(Resource.Id.Main_Text1);
            if (abgelaufen > 0)
            {
                string value = Resources.GetString(Resource.String.Main_ArticlesWithExpiryDate);
                text.Text = string.Format(value, abgelaufen);
                text.Visibility = ViewStates.Visible;
            }
            else
            {
                text.Visibility = ViewStates.Gone;
            }

            decimal kurzDavor;

            try
            {
                kurzDavor = Database.GetArticleCount_BaldZuVerbrauchen();
            }
            catch(Exception ex)
            {
                //Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
                return ex.Message;
            }

            text = FindViewById<TextView>(Resource.Id.Main_Text2);
            if (kurzDavor > 0)
            {
                string value = Resources.GetString(Resource.String.Main_ArticlesNearExpiryDate);
                text.Text = string.Format(value, kurzDavor);
                text.Visibility = ViewStates.Visible;
            }
            else
            {
                text.Visibility = ViewStates.Gone;
            }

            text = FindViewById<TextView>(Resource.Id.Main_Text);
            if ((abgelaufen > 0) || (kurzDavor > 0))
            {
                text.Visibility = ViewStates.Visible;
            }
            else
            {
                text.Visibility = ViewStates.Gone;
            }

            return null;
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
                ActionBar.Subtitle = "Keine Datenbank ausgewählt";
                return;
            }

            string dbFileName = Path.GetFileNameWithoutExtension(databaseName);
            if (dbFileName != "Vorraete")
            {
                ActionBar.Subtitle = " Datenbank: " + dbFileName;
                return;
            }
            ActionBar.Subtitle = null;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

			if (requestCode == EditStorageItemQuantityId)
			{
				string error = this.ShowStorageInfoText();
                this.ShowDatabaseError(error);
			}

            if (requestCode == OptionsId)
            {
                // Sich neu connecten;
                this.InitializeDatabase();
            }
        }

        private void EnableButtons(bool enable)
        {
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

        private async void ButtonBarcode_Click(object sender, System.EventArgs e)
        {
            string eanCode;
            
            if (Debugger.IsAttached)
            {
                eanCode = "4005500339403";
            }
            else
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                var scanResult = await scanner.Scan();

                if (scanResult == null)
                    return;

                TRACE("Scanned Barcode: {0}", scanResult.Text);
                eanCode = scanResult.Text;
            }

            var result = Database.GetArticlesByEanCode(eanCode);
            if (result.Count == 0)
            {
                // Neuanlage Artikel
                var articleDetails = new Intent (this, typeof(ArticleDetailsActivity));
                articleDetails.PutExtra("EANCode", eanCode);
                StartActivityForResult(articleDetails, 10);
                return;
            }
            if (result.Count == 1)          // Artikel eindeutig gefunden
            {                
                int artickeId = result[0].ArticleId;

                string[] actions = 
                    {
                        Resources.GetString(Resource.String.Main_Button_Lagerbestand),
                        Resources.GetString(Resource.String.Main_Button_Artikelangaben),
                        Resources.GetString(Resource.String.Main_Button_Einkaufsliste)
                    };

                AlertDialog.Builder builder = new AlertDialog.Builder(this);
                builder.SetTitle("Aktion wählen...");
                builder.SetItems(actions, (sender2, args) =>
                {
                    switch(args.Which)
                    {
                        case 0: // Lagerbestand bearbeiten
                            var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                            storageDetails.PutExtra("ArticleId", artickeId);
                            storageDetails.PutExtra("EditMode",  true);

                            this.StartActivityForResult(storageDetails, 1000);
                            break;

                        case 1:
                            // Artikelstamm bearbeiten
                            var articleDetails = new Intent(this, typeof(ArticleDetailsActivity));
                            articleDetails.PutExtra("ArticleId", artickeId);
                            StartActivityForResult(articleDetails, 10);
                            break;
                        case 2:
                            // Auf die Einkaufsliste
                            double count = Database.AddToShoppingList(artickeId, 1);
                            string msg = string.Format("{0} Stück auf der Liste.", count);
                            Toast.MakeText(this, msg, ToastLength.Short).Show();
                            break;
                    }
                    return;
                });
                builder.Show();

                return;
            }

            var storageitemList = new Intent(this, typeof(StorageItemListActivity));
            storageitemList.PutExtra("EANCode", eanCode);
            storageitemList.PutExtra("ShowEmptyStorageArticles", true); // Auch Artikel ohne Lagerbestand anzeigen
            StartActivity(storageitemList);
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

        internal static void SetUserDefinedCategories(string newCategories)
        {
            Database.SetSettings("USER_CATEGORIES", newCategories);
        }
    }
}

