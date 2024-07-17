using System;
using System.Globalization;
using System.Collections.Generic;

using Android.App;
using Android.Text;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Support.V7.App;

using static VorratsUebersicht.Tools;

namespace VorratsUebersicht
{
    using static VorratsUebersicht.StorageItemQuantityListViewAdapter;
    using AlertDialog = Android.Support.V7.App.AlertDialog;

    [Activity(Label = "@string/StorageItemQuantity", Theme = "@style/Theme.AppCompat", Icon = "@drawable/ic_assignment_white_48dp", ScreenOrientation = ScreenOrientation.Portrait)]
    public class StorageItemQuantityActivity : AppCompatActivity
    {
        public static readonly int ArticleDetailId = 1002;

        public static List<StorageItemQuantityListView> liste = null;
        public static Article article = null;
        public static ArticleImage articleImage = null;
        public static bool UseAltDatePicker;

        int articleId;
        string text;
        bool durableInfinity = false;
        bool isChanged = false;
        bool isEditMode = false;
        bool noArticleDetails = false;
        double quantity = -1;
        List<string> Storages;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StorageItemQuantity);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            this.SupportActionBar.SetBackgroundDrawable(backgroundPaint);
            this.SupportActionBar.SetDisplayHomeAsUpEnabled(true);

            this.text             = Intent.GetStringExtra ("Heading") ?? string.Empty;
            this.articleId        = Intent.GetIntExtra    ("ArticleId", 0);
            bool editMode         = Intent.GetBooleanExtra("EditMode", false);
            this.noArticleDetails = Intent.GetBooleanExtra("NoArticleDetails", false);
            this.quantity         = Intent.GetDoubleExtra ("Quantity", -1);

            if (System.Diagnostics.Debugger.IsAttached)
            {
                if (this.articleId == 0)
                {
                    this.text = "Was auch immer";
                    this.articleId = 4;
                }
            }

            this.ShowPictureAndDetails(this.articleId, this.text);
            this.ShowStorageListForArticle(this.articleId);

            // Lagerort Eingabe
            this.Storages = Database.GetStorageNames();

            var storage = FindViewById<AutoCompleteTextView>(Resource.Id.StorageItemQuantity_StorageText);
            storage.Text = StorageItemQuantityActivity.article?.StorageName;

