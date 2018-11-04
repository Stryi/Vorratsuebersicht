using System;
using System.Globalization;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Provider;
using Android.Content.PM;
using MatrixGuide;
using static Android.Widget.AdapterView;
using System.Threading;
using Android.Support.V4.Content;
using Android.Speech;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "@string/Main_Button_Artikelangaben", Icon = "@drawable/ic_local_offer_white_48dp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class ArticleDetailsActivity : Activity
    {
        internal static byte[] imageLarge;
        internal static byte[] imageSmall;

        Article article;
        int articleId;
        bool isChanged = false;
        ImageView imageView;
        ImageView imageView2;
        TextView  imageTextView;
        EditText  warningInDaysView;
        TextView  warningInDaysLabelView;
        Toast toast;
        ImageCaptureHelper imageCapture;

        IList<string> Manufacturers;
        CatalogItemSelectedListener catalogListener;
        IList<string> SubCategories;
        IList<string> Storages;
        IList<string> Supermarkets;

        string category;
        string subCategory;
        bool cameraExists;

        public static readonly int PickImageId = 1000;
        public static readonly int TakePhotoId = 1001;
        public static readonly int SpechId = 1002;
        public static readonly int EditPhoto = 1003;

        public bool IsPhotoSelected
        {
            get 
            {
                if (this.article.Image != null)
                    return true;

                if (ArticleDetailsActivity.imageLarge != null)
                    return true;

                return false;
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            var stopWatch = new System.Diagnostics.Stopwatch();
            stopWatch.Start();
            base.OnCreate(savedInstanceState);

            this.SetContentView(Resource.Layout.ArticleDetails);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            string text      = Intent.GetStringExtra ("Name") ?? string.Empty;
            this.articleId   = Intent.GetIntExtra    ("ArticleId", 0);
            string eanCode   = Intent.GetStringExtra ("EANCode") ?? string.Empty;
            this.category    = Intent.GetStringExtra ("Category");
            this.subCategory = Intent.GetStringExtra ("SubCategory");

            this.imageCapture = new ImageCaptureHelper();
            this.cameraExists = this.imageCapture.Initializer(this);

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

            // Hersteller Eingabe
            this.Manufacturers = new List<string>();

            var manufacturer = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Manufacturer);

            ArrayAdapter<String> manufacturerAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Manufacturers);
            manufacturer.Adapter = manufacturerAdapter;
            manufacturer.Threshold = 1;
            manufacturer.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            manufacturer.SetTokenizer(new SpaceTokenizer());


            // Kategorie Auswahl
            this.catalogListener = new CatalogItemSelectedListener();

            string[] Categories = Resources.GetStringArray(Resource.Array.ArticleCatagories);

            ArrayAdapter<String> categoryAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, Categories);
            categoryAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

            Spinner categorySpinner = (Spinner)FindViewById<Spinner>(Resource.Id.ArticleDetails_Category);
            categorySpinner.Adapter = categoryAdapter;
            categorySpinner.OnItemSelectedListener = this.catalogListener;

            // Unterkategorie Eingabe
            this.SubCategories = new List<string>();

            var subCategory = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);

            ArrayAdapter<String> subCategoryAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.SubCategories);
            subCategory.Adapter = subCategoryAdapter;
            subCategory.Threshold = 1;
            subCategory.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            subCategory.SetTokenizer(new SpaceTokenizer());

            // Lagerort Eingabe
            this.Storages = new List<string>();

            var storage = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);

            ArrayAdapter<String> storageAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Storages);
            storage.Adapter = storageAdapter;
            storage.Threshold = 1;
            storage.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            storage.SetTokenizer(new SpaceTokenizer());

            // Einkaufsmarkt Eingabe
            this.Supermarkets = new List<string>();

            var supermarket = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);

            ArrayAdapter<String> supermarketAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Supermarkets);
            supermarket.Adapter = supermarketAdapter;
            supermarket.Threshold = 1;
            supermarket.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            supermarket.SetTokenizer(new SpaceTokenizer());


            this.ShowPictureAndDetails(this.articleId, eanCode);

            imageView.Click     += TakeOrShowPhoto;
            imageTextView.Click += TakeOrShowPhoto;

            imageView2.Click += delegate
            {
                this.SelectAPicture();
            };

            if (!string.IsNullOrEmpty(this.article.Name))
            { 
                // Artikelname ist eingetragen. Tastatus anf�nglich ausblenden.
                this.Window.SetSoftInputMode(SoftInput.StateHidden);
            }
            stopWatch.Stop();
            TRACE("Dauer Laden der Artikeldaten: {0}", stopWatch.Elapsed.ToString());
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

            IMenuItem itemAddPhoto = menu.FindItem(Resource.Id.ArticleDetailsMenu_MakeAPhoto);
            itemAddPhoto.SetVisible(this.cameraExists);

            IMenuItem itemShowPicture = menu.FindItem(Resource.Id.ArticleDetailsMenu_ShowPicture);
            itemShowPicture.SetEnabled(this.IsPhotoSelected);

            IMenuItem itemSpeech = menu.FindItem(Resource.Id.ArticleDetailsMenu_Speech);
            itemSpeech.SetEnabled(this.IsThereAnSpeechAvailable());

            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                IMenuItem itemEanScan = menu.FindItem(Resource.Id.ArticleDetailsMenu_ScanEAN);
                itemEanScan.SetEnabled(false);
            }

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

                case Resource.Id.ArticleDetailsMenu_ScanEAN:
                    this.ScanEAN();
                    return true;

                case Resource.Id.ArticleDetailsMenu_ToShoppingList:
                    this.SaveAndAddToShoppingList();                // Bei Neuanlage erst Artikel speichern (sonst keine Referenz aus dem Einkaufszettel)
                    return true;

                case Resource.Id.ArticleDetailsMenu_Speech:
                    this.SprachEingabe();
                    return true;

            }

            return false;
        }

        private void SaveAndAddToShoppingList()
        {
            if (this.articleId != 0)
            {
                this.AddToShoppingListAutomatically();
                return;
            }

            var message = new AlertDialog.Builder(this);
            message.SetMessage("Ein Artikel kann erst nach der Neuanlage auf die Einkaufsliste kommen.\n\nArtikel speichern (anlegen)?");
            message.SetIcon(Resource.Drawable.ic_launcher);
            message.SetPositiveButton("OK", (s, e) => 
                {
                    this.SaveArticle();
                    if (this.articleId != 0)    // Speichern erfolgreich (articleId gesetzt?)
                    {
                        this.AddToShoppingListAutomatically();
                        return;
                    }
                });
            message.SetNegativeButton("Abbrechen", (s, e) => { });
            message.Create().Show();
        }

        private async void ScanEAN()
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var scanResult = await scanner.Scan();

            if (scanResult == null)
                return;

            TRACE("Scanned Barcode: {0}", scanResult.Text);
            FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text = scanResult.Text;
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }

            if ((requestCode == PickImageId) && (data != null))
            {
                var progressDialog = this.CreateProgressBar();
                new Thread(new ThreadStart(delegate             
                {
                    // Dispose of the Java side bitmap.
                    GC.Collect();

                    Android.Net.Uri uri = data.Data;
                    Bitmap newBitmap = MediaStore.Images.Media.GetBitmap( this.ContentResolver, uri);

                    this.ResizeBitmap(newBitmap);

                    // Dispose of the Java side bitmap.
                    GC.Collect();

                    this.HideProgressBar(progressDialog);

                })).Start();
            }

            if ((requestCode == TakePhotoId))
            {
                var progressDialog = this.CreateProgressBar();
                new Thread(new ThreadStart(delegate             
                {
                    string path = this.imageCapture.FilePath;
                    if (System.IO.File.Exists(path))
                    {
                        this.LoadAndResizeBitmap(path);
                    }
                    else
                    {
                        RunOnUiThread(() =>
                        {
                            string message = string.Format("Fotodatei '{0}' nicht gefunden.", path);
                            Toast.MakeText(this, message, ToastLength.Long);
                        });
                    }

                    this.HideProgressBar(progressDialog);

                    // Dispose of the Java side bitmap.
                    GC.Collect();

                })).Start();
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
        }

        private void SelectAPicture()
        {
            Intent = new Intent();
            Intent.SetType("image/*");
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Picture"), PickImageId);
        }

        private void TakeAPhoto()
        {
            if (MainActivity.IsGooglePlayPreLaunchTestMode)
                return;

            this.imageCapture.TakePicture();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            this.imageCapture.RequestPermissions(requestCode, grantResults);
        }

        private bool SaveArticle()
        {
            int?     warnInDays = null;
            int?     calorie = null;
            decimal? size = null;
            int?     minQuantity = null;
            int?     prefQuantity = null;

            try
            {
                warnInDays   = GetIntegerFromEditText(Resource.Id.ArticleDetails_WarnInDays);
                size         = GetDecimalFromEditText(Resource.Id.ArticleDetails_Size);
                calorie      = GetIntegerFromEditText(Resource.Id.ArticleDetails_Calorie);
                minQuantity  = GetIntegerFromEditText(Resource.Id.ArticleDetails_MinQuantity);
                prefQuantity = GetIntegerFromEditText(Resource.Id.ArticleDetails_PrefQuantity);
            }
            catch(Exception ex)
            {
                string fehlerText = ex.Message;

                string text = fehlerText + "\n\nSoll eine E-Mail mit dem Fehler an den Entwickler geschickt werden?";
                text += "\n\n(Ihre E-Mail Adresse wird dem Entwickler angezeigt)?";

                var message = new AlertDialog.Builder(this);
                message.SetMessage(text);
                message.SetPositiveButton("Ja", (s, e) => 
                    {
                        fehlerText += "\n";
                        fehlerText += "\nMessage: " + ex.Message;
                        fehlerText += "\nStackTrace: " + ex.StackTrace;
                        fehlerText += "\nCurrentCulture: " + CultureInfo.CurrentCulture.DisplayName;
                        fehlerText += "\nCurrentUICulture: " + CultureInfo.CurrentUICulture.DisplayName;

                        var emailIntent = new Intent(Intent.ActionSend);
                        emailIntent.PutExtra(Android.Content.Intent.ExtraEmail, new[] { "cstryi@freenet.de" });
                        emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fehlerbericht: Vorrats�bersicht");
                        emailIntent.SetType("message/rfc822");
                        emailIntent.PutExtra(Android.Content.Intent.ExtraText, fehlerText);
                        StartActivity(Intent.CreateChooser(emailIntent, "E-Mail an Entwickler senden mit..."));
                    });
                message.SetNegativeButton("Nein", (s, e) => { });
                message.Create().Show();

                size = 0;
                return false;
            }

            this.article.Name            = FindViewById<EditText>(Resource.Id.ArticleDetails_Name).Text;
            this.article.Manufacturer    = FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text;
            this.article.Category        = this.catalogListener.Value;
            this.article.SubCategory     = FindViewById<EditText>(Resource.Id.ArticleDetails_SubCategory).Text;
            this.article.DurableInfinity = FindViewById<Switch>(Resource.Id.ArticleDetails_DurableInfinity).Checked;
            this.article.WarnInDays      = warnInDays;
            this.article.Size            = size;
            this.article.Calorie         = calorie;
            this.article.Unit            = FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).Text;
            this.article.MinQuantity     = minQuantity;
            this.article.PrefQuantity    = prefQuantity;
            this.article.StorageName     = FindViewById<EditText>(Resource.Id.ArticleDetails_Storage).Text;
            this.article.Supermarket     = FindViewById<EditText>(Resource.Id.ArticleDetails_Supermarket).Text;
            this.article.EANCode         = FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text;
            this.article.Notes           = FindViewById<EditText>(Resource.Id.ArticleDetails_Notes).Text;

            if (ArticleDetailsActivity.imageLarge != null)
                this.article.ImageLarge = ArticleDetailsActivity.imageLarge;

            if (ArticleDetailsActivity.imageSmall != null)
                this.article.Image      = ArticleDetailsActivity.imageSmall;

            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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

            this.isChanged = true;

            return true;
        }

        private void DeleteArticle()
        {
            // Pr�fen, ob noch Bestand an dem Artikel vorhanden ist
            decimal anzahl = Database.GetArticleQuantityInStorage(this.articleId);
            if (anzahl > 0)
            {
                string msg = string.Format("Dieser Artikel kann nicht gel�scht werden, da noch ein Bestand von {0} im Lager vorhanden ist.", anzahl);

                var builder1 = new AlertDialog.Builder(this);
                builder1.SetMessage(msg);
                builder1.SetPositiveButton("OK", (s, e) => { });
                builder1.Create().Show();

                return;
            }

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (isInList)
            {
                string msg = string.Format("Dieser Artikel kann nicht gel�scht werden, da er sich noch auf der Einkaufsliste befinden.");

                var builder1 = new AlertDialog.Builder(this);
                builder1.SetMessage(msg);
                builder1.SetPositiveButton("OK", (s, e) => { });
                builder1.Create().Show();

                return;
            }

            var builder = new AlertDialog.Builder(this);
            builder.SetMessage("Soll dieser Artikel wirklich gel�scht werden?");
            builder.SetNegativeButton("Nein", (s, e) => { });
            builder.SetPositiveButton("Ja", (s, e) => 
            { 
                SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
                if (this.article.ArticleId > 0)
                {
                    databaseConnection.Delete(this.article);
                    this.SetResult(Result.Ok);

                    this.OnBackPressed();
                }

            });
            builder.Create().Show();

        }

        private void AddToShoppingListAutomatically()
        {
            int? minQuantity  = GetIntegerFromEditText(Resource.Id.ArticleDetails_MinQuantity);
            int? prefQuantity = GetIntegerFromEditText(Resource.Id.ArticleDetails_PrefQuantity);

            if (minQuantity  == null) minQuantity  = 0;
            if (prefQuantity == null) prefQuantity = 0;

            int toBuyQuantity = Database.GetToShoppingListQuantity(this.articleId, minQuantity.Value, prefQuantity.Value);
            if (toBuyQuantity == 0)
                toBuyQuantity = 1;

            double count = Database.AddToShoppingList(this.articleId, toBuyQuantity);

            string msg = string.Format("{0} St�ck auf der Einkaufsliste.", count);
            if (this.toast != null)
            {
                this.toast.Cancel();
                this.toast = Toast.MakeText(this, msg, ToastLength.Short);
            }
            else
            {
                this.toast = Toast.MakeText(this, msg, ToastLength.Short);
            }

            this.toast.Show();
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
                alert.SetPositiveButton("OK", (sender, e) =>
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

        private void ShowPictureAndDetails(int articleId, string eanCode)
        {
            this.article = Database.GetArticle(articleId);
            if (this.article == null)
                this.article = new Article();

            FindViewById<TextView>(Resource.Id.ArticleDetails_ArticleId).Text       = string.Format("ArticleId: {0}", article.ArticleId);

            FindViewById<EditText>(Resource.Id.ArticleDetails_Name).Text               = article.Name;
            FindViewById<Switch>  (Resource.Id.ArticleDetails_DurableInfinity).Checked = article.DurableInfinity;

            if (article.WarnInDays.HasValue) FindViewById<EditText>(Resource.Id.ArticleDetails_WarnInDays).Text = article.WarnInDays.Value.ToString();
            if (article.Calorie.HasValue) FindViewById<EditText>(Resource.Id.ArticleDetails_Calorie).Text       = article.Calorie.Value.ToString();
            if (article.Size.HasValue)    FindViewById<EditText>(Resource.Id.ArticleDetails_Size).Text          = article.Size.Value.ToString(CultureInfo.InvariantCulture);
            if (article.MinQuantity.HasValue)  FindViewById<EditText>(Resource.Id.ArticleDetails_MinQuantity).Text  = article.MinQuantity.Value.ToString();
            if (article.PrefQuantity.HasValue) FindViewById<EditText>(Resource.Id.ArticleDetails_PrefQuantity).Text = article.PrefQuantity.Value.ToString();

            FindViewById<EditText>(Resource.Id.ArticleDetails_Unit).Text               = article.Unit;
            FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text            = article.EANCode;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Notes).Text              = article.Notes;

            this.warningInDaysView.Enabled      = !article.DurableInfinity;
            this.warningInDaysLabelView.Enabled = !article.DurableInfinity;


            // Hersteller
            this.Manufacturers = Database.GetManufacturerNames();

            var manufacturer = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Manufacturer);
            var manufacturerAdapter = (ArrayAdapter<String>)(manufacturer.Adapter);
            manufacturerAdapter.Clear();
            manufacturerAdapter.AddAll(this.Manufacturers.ToList<string>());
            manufacturerAdapter.NotifyDataSetChanged();


            Spinner categorySpinner = FindViewById<Spinner>(Resource.Id.ArticleDetails_Category);

            var categoryAdapter = (ArrayAdapter<String>)(categorySpinner.Adapter);
            
            int position = categoryAdapter.GetPosition(article.Category);
            categorySpinner.SetSelection(position);

            this.SubCategories = Database.GetSubcategoriesOf();

            var subCategory = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);
            var subCategoryAdapter = (ArrayAdapter<String>)(subCategory.Adapter);
            subCategoryAdapter.Clear();
            subCategoryAdapter.AddAll(this.SubCategories.ToList<string>());
            subCategoryAdapter.NotifyDataSetChanged();


            this.Storages = Database.GetStorageNames();

            var storage = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);
            var storageAdapter = (ArrayAdapter<String>)(storage.Adapter);
            storageAdapter.Clear();
            storageAdapter.AddAll(this.Storages.ToList<string>());
            storageAdapter.NotifyDataSetChanged();


            this.Supermarkets = Database.GetSupermarketNames();

            var supermarket = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);
            var supermarketAdapter = (ArrayAdapter<String>)(supermarket.Adapter);
            supermarketAdapter.Clear();
            supermarketAdapter.AddAll(this.Supermarkets.ToList<string>());
            supermarketAdapter.NotifyDataSetChanged();

            FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text = article.Manufacturer;
            FindViewById<EditText>(Resource.Id.ArticleDetails_SubCategory).Text  = article.SubCategory;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Supermarket).Text  = article.Supermarket;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Storage).Text      = article.StorageName;

            if (article.Image != null)
            {
                string text = string.Empty;
                this.imageView2.Visibility = ViewStates.Gone;
                try
                {
                    Bitmap smallBitmap = BitmapFactory.DecodeByteArray(article.Image,      0, article.Image.Length);
                
                    this.imageView.SetImageBitmap(smallBitmap);

                    text += string.Format("Voransicht: {0:n0} X {1:n0}\n", smallBitmap.Height, smallBitmap.Width);
                    text += string.Format("Gr��e: {0:n0}\n", Tools.ToFuzzyByteString(smallBitmap.ByteCount));
                    text += string.Format("Komprimiert: {0:n0}\n", Tools.ToFuzzyByteString(article.Image.Length));
                }
                catch(Exception ex)
                {
                    text += "\n" + ex.Message;

                    this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                    this.imageView.Enabled = false;
                }

                this.imageTextView.Text = text;
            }
            else
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);

            if (!string.IsNullOrEmpty(eanCode))
            {
                FindViewById<EditText>(Resource.Id.ArticleDetails_EANCode).Text = eanCode;
            }
        }

        private void ResizeBitmap(Bitmap newBitmap)
        {
            MemoryStream stream;
            byte[] resizedImage;

            string text = string.Empty;

            try
            {
                int width1  = newBitmap.Width;
                int height1 = newBitmap.Height;

                text += string.Format("Org.: {0:n0} x {1:n0} ({2:n0})\r\n",  newBitmap.Height,  newBitmap.Width, Tools.ToFuzzyByteString(newBitmap.ByteCount));

                resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, 480*1, 854*1);

                Bitmap largeBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

                stream = new MemoryStream();
                largeBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                ArticleDetailsActivity.imageLarge = stream.ToArray();

                text += string.Format("Bild: {0:n0} x {1:n0} ({2:n0}, {3:n0})\r\n", largeBitmap.Height, largeBitmap.Width, Tools.ToFuzzyByteString(largeBitmap.ByteCount), Tools.ToFuzzyByteString(ArticleDetailsActivity.imageLarge.Length));

                resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, 48*2, 85*2);

                Bitmap smallBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

                stream = new MemoryStream();
                smallBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
                ArticleDetailsActivity.imageSmall = stream.ToArray();

                text += string.Format("Thn.: {0:n0} x {1:n0} ({2:n0}, {3:n0})", smallBitmap.Height, smallBitmap.Width, Tools.ToFuzzyByteString(smallBitmap.ByteCount), Tools.ToFuzzyByteString(ArticleDetailsActivity.imageSmall.Length));

                RunOnUiThread(() => this.imageView.SetImageBitmap(smallBitmap));
                RunOnUiThread(() => this.imageView2.Visibility = ViewStates.Gone);
                RunOnUiThread(() => this.imageTextView.Text = text);

                TRACE("Bild original : W={0:n0}, H={1:n0}", newBitmap.Width, newBitmap.Height);
                TRACE("Bild original : Size={0}", Tools.ToFuzzyByteString(newBitmap.ByteCount));
                TRACE("Bild small    : W={0:n0}, H={1:n0}", smallBitmap.Width, smallBitmap.Height);
                TRACE("Bild small    : Size={0}", Tools.ToFuzzyByteString(smallBitmap.ByteCount));
                TRACE("Image size    : Size={0}", Tools.ToFuzzyByteString(ArticleDetailsActivity.imageLarge.Length));
            }
            catch(Exception ex)
            {
                RunOnUiThread(() => this.imageTextView.Text = ex.Message);
            }
        
        }


        private void LoadAndResizeBitmap(string fileName)
        {
            Bitmap bitmap = BitmapFactory.DecodeFile(fileName);

            this.ResizeBitmap(bitmap);

            File.Delete(fileName);
        }

        private int? GetIntegerFromEditText(int resourceId)
        {
            int? value = null;

            string valueText  = FindViewById<EditText>(resourceId).Text;

            try
            {
                if (!string.IsNullOrEmpty(valueText))
                {
                    value = Convert.ToInt32(valueText, CultureInfo.InvariantCulture);
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
        public string Value { get; private set; }

        public void OnItemSelected(AdapterView parent, View view, int position, long id)
        {
            TextView textView = view as TextView;
            if (textView == null) return;

            this.Value = textView.Text;
        }

        public void OnNothingSelected(AdapterView parent)
        {
            this.Value = null;
        }
    }

}