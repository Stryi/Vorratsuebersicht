
using System;
using System.Diagnostics;

using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Content.Res;

namespace VorratsUebersicht
{
    [Activity(Label = "Vorratsübersicht", Icon = "@drawable/ic_launcher")]
    //[Activity(Label = "Vorratsübersicht", Icon = "@drawable/ic_launcher", MainLauncher = true)]
    public class MainActivity : Activity
    {
        public static readonly int SelectBackupFileId = 1000;
        public static readonly int EditStorageItemQuantityId = 1001;
        public static readonly int OptionsId = 1002;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            var lan = Resources.Configuration.Locale;

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.Main);

            // Datenbanken erstellen
            new Android_Database().RestoreSampleDatabaseFromResources();

            string databaseName = new Android_Database().GetDatabasePath();
            if (databaseName == null)
            {
                TextView text = FindViewById<TextView>(Resource.Id.Main_Text1);
                text.Text = "Keine Datenbank gefunden";
                text.Visibility = ViewStates.Visible;
                return;
            }

            // Somewhere in your app, call the initialization code:
            ZXing.Mobile.MobileBarcodeScanner.Initialize (Application);

			this.ShowInfoText();

            FindViewById<TextView>(Resource.Id.Main_Text).Click  += ArticlesNearExpiryDate_Click;
            FindViewById<TextView>(Resource.Id.Main_Text1).Click += ArticlesNearExpiryDate_Click;
            FindViewById<TextView>(Resource.Id.Main_Text2).Click += ArticlesNearExpiryDate_Click;

            Button buttonKategorie = FindViewById<Button>(Resource.Id.MainButton_Kategorie);
            buttonKategorie.Enabled = true;
            buttonKategorie.Click += delegate 
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

                    var subCategory = new Intent (this, typeof(SubCategoryActivity));
                    subCategory.PutExtra("Category", kategorie);
                    StartActivity(subCategory);


                });
                builder.Show();
            };

            Button buttonLagerbestand = FindViewById<Button>(Resource.Id.MainButton_Lagerbestand);
            buttonLagerbestand.Enabled = true;
            buttonLagerbestand.Click += delegate { StartActivityForResult (new Intent (this, typeof(StorageItemListActivity)), EditStorageItemQuantityId);};


            Button buttonArticle = FindViewById<Button>(Resource.Id.MainButton_Artikeldaten);
            buttonArticle.Enabled = true;
            buttonArticle.Click += delegate { StartActivity (new Intent (this, typeof(ArticleListActivity)));};

            Button buttonCategories = FindViewById<Button>(Resource.Id.MainButton_Kategorie);
            // TODO: Irgendwann mal implementieren...
            //buttonCategories.Enabled = true;
            //buttonCategories.Click += ...

            Button buttonShoppingList = FindViewById<Button>(Resource.Id.MainButton_ShoppingList);
            // TODO: Irgendwann mal implementieren...
            //buttonShoppingList.Enabled = true;
            //buttonShoppingList.Click += delegate { StartActivity (new Intent (this, typeof(ShoppingListActivity)));};

            Button buttonBarcode = FindViewById<Button>(Resource.Id.MainButton_Barcode);
            buttonBarcode.Enabled = true;
            buttonBarcode.Click += ButtonBarcode_Click;

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
                    StartActivityForResult(new Intent(this, typeof(Settings)), OptionsId);

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

        private void ShowInfoAufTestversion()
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            string lastRunDay = prefs.GetString("LastRunDay", string.Empty);
            string today      = DateTime.Today.ToString("yyyy.MM.dd");

            if (!lastRunDay.Equals(today))
            {
                var message = new AlertDialog.Builder(this);
                message.SetMessage("Das ist eine Testversion. Diese darf nicht produktiv eingesetzt werden.");
                message.SetPositiveButton("OK", (s, e) => { });
                message.Create().Show();
            }

            var prefEditor = prefs.Edit();
            prefEditor.PutString("LastRunDay", today);
            prefEditor.Commit();
        }


        /// <summary>
        /// Information über abgelaufene Lagerpositionen und die Positionen, bei denen das Ablaufdatum
        /// innerhalb vom Warnungsdatum liegt.
        /// </summary>
        private void ShowInfoText()
		{
            decimal abgelaufen = Database.GetArticleCount_Abgelaufen();

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

            decimal kurzDavor = Database.GetArticleCount_BaldZuVerbrauchen();
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
				this.ShowInfoText();
			}

            if (requestCode == OptionsId)
            {
                // Sich neu connecten;
                Android_Database.SQLiteConnection = null;

                //this.ShowDatabaseInfo();
                this.ShowInfoText();
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
            if (result.Count == 1)
            {
                int artickeId = result[0].ArticleId;

                // Artikel gefunden
                var articleDetails = new Intent (this, typeof(StorageItemQuantityActivity));
                articleDetails.PutExtra("ArticleId", artickeId);
                this.StartActivityForResult(articleDetails, 1000);
                return;
            }

            var storageitemList = new Intent(this, typeof(StorageItemListActivity));
            storageitemList.PutExtra("EANCode", eanCode);
            storageitemList.PutExtra("ShowEmptyStorageArticles", true); // Auch Artikel ohne Lagerbestand anzeigen
            StartActivity(storageitemList);
        }
    }
}