            ArrayAdapter<String> storageAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleDropDownItem1Line, this.Storages);
            storage.Adapter = storageAdapter;
            storage.Threshold = 1;

            ImageView image = FindViewById<ImageView>(Resource.Id.StorageItemQuantity_Image);
            image.Click += delegate 
            {
                if (StorageItemQuantityActivity.articleImage?.ImageSmall == null)
                   return;

                var articleImage = new Intent (this, typeof(ArticleImageActivity));
                articleImage.PutExtra("ArticleId", articleId);
                this.StartActivity(articleImage);
            };

            TextView articleText = FindViewById<TextView>(Resource.Id.StorageItemQuantity_ArticleDetail);
            articleText.Click += delegate
            {
                this.GotoArticleDetails();
            };

            Button selectStorage = FindViewById<Button>(Resource.Id.StorageItemQuantity_SelectStorage);
            selectStorage.Click += SelectStorage_Click;

            Button stepButton = FindViewById<Button>(Resource.Id.StorageItemQuantity_StepButton);
            stepButton.Click += StepButton_Click;

            ImageButton addArticle = FindViewById<ImageButton>(Resource.Id.StorageItemQuantity_AddArticle);
            addArticle.Click += AddArticle_Click;

            ImageButton addButton = FindViewById<ImageButton>(Resource.Id.StorageItemQuantity_AddPosition);
            addButton.Click += AddArticle_Click;

            if (editMode)
            {
                this.SetEditMode(true);
            }

            if (this.quantity >= 0)
            {
                this.AddArticleWithQuantity();
            }

            StorageItemQuantityListViewAdapter.StepValue = 1;
        }

        private void AddArticleWithQuantity()
        {
            var storageName = FindViewById<EditText>(Resource.Id.StorageItemQuantity_StorageText).Text;

            StorageItemQuantityResult storageItemQuantity = new StorageItemQuantityResult()
            {
			    ArticleId    = this.articleId,
                Quantity     = (decimal)this.quantity,
                BestBefore   = DateTime.Today,
                StorageName  = storageName,
                IsChanged    = true
            };
            
            StorageItemQuantityListView itemView = new StorageItemQuantityListView(storageItemQuantity, this.Resources);

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            var adapter = (StorageItemQuantityListViewAdapter)listView.Adapter;
            adapter.Add(itemView);
            listView.InvalidateViews();

            if (this.durableInfinity)
            {
                storageItemQuantity.BestBefore = null;

                if (this.quantity > 0)
                {
                    this.ChangeQuantity(storageItemQuantity, adapter);
                }
                return;
            }

            this.ChangeBestBeforeDate(storageItemQuantity, adapter);
        }

        private void AddArticle_Click(object sender, EventArgs e)
        {
            var storageName = FindViewById<EditText>(Resource.Id.StorageItemQuantity_StorageText).Text;

            StorageItemQuantityResult storageItemQuantity = new StorageItemQuantityResult()
            {
			    ArticleId    = this.articleId,
                Quantity     = 1,
                BestBefore   = DateTime.Today,
                StorageName  = storageName,
                IsChanged    = true,
            };

            if (StorageItemQuantityActivity.article.WarnInDays.HasValue)
            {
                storageItemQuantity.WarnInDays = StorageItemQuantityActivity.article.WarnInDays.Value;
            }

            if (this.quantity > 1)
            {
                storageItemQuantity.Quantity = (decimal)this.quantity;
            }

            StorageItemQuantityListView itemView = new StorageItemQuantityListView(storageItemQuantity, this.Resources);

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            var adapter = (StorageItemQuantityListViewAdapter)listView.Adapter;
            adapter.Add(itemView);
            listView.InvalidateViews();

            if (!this.durableInfinity)
            {
                // Haltbarkeitsdatum erfassen (kann aber auch weggelassen werden)
                listView.InvalidateViews();
                if (!UseAltDatePicker)
                {
                    DatePickerFragment frag = DatePickerFragment.NewInstance(delegate (DateTime? time)
                        {
                            if (time.HasValue)
                                storageItemQuantity.BestBefore = time.Value;
                            else
                                storageItemQuantity.BestBefore = null;

                            listView.InvalidateViews();

                            if (this.quantity > 0)
                            {
                                this.ChangeQuantity(storageItemQuantity, adapter);
                            }

                        }, DateTime.Today);
                    frag.ShowsDialog = true;
                    frag.Show(this.SupportFragmentManager, DatePickerFragment.TAG);
                } else
                {
                    AltDatePickerFragment frag = AltDatePickerFragment.NewInstance(delegate (DateTime? time)
                    {
                        if (time.HasValue)
                            storageItemQuantity.BestBefore = time.Value;
                        else
                            storageItemQuantity.BestBefore = null;

                        listView.InvalidateViews();

                        if (this.quantity > 0)
                        {
                            this.ChangeQuantity(storageItemQuantity, adapter);
                        }

                    }, DateTime.Today);
                    frag.ShowsDialog = true;
                    frag.Show(this.SupportFragmentManager, AltDatePickerFragment.TAG);

                }
            }
            else
            {
                // Ist ohne Haltbarkeitsdatum (unendlich haltbar)
                // Datum muss nicht erfasst werden.
                storageItemQuantity.BestBefore = null;
                listView.InvalidateViews();
            }
        }

        private void StepButton_Click(object sender, EventArgs e)
        {
            switch (StorageItemQuantityListViewAdapter.StepValue)
            {
                case 0.01m: StorageItemQuantityListViewAdapter.StepValue = 0.10m; break;
                case 0.10m: StorageItemQuantityListViewAdapter.StepValue = 0.25m; break;
                case 0.25m: StorageItemQuantityListViewAdapter.StepValue = 0.50m; break;
                case 0.50m: StorageItemQuantityListViewAdapter.StepValue =    1m; break;
                case    1m: StorageItemQuantityListViewAdapter.StepValue =   10m; break;
                case   10m: StorageItemQuantityListViewAdapter.StepValue =  100m; break;
                case  100m: StorageItemQuantityListViewAdapter.StepValue = 0.01m; break;
                
                default: break;
            }

            TextView text = FindViewById<TextView>(Resource.Id.StorageItemQuantity_StepText);
            text.Text = string.Format("{0}x", StorageItemQuantityListViewAdapter.StepValue);
        }

        private void SelectStorage_Click(object sender, EventArgs e)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(this.Resources.GetString(Resource.String.StorageItemQuantityList_StorageForNew));
            builder.SetItems(this.Storages.ToArray(), (s, a) =>
            {
                var textView = FindViewById<AutoCompleteTextView>(Resource.Id.StorageItemQuantity_StorageText);
                textView.Text = this.Storages[a.Which];
            });
            builder.Show();
        }

        public static void Reload()
        {
            StorageItemQuantityActivity.liste = null;
            StorageItemQuantityActivity.article = null;
            StorageItemQuantityActivity.articleImage = null;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StorageItemQuantity_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.FindItem(Resource.Id.StorageItemQuantity_Menu_Edit).SetVisible(!this.isEditMode);
            menu.FindItem(Resource.Id.StorageItemQuantity_Cancel).SetVisible(this.isEditMode);
            menu.FindItem(Resource.Id.StorageItemQuantity_Save).SetVisible(this.isEditMode);

            if (StorageItemQuantityActivity.articleImage?.ImageSmall == null)
                menu.FindItem(Resource.Id.StorageItemQuantity_Menu_EditPicture).SetVisible(false);

            menu.FindItem(Resource.Id.StorageItemQuantity_ToArticleDetails).SetVisible(!this.noArticleDetails);
            
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;

                case Resource.Id.StorageItemQuantity_Menu_Edit:
					this.SetEditMode(true);

                    return true;

                case Resource.Id.StorageItemQuantity_Save:
					this.isChanged = true;
                    bool ok = this.SaveChanges();
                    if (!ok)
                    {
                        return false;
                    }

					this.SetEditMode(false);
                    this.quantity = -1;
                    this.AddToShoppingList();
                    break;

                case Resource.Id.StorageItemQuantity_Cancel:
                    this.ShowStorageListForArticle(this.articleId);
					this.SetEditMode(false);
                    this.quantity = -1;
                    break;

                case Resource.Id.StorageItemQuantity_ToShoppingList:
                    this.AddToShoppingListManually();
                    return true;

                case Resource.Id.StorageItemQuantity_ToArticleDetails:
                    this.GotoArticleDetails();
                    return true;

                case Resource.Id.StorageItemQuantity_Menu_EditPicture:
                    var articleImage = new Intent (this, typeof(ArticleImageActivity));
                    articleImage.PutExtra("Heading", text);
                    articleImage.PutExtra("ArticleId", articleId);
                    this.StartActivity(articleImage);
                    break;
            }
            return true;
        }

        private void SetEditMode(bool editMode)
		{
            this.isEditMode = editMode;

            this.InvalidateOptionsMenu();

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            StorageItemQuantityListViewAdapter adapter = listView.Adapter as StorageItemQuantityListViewAdapter;

			if (editMode)
            {
                FindViewById(Resource.Id.StorageItemQuantity_Storage).Visibility = ViewStates.Visible;
                FindViewById(Resource.Id.StorageItemQuantity_Step).Visibility = ViewStates.Visible;
                FindViewById(Resource.Id.StorageItemQuantity_AddPosition).Visibility = ViewStates.Visible;
                adapter.ActivateButtons();
                this.Window.SetSoftInputMode(SoftInput.StateHidden);
            }
            else
            {
                FindViewById(Resource.Id.StorageItemQuantity_Storage).Visibility = ViewStates.Gone;
                FindViewById(Resource.Id.StorageItemQuantity_Step).Visibility = ViewStates.Gone;
                FindViewById(Resource.Id.StorageItemQuantity_AddPosition).Visibility = ViewStates.Gone;
                adapter.DeactivateButtons();
            }

            listView.InvalidateViews();
		}

        private void AddToShoppingListManually()
        {
            int? minQuantity  = StorageItemQuantityActivity.article?.MinQuantity;
            int? prefQuantity = StorageItemQuantityActivity.article?.PrefQuantity;

            AddToShoppingListDialog.ShowDialog(this, articleId, minQuantity, prefQuantity);
        }

        private void AddToShoppingList()
        {
            int toBuy = Database.GetToShoppingListQuantity(this.articleId);
            if (toBuy == 0)
                return;
            
            this.AddToShoppingListManually();
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == ArticleDetailId) && (resultCode == Result.Ok))
            {
                StorageItemQuantityActivity.article = null;
                StorageItemQuantityActivity.articleImage = null;
                this.ShowPictureAndDetails(this.articleId, this.text);
                this.isChanged = true;
            }

        }

        public override void Finish()
        {
            if (this.isChanged)
            {
                Intent intent = new Intent();
				intent.PutExtra("ArticleId", this.articleId);
                this.SetResult(Result.Ok, intent);
            }

            base.Finish();

            StorageItemQuantityActivity.Reload();
        }

        private bool SaveChanges()
        {
            try
            {
                foreach(StorageItemQuantityListView item in StorageItemQuantityActivity.liste)
                {
                    if ((item.StorageItem.IsChanged))
                    {
                        Database.UpdateStorageItemQuantity(item.StorageItem);
                        item.StorageItem.IsChanged = false;
                    }
                }
            }
            catch(Exception ex)
            {
                TRACE(ex);

                var messageBox = new AlertDialog.Builder(this);
                messageBox.SetTitle(this.Resources.GetString(Resource.String.App_ErrorOccurred));
                messageBox.SetMessage(ex.Message);
                messageBox.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                messageBox.Create().Show();

                return false;
            }

            return true;
        }

        private void ShowPictureAndDetails(int articleId, string title)
        {
            var headerView = FindViewById<TextView>  (Resource.Id.StorageItemQuantity_ArticleDetailHeader);
            var imageView  = FindViewById<ImageView> (Resource.Id.StorageItemQuantity_Image);
            var detailView = FindViewById<TextView>  (Resource.Id.StorageItemQuantity_ArticleDetail);

            if (articleId == 0)
            {
                headerView.Text = title;
                detailView.Text = "Details zum " + title;
                imageView.SetImageResource(Resource.Drawable.ic_add_a_photo_white_24dp);

                return;
            }
            try
            {
                if (StorageItemQuantityActivity.article == null)
                {
                    StorageItemQuantityActivity.article = Database.GetArticle(articleId);
                }

                if (StorageItemQuantityActivity.articleImage == null)
                {
                    StorageItemQuantityActivity.articleImage = Database.GetArticleImage(articleId, false);
                }
                
                Article article = StorageItemQuantityActivity.article;

                this.durableInfinity = article.DurableInfinity;

                ArticleListView articleView = new ArticleListView(article, this.Resources);

                headerView.Text = articleView.Heading;
                detailView.Text = articleView.SubHeading;
                if (!string.IsNullOrEmpty(articleView.Notes))
                {
                    detailView.Text += "\n";
                    detailView.Text += articleView.Notes;
                }

                this.ShowPicture(detailView, imageView);
            }
            catch(Exception ex)
            {
                TRACE(ex);

                imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                headerView.Text = null;
                detailView.Text = ex.Message + ex.StackTrace;
            }
        }

        // Zusätzlich in eine Methode ausgelager,
        // damit der Absturz besser untersucht werden kann.
        private void ShowPicture(TextView detailView, ImageView imageView)
        {
            if (StorageItemQuantityActivity.articleImage?.ImageSmall != null)
            {
                try
                {
                    Bitmap image= BitmapFactory.DecodeByteArray (
                        StorageItemQuantityActivity.articleImage.ImageSmall, 
                        0, 
                        articleImage.ImageSmall.Length);

                    imageView.SetImageBitmap(image);
                }
                catch(Exception ex)
                {
                    detailView.Text += "\r\n" + ex.Message;
                    imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);

                }
            }
            else
            {
                imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                imageView.Alpha = 0.5f;
            }
        }

        private void ShowStorageListForArticle(int articleId)
        {
            StorageItemQuantityActivity.liste = new List<StorageItemQuantityListView>();

            var storageItemQuantityList = Database.GetStorageItemQuantityList(articleId);

            foreach(StorageItemQuantityResult storegeItem in storageItemQuantityList)
            {
                StorageItemQuantityListView item = new StorageItemQuantityListView(storegeItem, this.Resources);

                StorageItemQuantityActivity.liste.Add(item);
            }

            StorageItemQuantityListViewAdapter listAdapter = new StorageItemQuantityListViewAdapter(this, StorageItemQuantityActivity.liste);
            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.Adapter = listAdapter;

            listAdapter.ItemClicked += ListAdapter_ItemClicked;
        }

        private void ListAdapter_ItemClicked(object sender, StorageItemEventArgs e)
        {
            var adapter = sender as StorageItemQuantityListViewAdapter;
            
            string selectQuantity = this.Resources.GetString(Resource.String.StorageItemQuantityList_ActionSelectQuantity);
            string expiryDate     = this.Resources.GetString(Resource.String.StorageItemQuantityList_ActionSelectExpiryDate);
            string SelectStorage  = this.Resources.GetString(Resource.String.StorageItemQuantityList_ActionSelectStorage);

            string[] actions = {
                selectQuantity,
                selectQuantity + " + 1",
                selectQuantity + " - 1",
                expiryDate,
                SelectStorage
                };

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(this.Resources.GetString(Resource.String.StorageItemQuantityList_ChangeDetails));
            builder.SetItems(actions, (sender2, args) =>
            {

                switch (args.Which)
                {
                    case 0: // Anzahl
                        this.ChangeQuantity(e.StorageItem, adapter);
                        break;

                    case 1: // Anzahl + 1
                        e.StorageItem.Quantity++;
                        
                        this.SaveStorageItem(e.StorageItem, adapter);

                        break;

                    case 2: // Anzahl - 1
                        e.StorageItem.Quantity--;
                        if (e.StorageItem.Quantity < 0)
                        {
                            e.StorageItem.Quantity = 0;
                        }

                        this.SaveStorageItem(e.StorageItem, adapter);
                        
                        break;

                    case 3: // Datum
                        this.ChangeBestBeforeDate(e.StorageItem, adapter);
                        break;

                    case 4: // Lagerort
                        this.ChangeStorage(e.StorageItem, adapter);
                        break;
                }

                return;
            });
            builder.Show();
        }

        private void GotoArticleDetails()
        {
            if (this.noArticleDetails)
                return;

            var articleDetails = new Intent(this, typeof(ArticleDetailsActivity));
            articleDetails.PutExtra("ArticleId", this.articleId);
            articleDetails.PutExtra("NoStorageQuantity", true);
            articleDetails.PutExtra("NoDeleteArticle", true);
            this.StartActivityForResult(articleDetails, ArticleDetailId);
        }

        private void ChangeBestBeforeDate(StorageItemQuantityResult storageItem, StorageItemQuantityListViewAdapter adapter)
        {
            DateTime? date = storageItem.BestBefore;

            // Haltbarkeitsdatum erfassen (kann aber auch weggelassen werden)
            adapter.NotifyDataSetInvalidated();

            Action<DateTime?> dateSelected = delegate (DateTime? time)
            {
                if (time.HasValue)
                    storageItem.BestBefore = time.Value;
                else
                    storageItem.BestBefore = null;

                this.SaveStorageItem(storageItem, adapter);
                
                if (this.quantity > 0)
                {
                    this.ChangeQuantity(storageItem, adapter);
                }
            };

            if (!UseAltDatePicker)
            {
                DatePickerFragment frag = DatePickerFragment.NewInstance(dateSelected, date);
                frag.ShowsDialog = true;
                frag.Show(this.SupportFragmentManager, DatePickerFragment.TAG);
            }
            else
            {
                AltDatePickerFragment frag = AltDatePickerFragment.NewInstance(dateSelected, date);
                frag.ShowsDialog = true;
                frag.Show(this.SupportFragmentManager, AltDatePickerFragment.TAG);
            }
        }

        private void ChangeQuantity(StorageItemQuantityResult storageItem, StorageItemQuantityListViewAdapter adapter)
        {
            var dialog = new AlertDialog.Builder(this);
            dialog.SetMessage(this.Resources.GetString(Resource.String.App_EnterQuantity));
            EditText input = new EditText(this)
            {
                InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal
            };

            if (storageItem.Quantity > 0)
            {
                input.Text = storageItem.Quantity.ToString();
            }
            input.SetSelection(input.Text.Length);
            dialog.SetView(input);
            dialog.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (sender, whichButton) =>
                {
                    if (string.IsNullOrEmpty(input.Text))
                        input.Text = "0";

                    decimal neueAnzahl = 0;

                    bool decialOk = Decimal.TryParse(input.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out neueAnzahl);
                    if (decialOk)
                    {
                        storageItem.Quantity = neueAnzahl;

                        this.SaveStorageItem(storageItem, adapter);

                        if (this.quantity >= 1)
                        {
                            this.quantity = this.quantity - (double)neueAnzahl;
                        }

                    }
                });
            dialog.SetNegativeButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => {});
            dialog.Show();
        }

        private void ChangeStorage(StorageItemQuantityResult storageItem, StorageItemQuantityListViewAdapter adapter)
        {
            var storages = Database.GetStorageNames();
            if (storages.Count == 0)
                return;

            storages.Insert(0, "[Kein Lagerort]");

            AlertDialog.Builder dialog = new AlertDialog.Builder(this);
            dialog.SetTitle(this.Resources.GetString(Resource.String.StorageItemQuantityList_SelectStorage));
            dialog.SetItems(storages.ToArray(), (sender, args) =>
            {
                if (args.Which == 0)
                {
                    storageItem.StorageName = null;
                }
                else
                {
                    storageItem.StorageName = storages[args.Which];
                }

                this.SaveStorageItem(storageItem, adapter);

                return;
            });
            dialog.Show();
        }

        private void SaveStorageItem(StorageItemQuantityResult storageItem, StorageItemQuantityListViewAdapter adapter)
        {
            if (this.isEditMode)
            {
                // Änderung merken, später speichern.
                storageItem.IsChanged = true;
            }
            else
            {
                // Änderung sofort speichern.
                Database.UpdateStorageItemQuantity(storageItem);
            }
            adapter.NotifyDataSetChanged();

            this.isChanged = true;
        }
    }
}