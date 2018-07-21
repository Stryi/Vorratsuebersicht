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
    [Activity(Label = "@string/Main_Button_Artikelangaben", Icon = "@drawable/ic_local_offer_white_48dp")]
    public class ArticleDetailsActivity : Activity
    {
        Article article;
        int articleId;
        bool isChanged = false;
        byte[] imageLarge;
        byte[] imageSmall;
        ImageView imageView;
        ImageView imageView2;
        TextView  imageTextView;
        EditText  warningInDaysView;
        TextView  warningInDaysLabelView;
        Toast toast;
        ImageCaptureHelper imageCapture;

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

        protected override void OnCreate(Bundle savedInstanceState)
        {
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

            this.catalogListener = new CatalogItemSelectedListener();

            string[] Categories = Resources.GetStringArray(Resource.Array.ArticleCatagories);

            ArrayAdapter<String> categoryAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, Categories);
            categoryAdapter.SetDropDownViewResource (Android.Resource.Layout.SimpleSpinnerDropDownItem);

            Spinner categorySpinner = (Spinner)FindViewById<Spinner>(Resource.Id.ArticleDetails_Category);
            categorySpinner.Adapter = categoryAdapter;
            categorySpinner.OnItemSelectedListener = this.catalogListener;
            
            this.SubCategories = new List<string>();

            var subCategory = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_SubCategory);

            ArrayAdapter<String> subCategoryAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, SubCategories);
            subCategory.Adapter = subCategoryAdapter;
            subCategory.Threshold = 1;
            subCategory.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            subCategory.SetTokenizer(new SpaceTokenizer());

            this.Storages = new List<string>();

            var storage = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Storage);

            ArrayAdapter<String> storageAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, Storages);
            storage.Adapter = storageAdapter;
            storage.Threshold = 1;
            storage.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            storage.SetTokenizer(new SpaceTokenizer());

            this.Supermarkets = new List<string>();

            var supermarket = FindViewById<MultiAutoCompleteTextView>(Resource.Id.ArticleDetails_Supermarket);

            ArrayAdapter<String> supermarketAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Supermarkets);
            supermarket.Adapter = supermarketAdapter;
            supermarket.Threshold = 1;
            supermarket.SetTokenizer(new MultiAutoCompleteTextView.CommaTokenizer());
            supermarket.SetTokenizer(new SpaceTokenizer());


            this.ShowPictureAndDetails(this.articleId, eanCode);

            imageView.Click += delegate
            {
                if (this.article.Image == null)
                {
                    this.TakeAPhoto();
                    return;
                };


                var articleImage = new Intent (this, typeof(ArticleImageActivity));
                articleImage.PutExtra("ArticleId", this.articleId);
                articleImage.PutExtra("Large", true);
                StartActivity (articleImage);
            };

            imageView2.Click += delegate
            {
                this.SelectAPicture();
            };

            this.Window.SetSoftInputMode(SoftInput.StateHidden);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ArticleDetails_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            IMenuItem itemDelete = menu.FindItem(Resource.Id.ArticleDetails_Delete);
            itemDelete.SetVisible(this.article?.ArticleId > 0);

            IMenuItem itemAddPhoto = menu.FindItem(Resource.Id.ArticleDetails_MakeAPhoto);
            itemAddPhoto.SetVisible(this.cameraExists);

            IMenuItem itemSpeech = menu.FindItem(Resource.Id.ArticleDetails_Speech);
            itemSpeech.SetEnabled(this.IsThereAnSpeechAvailable());

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch(item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetails_Delete:
                    this.DeleteArticle();
                    return true;

                case Resource.Id.ArticleDetails_Save:
                    this.isChanged = true;

                    if (!this.SaveArticle())
                    {
                        return false;
                    }

                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetails_Cancel:
                    this.MoveTaskToBack(false);
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleDetails_MakeAPhoto:
                    this.TakeAPhoto();
                    return true;

                case Resource.Id.ArticleDetails_SelectAPicture:
                    this.SelectAPicture();
                    return true;

                case Resource.Id.ArticleDetails_ScanEAN:
                    this.ScanEAN();
                    return true;

                case Resource.Id.ArticleDetails_ToShoppingList:
                    this.AddToShoppingListAutomatically();
                    return true;

                case Resource.Id.ArticleDetails_Speech:
                    this.SprachEingabe();
                    return true;

            }

            return false;
        }

        private async void ScanEAN()
        {
            var scanner = new ZXing.Mobile.MobileBarcodeScanner();
            var scanResult = await scanner.Scan();

            if (scanResult == null)
                return;

            System.Diagnostics.Trace.WriteLine("Scanned Barcode: " + scanResult.Text);
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
                var progressDialog = ProgressDialog.Show(this, "Bitte warten...", "Bild wird komprimiert...", true);
                new Thread(new ThreadStart(delegate             
                {
                    Android.Net.Uri uri = data.Data;
                    Bitmap newBitmap = MediaStore.Images.Media.GetBitmap( this.ContentResolver, uri);

                    this.ResizeBitmap(newBitmap);

                    // Dispose of the Java side bitmap.
                    GC.Collect();

                    RunOnUiThread(() => progressDialog.Hide());

                })).Start();
            }

            if ((requestCode == TakePhotoId))
            {
                var progressDialog = ProgressDialog.Show(this, "Bitte warten...", "Bild wird komprimiert...", true);
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

                    RunOnUiThread(() => progressDialog.Hide());

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
            this.imageCapture.TakePicture();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            this.imageCapture.RequestPermissions(requestCode, grantResults);
        }

        private bool SaveArticle()
        {
            string warnInDaysText = FindViewById<EditText>(Resource.Id.ArticleDetails_WarnInDays).Text;
            string sizeText    = FindViewById<EditText>(Resource.Id.ArticleDetails_Size).Text;
            string calorieText = FindViewById<EditText>(Resource.Id.ArticleDetails_Calorie).Text;
            string minQuantityText  = FindViewById<EditText>(Resource.Id.ArticleDetails_MinQuantity).Text;
            string prefQuantityText = FindViewById<EditText>(Resource.Id.ArticleDetails_PrefQuantity).Text;

            int?     warnInDays = null;
            int?     calorie = null;
            decimal? size = null;
            int?     minQuantity = null;
            int?     prefQuantity = null;

            try
            {
                if (!string.IsNullOrEmpty(warnInDaysText))
                {
                    warnInDays = Convert.ToInt32(warnInDaysText, CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(sizeText))
                {
                    size = Convert.ToDecimal(sizeText, CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(calorieText))
                {
                    calorie = Convert.ToInt32(calorieText, CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(minQuantityText))
                {
                    minQuantity = Convert.ToInt32(minQuantityText, CultureInfo.InvariantCulture);
                }
                if (!string.IsNullOrEmpty(prefQuantityText))
                {
                    prefQuantity = Convert.ToInt32(prefQuantityText, CultureInfo.InvariantCulture);
                }
            }
            catch(Exception ex)
            {
                string fehlerText = "Fehler beim Konvertieren von '{0}', '{1}' oder '{2}' in eine Zahl.";
                fehlerText = string.Format(fehlerText, warnInDaysText, sizeText, calorieText);

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
                        emailIntent.PutExtra(Android.Content.Intent.ExtraSubject, "Fehlerbericht: Vorratsübersicht");
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

            if (this.imageLarge != null)
                this.article.ImageLarge = this.imageLarge;

            if (this.imageSmall != null)
                this.article.Image      = this.imageSmall;

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
            }

            return true;
        }

        private void DeleteArticle()
        {
            // Prüfen, ob noch Bestand an dem Artikel vorhanden ist
            decimal anzahl = Database.GetArticleQuantityInStorage(this.articleId);
            if (anzahl > 0)
            {
                string msg = string.Format("Dieser Artikel kann nicht gelöscht werden, da noch ein Bestand von {0} im Lager vorhanden ist.", anzahl);

                var builder1 = new AlertDialog.Builder(this);
                builder1.SetMessage(msg);
                builder1.SetPositiveButton("OK", (s, e) => { });
                builder1.Create().Show();

                return;
            }

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (isInList)
            {
                string msg = string.Format("Dieser Artikel kann nicht gelöscht werden, da er sich noch auf der Einkaufsliste befinden.");

                var builder1 = new AlertDialog.Builder(this);
                builder1.SetMessage(msg);
                builder1.SetPositiveButton("OK", (s, e) => { });
                builder1.Create().Show();

                return;
            }

            var builder = new AlertDialog.Builder(this);
            builder.SetMessage("Soll dieser Artikel wirklich gelöscht werden?");
            builder.SetNegativeButton("Nein", (s, e) => 
            {
                this.OnBackPressed();

            });
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
            int toBuyQuantity = Database.GetToShoppingListQuantity(this.articleId);
            if (toBuyQuantity == 0)
                toBuyQuantity = 1;

            double count = Database.AddToShoppingList(this.articleId, toBuyQuantity);

            string msg = string.Format("{0} Stück auf der Einkaufsliste.", count);
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
            FindViewById<EditText>(Resource.Id.ArticleDetails_Manufacturer).Text       = article.Manufacturer;
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
            var supermarketAdapter = (ArrayAdapter<String>)(storage.Adapter);
            supermarketAdapter.Clear();
            supermarketAdapter.AddAll(this.Storages.ToList<string>());
            supermarketAdapter.NotifyDataSetChanged();

            FindViewById<EditText>(Resource.Id.ArticleDetails_SubCategory).Text = article.SubCategory;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Supermarket).Text = article.Supermarket;
            FindViewById<EditText>(Resource.Id.ArticleDetails_Storage).Text     = article.StorageName;

            if (article.Image != null)
            {
                Bitmap largeBitmap = BitmapFactory.DecodeByteArray(article.ImageLarge, 0, article.ImageLarge.Length);
                Bitmap smallBitmap = BitmapFactory.DecodeByteArray(article.Image,      0, article.Image.Length);
                
                this.imageView.SetImageBitmap(smallBitmap);
                this.imageView2.Visibility = ViewStates.Gone;

                string text = string.Empty;

                text += string.Format("Thn.: {0:n0} X {1:n0} ({2:n0})", smallBitmap.Height, smallBitmap.Width, Tools.ToFuzzyByteString(smallBitmap.ByteCount));
                text += "\n";
                text += string.Format("Bild: {0:n0} X {1:n0} ({2:n0})", largeBitmap.Height, largeBitmap.Width, Tools.ToFuzzyByteString(largeBitmap.ByteCount));

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
            string text = string.Empty;

            int width1  = newBitmap.Width;
            int height1 = newBitmap.Height;

            text += string.Format("Org.: {0:n0} x {1:n0} ({2:n0})\r\n",  newBitmap.Height,  newBitmap.Width, Tools.ToFuzzyByteString(newBitmap.ByteCount));

            byte[] resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, 480*1, 854*1);

            Bitmap largeBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

            int width2  = largeBitmap.Width;
            int height2 = largeBitmap.Height;

            text += string.Format("Bild: {0:n0} x {1:n0} ({2:n0})\r\n", largeBitmap.Height, largeBitmap.Width, Tools.ToFuzzyByteString(largeBitmap.ByteCount));

            MemoryStream stream = new MemoryStream();
            largeBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            this.imageLarge = stream.ToArray();



            resizedImage = ImageResizer.ResizeImageAndroid(newBitmap, 48, 85);

            Bitmap smallBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

            int width3  = smallBitmap.Width;
            int height3 = smallBitmap.Height;

            text += string.Format("Thn.: {0:n0} x {1:n0} ({2:n0})", smallBitmap.Height, smallBitmap.Width, Tools.ToFuzzyByteString(smallBitmap.ByteCount));

            stream = new MemoryStream();
            smallBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            this.imageSmall = stream.ToArray();

            RunOnUiThread(() => this.imageView.SetImageBitmap(smallBitmap));
            RunOnUiThread(() => this.imageView2.Visibility = ViewStates.Gone);
            RunOnUiThread(() => this.imageTextView.Text = text);

            System.Diagnostics.Trace.WriteLine(string.Format("Bild original : W={0:n0}, H={1:n0}", newBitmap.Width, newBitmap.Height));
            System.Diagnostics.Trace.WriteLine(string.Format("Bild original : Size={0}", Tools.ToFuzzyByteString(newBitmap.ByteCount)));
            System.Diagnostics.Trace.WriteLine(string.Format("Bild small    : W={0:n0}, H={1:n0}", smallBitmap.Width, smallBitmap.Height));
            System.Diagnostics.Trace.WriteLine(string.Format("Bild small    : Size={0}", Tools.ToFuzzyByteString(smallBitmap.ByteCount)));
            System.Diagnostics.Trace.WriteLine(string.Format("Image size    : Size={0}", Tools.ToFuzzyByteString(this.imageLarge.Length)));
        }


        private void LoadAndResizeBitmap(string fileName)
        {
            int height = 854;
            int width  = 480;

            // First we get the the dimensions of the file on disk
            BitmapFactory.Options options = new BitmapFactory.Options { InJustDecodeBounds = true };
            BitmapFactory.DecodeFile(fileName, options);

            FileInfo fileInfo = new FileInfo (fileName);
            Console.WriteLine (fileInfo.Length);
            string text = string.Empty;
            text += string.Format("Org.: {0:n0} x {1:n0} ({2:n0})\r\n", options.OutHeight, options.OutWidth, Tools.ToFuzzyByteString(fileInfo.Length));

            // Next we calculate the ratio that we need to resize the image by
            // in order to fit the requested dimensions.
            int outHeight = options.OutHeight;
            int outWidth  = options.OutWidth;
            int inSampleSize = 1;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }


            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap resizedBitmap = BitmapFactory.DecodeFile(fileName, options);

            text += string.Format("Bild: {0:n0} x {1:n0} ({2:n0})\r\n", resizedBitmap.Height, resizedBitmap.Width, Tools.ToFuzzyByteString(resizedBitmap.ByteCount));

            height = 48;
            width  = 85;

            if (outHeight > height || outWidth > width)
            {
                inSampleSize = outWidth > outHeight
                                   ? outHeight / height
                                   : outWidth / width;
            }

            // Now we will load the image and have BitmapFactory resize it for us.
            options.InSampleSize = inSampleSize;
            options.InJustDecodeBounds = false;
            Bitmap smallBitmap = BitmapFactory.DecodeFile(fileName, options);

            text += string.Format("Thn.: {0:n0} x {1:n0} ({2:n0})\r\n", smallBitmap.Height, smallBitmap.Width, Tools.ToFuzzyByteString(smallBitmap.ByteCount));

            RunOnUiThread(() => this.imageView.SetImageBitmap(resizedBitmap));
            RunOnUiThread(() => this.imageView2.Visibility = ViewStates.Gone);
            RunOnUiThread(() => this.imageTextView.Text = text);

            MemoryStream stream = new MemoryStream();
            resizedBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            this.imageLarge = stream.ToArray();

            stream = new MemoryStream();
            smallBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            this.imageSmall = stream.ToArray();
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