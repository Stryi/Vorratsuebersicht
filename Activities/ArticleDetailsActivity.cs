using System;
using System.Threading;
using System.Globalization;
using System.Collections.Generic;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Content.PM;
using Android.Content.Res;

using MatrixGuide;
using static Android.Widget.AdapterView;
using Android.Support.V4.Content;
using Android.Speech;

using Xamarin.Essentials;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "@string/Main_Button_Artikelangaben", Icon = "@drawable/ic_local_offer_white_48dp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ArticleDetailsActivity : Activity
    {
        CultureInfo currentCulture;

        internal static byte[] imageLarge;
        internal static byte[] imageSmall;

        Article article;
        ArticleImage articleImage;
        int articleId;
        bool isChanged = false;
        bool noStorageQuantity = false;
        bool noDeleteArticle = false;

        ImageView imageView;
        ImageView imageView2;
        TextView  imageTextView;
        EditText  warningInDaysView;
        TextView  warningInDaysLabelView;
        EditText size;
        EditText unit;
        EditText calorie;
        TextView caloriePerUnitLabel;
        EditText caloriePerUnit;
        bool ignoreTextChangeEvent;

        List<string> Manufacturers;
        CatalogItemSelectedListener catalogListener;
        List<string> Storages;
        List<string> Supermarkets;

        public static readonly int SpechId = 1002;
        public static readonly int EditPhoto = 1003;
        public static readonly int InternetDB = 1004;
        public static readonly int StorageQuantityId = 1005;
        public static readonly int EANScanID = 1006;

        public bool IsPhotoSelected
        {
            get 
            {
                if (this.articleImage.ImageSmall != null)
                    return true;

                if (ArticleDetailsActivity.imageSmall != null)
                    return true;

                return false;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            base.OnCreate(savedInstanceState);

            this.currentCulture = new CultureInfo(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);

            this.SetContentView(Resource.Layout.ArticleDetails);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            string text            = Intent.GetStringExtra ("Name") ?? string.Empty;
            this.articleId         = Intent.GetIntExtra    ("ArticleId", 0);
            string eanCode         = Intent.GetStringExtra ("EANCode") ?? string.Empty;

            // TODO: Kategorie und SubCategorie schon mal setzen (ist ja Neuanlage mit Filter)
            var defaultCategory    = Intent.GetStringExtra ("Category");
            var defaultSubCategory = Intent.GetStringExtra ("SubCategory");
            this.noStorageQuantity = Intent.GetBooleanExtra("NoStorageQuantity", false);
            this.noDeleteArticle   = Intent.GetBooleanExtra("NoDeleteArticle",   false);

            this.article = Database.GetArticle(this.articleId);
            if (this.article == null)
            {
                this.article = new Article();
            }

            this.articleImage = Database.GetArticleImage(this.articleId, false);
            if (this.articleImage == null)
                this.articleImage = new ArticleImage();

            this.imageView              = FindViewById<ImageView>(Resource.Id.ArticleDetails_Image);
            this.imageView2             = FindViewById<ImageView>(Resource.Id.ArticleDetails_Image2);
            this.imageTextView          = FindViewById<TextView>(Resource.Id.ArticleDetails_ImageText);
            this.warningInDaysView      = FindViewById<EditText>(Resource.Id.ArticleDetails_WarnInDays);
            this.warningInDaysLabelView = FindViewById<TextView>(Resource.Id.ArticleDetails_WarnInDaysLabel);

            Switch durableInfinity = FindViewById<Switch>  (Resource.Id.ArticleDetails_DurableInfinity);
            durableInfinity.Click += delegate
            {
                this.warningInDaysView.Enabled      = !durableInfinity.Checked;
                this.warningInDaysLabelView.Enabled = !durableInfinity.Checked;
            };

            this.size                  = FindViewById<EditText>(Resource.Id.ArticleDetails_Size);
            this.unit                  = FindViewById<EditText>(Resource.Id.ArticleDetails_Unit);
            this.calorie               = FindViewById<EditText>(Resource.Id.ArticleDetails_Calorie);
            this.caloriePerUnitLabel   = FindViewById<TextView>(Resource.Id.ArticleDetails_CaloriePerUnitLabel);
            this.caloriePerUnit        = FindViewById<EditText>(Resource.Id.ArticleDetails_CaloriePerUnit);

            this.size.TextChanged            += this.BerechneCalPerUnit;
            this.unit.TextChanged            += this.BerechneCalPerUnit;
            this.calorie.TextChanged         += this.BerechneCalPerUnit;
            this.caloriePerUnit.TextChanged  += this.BerechneCalGes;

            // Hersteller Eingabe
            this.Manufacturers = new List<string>();

            var manufacturer = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Manufacturer);
            FindViewById<Button>(Resource.Id.ArticleDetails_SelectManufacturer).Click += SelectManufacturer_Click;

            ArrayAdapter<String> manufacturerAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Manufacturers);
            manufacturer.Adapter = manufacturerAdapter;
            manufacturer.Threshold = 1;


            // Kategorie Auswahl
            this.catalogListener = new CatalogItemSelectedListener();

            EditText articleName = FindViewById<EditText>(Resource.Id.ArticleDetails_Name);
            var colors = articleName.TextColors;
            if (colors != null)
            {
                this.catalogListener.TextColor = new Color(colors.DefaultColor);
            }

            // Fest definierte Kategorien
            string[] defaultCategories = Resources.GetStringArray(Resource.Array.ArticleCatagories);

            // Frei definierte Kategorien zusätzlich laden.
            List<string> categories = MainActivity.GetDefinedCategories(defaultCategories);


            if (this.article.Category != null)
            {
                if (!categories.Contains(this.article.Category))
                {
                    categories.Add(this.article.Category);
                }
            }

            categories.Sort();

            ArrayAdapter<String> categoryAdapter = new ArrayAdapter<String>(this, Resource.Layout.SpinnerItem, categories);
            categoryAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

            Spinner categorySpinner = (Spinner)FindViewById<Spinner>(Resource.Id.ArticleDetails_Category);
            categorySpinner.Adapter = categoryAdapter;
            categorySpinner.OnItemSelectedListener = this.catalogListener;

            // Unterkategorie Eingabe
            var subCategories = new List<string>();

            var subCategory = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);
            FindViewById<Button>(Resource.Id.ArticleDetails_SelectSubCategory).Click  += SelectSubCategory_Click;

            ArrayAdapter<String> subCategoryAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, subCategories);
            subCategory.Adapter = subCategoryAdapter;
            subCategory.Threshold = 1;

            // Lagerort Eingabe
            this.Storages = new List<string>();

            var storage = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);
            FindViewById<Button>(Resource.Id.ArticleDetails_SelectStorage).Click      += SelectStorage_Click;

            ArrayAdapter<String> storageAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Storages);
            storage.Adapter = storageAdapter;
            storage.Threshold = 1;

            // Einkaufsmarkt Eingabe
            this.Supermarkets = new List<string>();

            var supermarket = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);
            FindViewById<Button>(Resource.Id.ArticleDetails_SelectSupermarket).Click  += SelectSupermarket_Click;

            ArrayAdapter<String> supermarketAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Supermarkets);
            supermarket.Adapter = supermarketAdapter;
            supermarket.Threshold = 1;


            this.ShowPictureAndDetails(eanCode);

            imageView.Click     += TakeOrShowPhoto;
            imageTextView.Click += delegate { this.SaveAndGoToStorageItem(); };

            imageView2.Click += delegate
            {
                this.SelectAPicture();
            };


            if (!string.IsNullOrEmpty(this.article.Name))
            { 
                // Artikelname ist eingetragen. Tastatus anfänglich ausblenden.
                this.Window.SetSoftInputMode(SoftInput.StateHidden);
            }
            else
            {
                FindViewById<EditText>(Resource.Id.ArticleDetails_Name).RequestFocus();
                this.Window.SetSoftInputMode(SoftInput.StateVisible);
            }

            stopWatch.Stop();
            TRACE("Dauer Laden der Artikeldaten: {0}", stopWatch.Elapsed.ToString());

            if (this.articleId == 0)
            {
                this.SearchEanCodeOnInternetDb();
            }

            // Für die automatische Berechnung des Preises pro Einheit.
            FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).AfterTextChanged  += CalculatePricePerUnit;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Size).AfterTextChanged  += CalculatePricePerUnit;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Price).AfterTextChanged += CalculatePricePerUnit;

            this.CalculatePricePerUnit(this, EventArgs.Empty);
        }

        private void CalculatePricePerUnit(object sender, EventArgs e)
        {
            string unit  = FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).Text;
            string size  = FindViewById<EditText>(Resource.Id.ArticleDetails_Size).Text;
            string price = FindViewById<EditText>(Resource.Id.ArticleDetails_Price).Text;

            string pricePerUnitText = PricePerUnit.Calculate(price, size, unit);

            FindViewById<TextView>(Resource.Id.ArticleDetails_PricePerUnit).Text = 
                string.Format("{0} {1}",
                    Resources.GetString(Resource.String.ArticleDetails_PricePerUnit),
                    pricePerUnitText);
        }

        private void SelectManufacturer_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.ArticleDetails_Manufacturer);
            builder.SetItems(this.Manufacturers.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Manufacturer);
                textView.Text = this.Manufacturers[a.Which];
            });
            builder.Show();
        }

        private void SelectSubCategory_Click(object sender, EventArgs e)
        {
            var subCategories    = Database.GetSubcategoriesOf(this.catalogListener.Value);
            var allSubCategories = Database.GetSubcategoriesOf();

            // Empty entry as delimitation and for deleting the subcategory.
            subCategories.Add(String.Empty);

            // All other categories.
            foreach (var subCategory in allSubCategories)
            {
                if (!subCategories.Contains(subCategory))
                {
                    subCategories.Add(subCategory);
                }
            }

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.ArticleDetails_SubCategory);
            builder.SetItems(subCategories.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);
                textView.Text = subCategories[a.Which];
            });
            builder.Show();
        }

        private void SelectSupermarket_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.ArticleDetails_SupermarketLabel);
            builder.SetItems(this.Supermarkets.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);
                textView.Text = this.Supermarkets[a.Which];
            });
            builder.Show();
        }

        private void SelectStorage_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(Resource.String.ArticleDetails_StorageLabel);
            builder.SetItems(this.Storages.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);
                textView.Text = this.Storages[a.Which];
            });
            builder.Show();
        }

        private void BerechneCalPerUnit(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (this.ignoreTextChangeEvent)
                return;

            string unitPerX = UnitConvert.GetConvertUnit(this.unit.Text);

            string calPerUnit = UnitConvert.GetCaloriePerUnit(
                this.size.Text,
                this.unit.Text,
                this.calorie.Text);

            this.ignoreTextChangeEvent = true;
            if (calPerUnit != "---")
            {
                this.caloriePerUnit.Text = calPerUnit;
                this.caloriePerUnit.Enabled = true;
            }
            else
            {
                this.caloriePerUnit.Text = "---";
                this.caloriePerUnit.Enabled = false;
            }
            this.ignoreTextChangeEvent = false;

            // Text auf "Kalorien pro 100 ??" setzen.
            string perUnitText = Resources.GetString(Resource.String.ArticleDetails_CaloriesPerUnit);
            perUnitText = string.Format(perUnitText, unitPerX);
            this.caloriePerUnitLabel.Text = perUnitText;
        }

        private void BerechneCalGes(object sender, Android.Text.TextChangedEventArgs e)
        {
            if (this.ignoreTextChangeEvent)
                return;

            string calorieGes = UnitConvert.GetGesamtCalorie(
                this.size.Text,
                this.unit.Text,
                this.caloriePerUnit.Text);

            if (calorieGes == "")
                return;

            // Hat sich nichts geändert?
            if (this.calorie.Text == calorieGes)
                return;

            this.ignoreTextChangeEvent = true;
            this.calorie.Text = calorieGes.ToString();
            this.ignoreTextChangeEvent = false;
        }

        private void TakeOrShowPhoto(object sender, EventArgs e)
        {
            if (!this.IsPhotoSelected)
            {
                this.TakeAPhoto();
                return;
            };


            var articleImage = new Intent (this, typeof(ArticleImageActivity));
            articleImage.PutExtra("ArticleId", this.articleId);
            articleImage.PutExtra("EditMode", true);
            StartActivityForResult (articleImage, EditPhoto);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ArticleDetails_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            IMenuItem itemDelete = menu.FindItem(Resource.Id.ArticleDetailsMenu_Delete);
            itemDelete.SetVisible(this.article?.ArticleId > 0);

            IMenuItem itemShowPicture = menu.FindItem(Resource.Id.ArticleDetailsMenu_ShowPicture);
            itemShowPicture.SetEnabled(this.IsPhotoSelected);

            IMenuItem itemRemovePicture = menu.FindItem(Resource.Id.ArticleDetailsMenu_RemovePicture);
            itemRemovePicture.SetEnabled(this.IsPhotoSelected);

            IMenuItem itemSpeech = menu.FindItem(Resource.Id.ArticleDetailsMenu_Speech);
            itemSpeech.SetEnabled(this.IsThereAnSpeechAvailable());

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                itemSpeech.SetEnabled(false);

                IMenuItem itemEanScan = menu.FindItem(Resource.Id.ArticleDetailsMenu_ScanEAN);
                itemEanScan.SetEnabled(false);
            }

            string EANCode = FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text;

            IMenuItem itemInternetDB = menu.FindItem(Resource.Id.ArticleDetailsMenu_InternetDB);
            itemInternetDB.SetEnabled(!string.IsNullOrEmpty(EANCode));

            menu.FindItem(Resource.Id.ArticleDetailsMenu_ToStorageQuantity).SetVisible(!this.noStorageQuantity);
            menu.FindItem(Resource.Id.ArticleDetailsMenu_Delete).SetVisible(!this.noDeleteArticle);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetailsMenu_Delete:
                    this.DeleteArticle();
                    return true;

                case Resource.Id.ArticleDetailsMenu_Save:
                    if (!this.SaveArticle())
                    {
                        return false;
                    }

                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetailsMenu_Cancel:
                    this.MoveTaskToBack(false);
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetailsMenu_MakeAPhoto:
                    this.TakeAPhoto();
                    return true;

                case Resource.Id.ArticleDetailsMenu_SelectAPicture:
                    this.SelectAPicture();
                    return true;

                case Resource.Id.ArticleDetailsMenu_ShowPicture:
                    if (this.IsPhotoSelected)
                    { 
                        var articleImage = new Intent (this, typeof(ArticleImageActivity));
                        articleImage.PutExtra("ArticleId", this.articleId);
                        articleImage.PutExtra("EditMode", true);
                        StartActivityForResult (articleImage, EditPhoto);
                    }
                    return true;

                case Resource.Id.ArticleDetailsMenu_RemovePicture:
                    if (this.IsPhotoSelected)
                    {
                        // Erstelltes oder ausgewähltes Bild entfernen
                        ArticleDetailsActivity.imageLarge = null;
                        ArticleDetailsActivity.imageSmall = null;

                        this.articleImage.ImageLarge = null;    // Änderungen verwerfen
                        this.articleImage.ImageSmall = null;    // Gespeichertes Bild auch löschen

                        this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);
                        this.imageView2.SetImageResource(Resource.Drawable.ic_photo_white_24dp);
                        this.imageView2.Visibility = ViewStates.Visible;

                        this.ShowStoreQuantityInfo();
                        
                        this.isChanged = true;
                    }
                    return true;

                case Resource.Id.ArticleDetailsMenu_ScanEAN:
                    //this.SearchEANCode("4444444");
                    StartActivityForResult(typeof(ZXingFragmentActivity), EANScanID);

                    return true;

                case Resource.Id.ArticleDetailsMenu_ToShoppingList:
                    this.SaveAndAddToShoppingList();                // Bei Neuanlage erst Artikel speichern (sonst keine Referenz aus dem Einkaufszettel)
                    return true;

                case Resource.Id.ArticleDetailsMenu_ToStorageQuantity:
                    this.SaveAndGoToStorageItem();
                    return true;

                case Resource.Id.ArticleDetailsMenu_Speech:
                    this.SprachEingabe();
                    return true;

                case Resource.Id.ArticleDetailsMenu_InternetDB:
                    this.SearchEanCodeOnInternetDb();
                    return true;
            }

            return false;
        }

        public static bool showCostMessage = true;

        private void SearchEanCodeOnInternetDb()
        {
            string EANCode = FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text;

            if (string.IsNullOrEmpty(EANCode))
                return;

            // API aufrufen, ohne auf die Kosten zu verweisen.
            if (!ArticleDetailsActivity.showCostMessage)
            {
                var internetDB = new Intent(this, typeof(InternetDatabaseSearchActivity));
                internetDB.PutExtra("EANCode", EANCode);
                this.StartActivityForResult(internetDB, InternetDB);
                return;
            }

            var message = new AlertDialog.Builder(this);
            message.SetTitle(this.Resources.GetString(Resource.String.ArticleDetails_SearchEANScan));
            message.SetMessage(this.Resources.GetString(Resource.String.ArticleDetails_SearchOnOpenFoodFacts));
            message.SetIcon(Resource.Drawable.ic_launcher);

            Switch checkBox = new Switch(this)
            {
                Text = this.Resources.GetString(Resource.String.ArticleDetails_StopShowingWarning),
                TextSize = 14
            };
            checkBox.SetPadding(50, 50, 20, 20);
            message.SetView(checkBox);
            message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Yes), (s, e) => 
            {
                var internetDB = new Intent(this, typeof(InternetDatabaseSearchActivity));
                internetDB.PutExtra("EANCode", EANCode);
                this.StartActivityForResult(internetDB, InternetDB);

                if (checkBox.Checked)
                {
                    Settings.PutBoolean("ShowOpenFoodFactsInternetCostsMessage", false);
                    ArticleDetailsActivity.showCostMessage = false;
                }
            });
            message.SetNegativeButton(this.Resources.GetString(Resource.String.App_No), (s, e) => { });
            message.Create().Show();
        }

        private void SaveAndAddToShoppingList()
        {
            if (this.articleId != 0)
            {
                this.SaveArticle();
                this.AddToShoppingListManually();
                return;
            }

            var message = new AlertDialog.Builder(this);
            message.SetMessage(this.Resources.GetString(Resource.String.ArticleDetails_SaveToAddToShippingList));
            message.SetIcon(Resource.Drawable.ic_launcher);
            message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => 
                {
                    this.SaveArticle();
                    if (this.articleId != 0)    // Speichern erfolgreich (articleId gesetzt?)
                    {
                        this.AddToShoppingListManually();
                        return;
                    }
                });
            message.SetNegativeButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => { });
            message.Create().Show();
        }

        private void SaveAndGoToStorageItem()
        {
            if (this.noStorageQuantity)
                return;

            if (this.articleId != 0)
            {
                this.SaveArticle();
                this.GoToStorageItem(this.articleId);
                return;
            }

            var message = new AlertDialog.Builder(this);
            message.SetMessage(this.Resources.GetString(Resource.String.ArticleDetails_SaveToAddToStorage));
            message.SetIcon(Resource.Drawable.ic_launcher);
            message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => 
                {
                    this.SaveArticle();
                    if (this.articleId != 0)    // Speichern erfolgreich (articleId gesetzt?)
                    {
                        this.GoToStorageItem(this.articleId);
                        return;
                    }
                });
            message.SetNegativeButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => { });
            message.Create().Show();
        }

        private void GoToStorageItem(int articleId)
        {
            if (this.noStorageQuantity)
                return;

            var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
            storageDetails.PutExtra("ArticleId", articleId);
            storageDetails.PutExtra("NoArticleDetails", true);
            this.StartActivityForResult(storageDetails, StorageQuantityId);
        }

        private void SearchEANCode(string eanCode)
        {
            if (string.IsNullOrEmpty(eanCode))
                return;

            TRACE("Scanned Barcode: {0}", eanCode);
            EditText editTextEanCode = FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode);
            if (string.IsNullOrEmpty(editTextEanCode.Text))
            {
                editTextEanCode.Text = eanCode;
                return;
            }

            var message = new AlertDialog.Builder(this);
            message.SetIcon(Resource.Drawable.ic_launcher);

            if (editTextEanCode.Text.Contains(eanCode))
            {
                message.SetMessage(this.Resources.GetString(Resource.String.ArticleDetails_EanCodeAlreadyContains));
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => {});
                message.Create().Show();

                return;
            }

            message.SetMessage(this.Resources.GetString(Resource.String.ArticleDetails_EanCodeReplaceOrInsert));
            message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Replace), (s, e) => 
            {
                editTextEanCode.Text = eanCode;
            });
            message.SetNegativeButton(this.Resources.GetString(Resource.String.App_Insert), (s, e) => 
            { 
                editTextEanCode.Text = editTextEanCode.Text +  "," + eanCode;
            });
            message.Create().Show();

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if (requestCode == SpechId)
            {
                var matches = data.GetStringArrayListExtra(RecognizerIntent.ExtraResults);
                if (matches.Count != 0)
                {
                    EditText text = FindViewById<EditText>(Resource.Id.ArticleDetails_Name);
                    string textInput = text.Text + matches[0];
                    text.Text = textInput;
                }
            }

            if (requestCode == EditPhoto)
            {
                Bitmap bitmap = BitmapFactory.DecodeByteArray(ArticleDetailsActivity.imageLarge, 0, ArticleDetailsActivity.imageLarge.Length);
                
                this.imageView.SetImageBitmap(bitmap);
                this.isChanged = true;
            }

            if (requestCode == InternetDB)
            {
                string  name       = data.GetStringExtra("Name");
                string  hersteller = data.GetStringExtra("Hersteller");
                decimal quantity   = data.GetLongExtra  ("Quantity", -1);
                string  unit       = data.GetStringExtra("Unit");
                decimal kcalPer100 = data.GetLongExtra  ("KCalPer100", -1);

                if (!string.IsNullOrEmpty(name))
                    FindViewById<EditText>(Resource.Id.ArticleDetails_Name).Text = name;

                if (!string.IsNullOrEmpty(hersteller))
                    FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text = hersteller;

                if (quantity > 0)
                    this.size.Text = quantity.ToString(this.currentCulture);

                if (!string.IsNullOrEmpty(unit))
                    this.unit.Text = unit;

                if (kcalPer100 > 0)
                {
                    this.caloriePerUnit.Text = kcalPer100.ToString();
                    this.BerechneCalGes(this, null);
                }

                if (InternetDatabaseSearchActivity.picture != null)
                {
                    this.ResizeBitmap(InternetDatabaseSearchActivity.picture);
                }
            }

            if (requestCode == StorageQuantityId)
            {
                this.ShowStoreQuantityInfo();
            }

            if ((requestCode == EANScanID) && (data != null))
            {
                string eanCode = data.GetStringExtra("EANCode");
                this.SearchEANCode(eanCode);
            }
        }

        public override void OnBackPressed()
        {
            if (this.isChanged)
            {
                Intent intent = new Intent();
                this.SetResult(Result.Ok, intent);
            }

            base.OnBackPressed();

            this.article = null;
            ArticleDetailsActivity.imageLarge = null;
            ArticleDetailsActivity.imageSmall = null;
            InternetDatabaseSearchActivity.picture = null;
        }

        private async void SelectAPicture()
        {
            try
            {
                FileResult photo = await MediaPicker.PickPhotoAsync();
                if (photo == null)
                    return;
            
                this.LoadAndResizeBitmap(photo.FullPath);
            }
            catch(Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        private async void TakeAPhoto()
        {
            try
            {
                if (MainActivity.IsGooglePlayPreLaunchTestMode)
                    return;

                var result = await MediaPicker.CapturePhotoAsync();
                if (result == null)
                    return;
            
                this.LoadAndResizeBitmap(result.FullPath);
            }
            catch(Exception ex)
            {
                Toast.MakeText(this, ex.Message, ToastLength.Long).Show();
            }
        }

        private bool SaveArticle()
        {
            int?     warnInDays = null;
            int?     calorie = null;
            decimal? size = null;
            int?     minQuantity = null;
            int?     prefQuantity = null;
            decimal? price = null;

            try
            {
                warnInDays   = GetIntegerFromEditText(Resource.Id.ArticleDetails_WarnInDays);
                size         = GetDecimalFromEditText(Resource.Id.ArticleDetails_Size);
                calorie      = GetIntegerFromEditText(Resource.Id.ArticleDetails_Calorie);
                minQuantity  = GetIntegerFromEditText(Resource.Id.ArticleDetails_MinQuantity);
                prefQuantity = GetIntegerFromEditText(Resource.Id.ArticleDetails_PrefQuantity);
                price        = GetDecimalFromEditText(Resource.Id.ArticleDetails_Price);


                // Absichern gegen Unsinn.
                if (warnInDays > 365*1000)
                    warnInDays = 365*1000;

                this.article.Name            = FindViewById<EditText>(Resource.Id.ArticleDetails_Name).Text;
                this.article.Manufacturer    = FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text;
                this.article.Category        = this.catalogListener.Value;
                this.article.SubCategory     = FindViewById<EditText>(Resource.Id.ArticleDetails_SubCategory).Text;
                this.article.DurableInfinity = FindViewById<Switch>(Resource.Id.ArticleDetails_DurableInfinity).Checked;
                this.article.WarnInDays      = warnInDays;
                this.article.Price           = price;
                this.article.Size            = size;
                this.article.Calorie         = calorie;
                this.article.Unit            = FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).Text;
                this.article.MinQuantity     = minQuantity;
                this.article.PrefQuantity    = prefQuantity;
                this.article.StorageName     = FindViewById<EditText>(Resource.Id.ArticleDetails_Storage).Text;
                this.article.Supermarket     = FindViewById<EditText>(Resource.Id.ArticleDetails_Supermarket).Text;
                this.article.EANCode         = FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text;
                this.article.Notes           = FindViewById<EditText>(Resource.Id.ArticleDetails_Notes).Text;

                this.article.Manufacturer    = this.article.Manufacturer?.TrimEnd();
                this.article.SubCategory     = this.article.SubCategory?.TrimEnd();
                this.article.StorageName     = this.article.StorageName?.TrimEnd();
                this.article.Supermarket     = this.article.Supermarket?.TrimEnd();

                if (ArticleDetailsActivity.imageLarge != null)
                    this.articleImage.ImageLarge = ArticleDetailsActivity.imageLarge;

                if (ArticleDetailsActivity.imageSmall != null)
                    this.articleImage.ImageSmall = ArticleDetailsActivity.imageSmall;

                SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
                if (databaseConnection == null)
                    return false;

                if (this.article.ArticleId > 0)
                {
                    databaseConnection.Update(this.article);
                }
                else
                {
                    databaseConnection.Insert(this.article);
                    this.articleId = this.article.ArticleId;
                }

                if (this.articleImage.ImageLarge != null)   // Ein neues Bild wurde ausgewählt oder vorhandenes geändert.
                {
                    if (this.articleImage.ImageId > 0)
                    {
                        databaseConnection.Update(this.articleImage);
                    }
                    else
                    {
                        this.articleImage.ArticleId = this.articleId;
                        this.articleImage.Type = 0;
                        this.articleImage.CreatedAt = DateTime.Now;
                        databaseConnection.Insert(this.articleImage);
                    }
                }

                if ((this.articleImage.ImageSmall == null) && (this.articleImage.ImageId > 0))  // Vorhandenes Bild gelöscht?
                {
                    databaseConnection.Delete(this.articleImage);
                }
            }
            catch(Exception ex)
            {
                TRACE(ex);

                string fehlerText = ex.Message;

                string text = fehlerText + "\n\nSoll eine E-Mail mit dem Fehler an den Entwickler geschickt werden?";
                text += "\n\n(Ihre E-Mail Adresse wird dem Entwickler angezeigt)?";

                var message = new AlertDialog.Builder(this);
                message.SetMessage(text);
                message.SetPositiveButton(this.Resources.GetString(Resource.String.App_Yes), (s, e) => 
                    {
                        fehlerText += "\n";
                        fehlerText += "\nMessage: " + ex.Message;
                        fehlerText += "\nStackTrace: " + ex.StackTrace;
                        fehlerText += "\nCurrentCulture: " + CultureInfo.CurrentCulture.DisplayName;
                        fehlerText += "\nCurrentUICulture: " + CultureInfo.CurrentUICulture.DisplayName;

                        var emailIntent = new Intent(Intent.ActionSend);
                        emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, new[] { "cstryi@freenet.de" });
                        emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fehlerbericht: Vorratsübersicht");
                        emailIntent.SetType("message/rfc822");
                        emailIntent.PutExtra(Android.Content.Intent.ExtraText, fehlerText);
                        StartActivity(Intent.CreateChooser(emailIntent, "E-Mail an Entwickler senden mit..."));
                    });
                message.SetNegativeButton(this.Resources.GetString(Resource.String.App_No), (s, e) => { });
                message.Create().Show();

                size = 0;
                return false;
            }

            this.isChanged = true;

            return true;
        }

        private void DeleteArticle()
        {
            // Prüfen, ob noch Bestand an dem Artikel vorhanden ist
            decimal anzahl = Database.GetArticleQuantityInStorage(this.articleId);
            if (anzahl > 0)
            {
                string message = string.Format(this.Resources.GetString(Resource.String.ArticleDetails_CanNotDeleteBecauseStorage), anzahl);

                var builder1 = new AlertDialog.Builder(this);
                builder1.SetMessage(message);
                builder1.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
                builder1.Create().Show();

                return;
            }

            string msg = this.Resources.GetString(Resource.String.ArticleDetails_DeleteArticlerReally);

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (isInList)
            {
                msg += "\n\n";
                msg += this.Resources.GetString(Resource.String.ArticleDetails_ArticleOnShoppingList);
            }

            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(msg);
            builder.SetNegativeButton(this.Resources.GetString(Resource.String.App_No), (s, e) => { });
            builder.SetPositiveButton(this.Resources.GetString(Resource.String.App_Yes), (s, e) => 
            { 
                SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
                if (this.article.ArticleId > 0)
                {
                    Database.DeleteArticle(this.article.ArticleId);
                    this.SetResult(Result.Ok);

                    this.OnBackPressed();
                }

            });
            builder.Create().Show();

        }

        private void AddToShoppingListManually()
        {
            int? minQuantity  = GetIntegerFromEditText(Resource.Id.ArticleDetails_MinQuantity);
            int? prefQuantity = GetIntegerFromEditText(Resource.Id.ArticleDetails_PrefQuantity);

            AddToShoppingListDialog.ShowDialog(this, articleId, minQuantity, prefQuantity);
        }

        private bool IsThereAnSpeechAvailable()
        {
            Intent intent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            IList<ResolveInfo> availableActivities =
                this.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        // Anhand vom https://docs.microsoft.com/de-de/xamarin/android/platform/speech
        private void SprachEingabe()
        {
            string rec = Android.Content.PM.PackageManager.FeatureMicrophone;
            if (rec != "android.hardware.microphone")
            {
                var alert = new AlertDialog.Builder(this);
                alert.SetTitle("You don't seem to have a microphone to record with");
                alert.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (sender, e) =>
                {
                    return;
                });
                alert.Show();
            }

            var voiceIntent = new Intent(RecognizerIntent.ActionRecognizeSpeech);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguageModel, RecognizerIntent.LanguageModelFreeForm);
            voiceIntent.PutExtra(RecognizerIntent.ExtraPrompt, Application.Context.GetString(Resource.String.ArticleDetails_ArticleName));
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputPossiblyCompleteSilenceLengthMillis, 1500);
            voiceIntent.PutExtra(RecognizerIntent.ExtraSpeechInputMinimumLengthMillis, 15000);
            voiceIntent.PutExtra(RecognizerIntent.ExtraMaxResults, 1);
            voiceIntent.PutExtra(RecognizerIntent.ExtraLanguage, Java.Util.Locale.Default);
            StartActivityForResult(voiceIntent, SpechId);
        }

        private void ShowPictureAndDetails(string eanCode)
        {
            FindViewById<TextView>(Resource.Id.ArticleDetails_ArticleId).Text       = string.Format("ArticleId: {0}", article.ArticleId);

            FindViewById<EditText>(Resource.Id.ArticleDetails_Name).Text               = article.Name;
            FindViewById<Switch>  (Resource.Id.ArticleDetails_DurableInfinity).Checked = article.DurableInfinity;

            if (article.WarnInDays.HasValue)   FindViewById<EditText>(Resource.Id.ArticleDetails_WarnInDays).Text   = article.WarnInDays.Value.ToString();
            if (article.Calorie.HasValue)      FindViewById<EditText>(Resource.Id.ArticleDetails_Calorie).Text      = this.article.Calorie.ToString();
            if (article.Size.HasValue)         FindViewById<EditText>(Resource.Id.ArticleDetails_Size).Text         = article.Size.Value.ToString(CultureInfo.InvariantCulture);
            if (article.MinQuantity.HasValue)  FindViewById<EditText>(Resource.Id.ArticleDetails_MinQuantity).Text  = article.MinQuantity.Value.ToString();
            if (article.PrefQuantity.HasValue) FindViewById<EditText>(Resource.Id.ArticleDetails_PrefQuantity).Text = article.PrefQuantity.Value.ToString();
            if (article.Price.HasValue)        FindViewById<EditText>(Resource.Id.ArticleDetails_Price).Text        = article.Price.Value.ToString(CultureInfo.InvariantCulture);

            FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).Text               = article.Unit;
            FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text            = article.EANCode;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Notes).Text              = article.Notes;

            this.warningInDaysView.Enabled      = !article.DurableInfinity;
            this.warningInDaysLabelView.Enabled = !article.DurableInfinity;


            // Hersteller
            this.Manufacturers = Database.GetManufacturerNames();

            var manufacturer = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Manufacturer);
            var manufacturerAdapter = (ArrayAdapter<String>)(manufacturer.Adapter);
            manufacturerAdapter.Clear();
            manufacturerAdapter.AddAll(this.Manufacturers);
            manufacturerAdapter.NotifyDataSetChanged();

            // Kategorie
            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.ArticleDetails_Category);

            var categoryAdapter = (ArrayAdapter<String>)(categorySpinner.Adapter);
            
            if (article.ArticleId  <= 0)
            {
                article.Category = Database.GetSettingsString("DEFAULT_CATEGORY");
            }

            int position = categoryAdapter.GetPosition(article.Category);
            if (position < 0)
            {
                position = 0;
            }

            categorySpinner.SetSelection(position);


            // Unterkategorie
            var subCategories = Database.GetSubcategoriesOf();

            var subCategory = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);
            var subCategoryAdapter = (ArrayAdapter<String>)(subCategory.Adapter);
            subCategoryAdapter.Clear();
            subCategoryAdapter.AddAll(subCategories);
            subCategoryAdapter.NotifyDataSetChanged();

            // Lagerort
            this.Storages = Database.GetStorageNames();

            var storage = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);
            var storageAdapter = (ArrayAdapter<String>)(storage.Adapter);
            storageAdapter.Clear();
            storageAdapter.AddAll(this.Storages);
            storageAdapter.NotifyDataSetChanged();

            // Einkaufsmarkt
            this.Supermarkets = Database.GetSupermarketNames();

            var supermarket = FindViewById<AutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);
            var supermarketAdapter = (ArrayAdapter<String>)(supermarket.Adapter);
            supermarketAdapter.Clear();
            supermarketAdapter.AddAll(this.Supermarkets);
            supermarketAdapter.NotifyDataSetChanged();

            FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text = article.Manufacturer;
            FindViewById<EditText>(Resource.Id.ArticleDetails_SubCategory).Text  = article.SubCategory;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Supermarket).Text  = article.Supermarket;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Storage).Text      = article.StorageName;

            this.ShowStoreQuantityInfo();

            if (this.articleImage.ImageSmall != null)
            {
                this.imageView2.Visibility = ViewStates.Gone;
                try
                {
                    Bitmap smallBitmap = BitmapFactory.DecodeByteArray(this.articleImage.ImageSmall, 0, this.articleImage.ImageSmall.Length);
                
                    this.imageView.SetImageBitmap(smallBitmap);
                    /*
                    string text = string.Empty;
                    text += string.Format("Voransicht: {0:n0} X {1:n0}\n", smallBitmap.Width, smallBitmap.Height);
                    text += string.Format("Größe: {0:n0}\n", Tools.ToFuzzyByteString(smallBitmap.ByteCount));
                    text += string.Format("Komprimiert: {0:n0}\n", Tools.ToFuzzyByteString(this.articleImage.ImageSmall.Length));
                    this.imageTextView.Text = text;
                    */
                }
                catch(Exception ex)
                {
                    TRACE(ex);

                    this.imageTextView.Text = ex.Message;

                    this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                    this.imageView.Enabled = false;
                }
            }
            else
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);

            if (!string.IsNullOrEmpty(eanCode))
            {
                FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text = eanCode;
            }
        }

        private void ShowStoreQuantityInfo()
        {
			var storageItemBestList = Database.GetBestBeforeItemQuantity(article.ArticleId);

            decimal bestand = 0;
            decimal vorDemAblauf = 0;
            decimal mitWarnung = 0;
            decimal abgelaufen = 0;
			
			foreach(StorageItemQuantityResult result in storageItemBestList)
			{
                bestand += result.Quantity;
				if (result.WarningLevel == 0)
				{
                    vorDemAblauf += result.Quantity;
				}
				if (result.WarningLevel == 1)
				{
                    mitWarnung += result.Quantity;
				}
				if (result.WarningLevel == 2)
				{
                    abgelaufen += result.Quantity;
                }
            }

            string info;

            info = string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.StorageItem_InventoryInPieces), bestand);

            if (vorDemAblauf > 0)
            {
			    if (!string.IsNullOrEmpty(info)) info += "\r\n";
			    info += string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ArticleDetails_WithExpiryDate), vorDemAblauf);
            }

            if (mitWarnung > 0)
            {
			    if (!string.IsNullOrEmpty(info)) info += "\r\n";
			    info += string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ArticleDetails_WithWarnings), mitWarnung);
            }

            if (abgelaufen > 0)
            {
			    if (!string.IsNullOrEmpty(info)) info += "\r\n";
			    info += string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ArticleDetails_AfterExpiryDate), abgelaufen);
            }

            this.imageTextView.Text = info;
        }

        private void ResizeBitmap(Bitmap newBitmap)
        {
            MemoryStream stream;
            byte[] resizedImage;

            try
            {
                int widthLarge  = 854;
                int heightLarge = 854;

                Bitmap largeBitmap = newBitmap;

                var compress = Settings.GetBoolean("CompressPictures", true);
                if (compress)
                {
                    int compressMode = Settings.GetInt("CompressPicturesMode", 1);
                    if (compressMode == 2)
                    {
                        widthLarge  = 1_024;
                        heightLarge = 1_024;
                    }

                    if (compressMode == 3)
                    {
                        widthLarge  = 1_280;
                        heightLarge = 1_280;
                    }

                    if (compressMode == 4)
                    {
                        widthLarge  = 1_536;
                        heightLarge = 1_536;
                    }

                    widthLarge = Math.Min (newBitmap.Width,  widthLarge);
                    heightLarge = Math.Min(newBitmap.Height, heightLarge);

                    resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, widthLarge, heightLarge);
                    largeBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);
                }

                stream = new MemoryStream();
                largeBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                ArticleDetailsActivity.imageLarge = stream.ToArray();

                // --------------------------------------------------------------------------------
                // Miniaturansicht erstellen
                // --------------------------------------------------------------------------------

                resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, 48*2, 85*2);

                Bitmap smallBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

                stream = new MemoryStream();
                smallBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                ArticleDetailsActivity.imageSmall = stream.ToArray();

                RunOnUiThread(() => this.imageView.SetImageBitmap(smallBitmap));
                RunOnUiThread(() => this.imageView2.Visibility = ViewStates.Gone);

                TRACE("-------------------------------------");
                TRACE(string.Format("Org.: {0:n0} x {1:n0} ({2:n0})\r\n", newBitmap.Width,   newBitmap.Height,   Tools.ToFuzzyByteString(newBitmap.ByteCount)));
                TRACE(string.Format("Bild: {0:n0} x {1:n0} ({2:n0})\r\n", largeBitmap.Width, largeBitmap.Height, Tools.ToFuzzyByteString(largeBitmap.ByteCount)));
                TRACE(string.Format("Thn.: {0:n0} x {1:n0} ({2:n0})",     smallBitmap.Width, smallBitmap.Height, Tools.ToFuzzyByteString(smallBitmap.ByteCount)));
                TRACE("-------------------------------------");
            }
            catch(Exception ex)
            {
                TRACE(ex);

                RunOnUiThread(() => this.imageTextView.Text = ex.Message);
            }
        
        }


        private void LoadAndResizeBitmap(string fileName)
        {
            var progressDialog = this.CreateProgressBar();

            new Thread(new ThreadStart(delegate             
            {
                Bitmap bitmap = BitmapFactory.DecodeFile(fileName);
                if (bitmap == null)
                {
                    this.HideProgressBar(progressDialog);
                    return;
                }

                this.ResizeBitmap(bitmap);

                File.Delete(fileName);

                this.HideProgressBar(progressDialog);

                // Dispose of the Java side bitmap.
                GC.Collect();

            })).Start();
        }

        private int? GetIntegerFromEditText(int resourceId)
        {
            int? value = null;

            string valueText  = FindViewById<EditText>(resourceId).Text;

            try
            {
                if (!string.IsNullOrEmpty(valueText))
                {
                    value = Convert.ToInt32(valueText, this.currentCulture);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Fehler beim Konvertieren von '{0}' in ein Integer.", valueText), ex);
            }

            return value;
        }

        
        private decimal? GetDecimalFromEditText(int resourceId)
        {
            decimal? value = null;

            string valueText  = FindViewById<EditText>(resourceId).Text;

            try
            {
                if (!string.IsNullOrEmpty(valueText))
                {
                    value = Convert.ToDecimal(valueText, CultureInfo.InvariantCulture);
                }
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Fehler beim Konvertieren von '{0}' in ein Decimal.", valueText), ex);
            }
            return value;
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

    public class CatalogItemSelectedListener : Java.Lang.Object, IOnItemSelectedListener
    {
        public Color? TextColor = null;

        public string Value { get; private set; }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            TextView textView = view as TextView;
            if (textView == null) return;

            if (this.TextColor != null)
            {
                textView.SetTextColor(this.TextColor.Value);
            }

            this.Value = textView.Text;
        }

        public void OnNothingSelected(AdapterView parent)
        {
            this.Value = null;
        }
    }

}