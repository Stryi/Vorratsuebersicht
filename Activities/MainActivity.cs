using System;
using System.Globalization;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.Res;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    [Activity(Label = "Vorratsübersicht", Icon = "@drawable/ic_launcher")]
    public class MainActivity : Activity
    {
        public static readonly int SelectBackupFileId = 1000;
        public static readonly int EditStorageItemQuantityId = 1001;
        public static readonly int OptionsId = 1002;

        public static string Strings_Manufacturer;
        public static string Strings_Size;
        public static string Strings_WarnenInTagen;
        public static string Strings_Calories;
        public static string Strings_Category;
        public static string Strings_SubCategory;
        public static string Strings_Supermarket;
        public static string Strings_Storage;
        public static string Strings_MinQuantity;
        public static string Strings_PrefQuantity;
        public static string Strings_EANCode;
        public static string Strings_Amount;

        private DateTime ActivateEANScanDay;

        protected override void OnCreate(Bundle bundle)
        {
            MainActivity.Strings_Manufacturer  = Resources.GetString(Resource.String.ArticleDetails_Manufacturer);
            MainActivity.Strings_Size          = Resources.GetString(Resource.String.ArticleDetails_Size);
            MainActivity.Strings_WarnenInTagen = Resources.GetString(Resource.String.ArticleDetails_WarningInDays);
            MainActivity.Strings_Calories      = Resources.GetString(Resource.String.ArticleDetails_Calories);
            MainActivity.Strings_Category      = Resources.GetString(Resource.String.ArticleDetails_Category);
            MainActivity.Strings_SubCategory   = Resources.GetString(Resource.String.ArticleDetails_SubCategory);
            MainActivity.Strings_Supermarket   = Resources.GetString(Resource.String.ArticleDetails_SupermarketLabel);
            MainActivity.Strings_Storage       = Resources.GetString(Resource.String.ArticleDetails_StorageLabel);
            MainActivity.Strings_MinQuantity   = Resources.GetString(Resource.String.ArticleDetails_MinQuantityLabel);
            MainActivity.Strings_PrefQuantity  = Resources.GetString(Resource.String.ArticleDetails_PrefQuantityLabel);
            MainActivity.Strings_EANCode       = Resources.GetString(Resource.String.ArticleDetails_EANCode);
            MainActivity.Strings_Amount        = Resources.GetString(Resource.String.ArticleDetails_Amount);

            this.ActivateEANScanDay = new DateTime(2018, 9, 1);

            base.OnCreate(bundle);

            if (Debugger.IsAttached)
            {
                ShoppingListHelper.UnitTest();
            }

            var lan = Resources.Configuration.Locale;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            // Datenbanken erstellen
            new Android_Database().RestoreSampleDatabaseFromResources();

            this.ShowInfoAufTestdatenbank();

            string databaseName = new Android_Database().GetDatabasePath();
            if (databaseName == null)
            {
                this.SetInfoText("Keine Datenbank gefunden");
                return;
            }

            // Somewhere in your app, call the initialization code:
            ZXing.Mobile.MobileBarcodeScanner.Initialize (Application);

            string error = this.ShowStorageInfoText();
            this.ShowDatabaseError(error);

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
            buttonArticle.Click += delegate { StartActivity (new Intent (this, typeof(ArticleListActivity)));};

            // Einkaufsliste
            Button buttonShoppingList = FindViewById<Button>(Resource.Id.MainButton_ShoppingList);
            buttonShoppingList.Click += delegate { StartActivity (new Intent (this, typeof(ShoppingListActivity)));};

            // Barcode scannen
            Button buttonBarcode = FindViewById<Button>(Resource.Id.MainButton_Barcode);
            buttonBarcode.Click += ButtonBarcode_Click;

            this.EnableButtons(string.IsNullOrEmpty(error));

            this.ShowInfoAufTestversion();
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
            storageitemList.PutExtra("ShowToConsumerOnly", true);
            StartActivity(storageitemList);

        }

        private void ShowCategoriesSelection()
        {
            string[] categories = Database.GetCategories();

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
            if (string.IsNullOrEmpty(error))
                return;

            string text = "Fehler beim Zugriff auf die Datenbank:\n\n" + error;

            this.SetInfoText(text);
        }

        private void ShowInfoAufTestversion()
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            string lastRunDay = prefs.GetString("LastRunDay", string.Empty);
            string today      = DateTime.Today.ToString("yyyy.MM.dd");

            if (!lastRunDay.Equals(today))
            {
                this.SetInfoText(Resources.GetString(Resource.String.Start_TestVersionInfo));
            }

            var prefEditor = prefs.Edit();
            prefEditor.PutString("LastRunDay", today);
            prefEditor.Commit();
        }

        private void ShowInfoAufTestdatenbank()
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            bool firstRun = prefs.GetBoolean("FirstRun", true);

            if (firstRun)
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage(Resource.String.Start_TestDbQuestion);
                message.SetTitle(Resource.String.App_Name);
                message.SetIcon(Resource.Drawable.ic_launcher);
                message.SetPositiveButton(Resource.String.App_Yes, (s, e) => 
                    { 
                        Android_Database.UseTestDatabase = true;
                        Android_Database.SQLiteConnection = null;   // Sich neu connecten;
                        this.ShowStorageInfoText();
                    });
                message.SetNegativeButton(Resource.String.App_No, (s, e) => { });
                message.Create().Show();
            }

            var prefEditor = prefs.Edit();
            prefEditor.PutBoolean("FirstRun", false);
            prefEditor.Commit();
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
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
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
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
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

        private void SetInfoText(string text)
        {
            TextView textView = FindViewById<TextView>(Resource.Id.Main_TextInfo);
            if (!string.IsNullOrEmpty(textView.Text))
                textView.Text += "\n\n";

            textView.Text += text;
            textView.Visibility = ViewStates.Visible;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
		{
			base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == SelectBackupFileId) && (resultCode == Result.Ok) && (data != null))
            {
                Android.Net.Uri uri = data.Data;
			}


			if (requestCode == EditStorageItemQuantityId)
			{
				this.ShowStorageInfoText();
			}

            if (requestCode == OptionsId)
            {
                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                string error = this.ShowStorageInfoText();
                this.ShowDatabaseError(error);

                this.EnableButtons(string.IsNullOrEmpty(error));
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
            if (DateTime.Now.Date >= this.ActivateEANScanDay)
            {
                buttonBarcode.Enabled = enable;
            }
        }

        private async void ButtonBarcode_Click(object sender, System.EventArgs e)
        {
            string eanCode;
            
            if (Debugger.IsAttached)
            {
                eanCode = "22120649";
            }
            else
            {
                var scanner = new ZXing.Mobile.MobileBarcodeScanner();
                var scanResult = await scanner.Scan();

                if (scanResult == null)
                    return;

                System.Diagnostics.Trace.WriteLine("Scanned Barcode: " + scanResult.Text);
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
    }
}

