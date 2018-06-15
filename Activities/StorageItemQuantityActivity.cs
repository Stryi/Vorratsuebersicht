using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    [Activity(Label = "Artikelbestand", Icon = "@drawable/ic_assignment_white_48dp")]
    public class StorageItemQuantityActivity : Activity
    {
        public static List<StorageItemQuantityListView> liste = null;
        public static Article article = null;

        int articleId;
        string text;
        bool durableInfinity = false;

        bool isChanged = false;

        bool isEditMode = false;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.StorageItemQuantity);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            this.text       = Intent.GetStringExtra ("Heading") ?? string.Empty;
            this.articleId  = Intent.GetIntExtra    ("ArticleId", 0);

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

            ImageView image = FindViewById<ImageView>(Resource.Id.StorageItemQuantity_Image);
            image.Click += delegate 
            {
                if (StorageItemQuantityActivity.article.ImageLarge == null)
                   return;

                var articleImage = new Intent (this, typeof(ArticleImageActivity));
                articleImage.PutExtra("Heading", text);
                articleImage.PutExtra("ArticleId", articleId);
                this.StartActivityForResult(articleImage, 1000);
            };

            ImageButton addRemove = FindViewById<ImageButton>(Resource.Id.StorageItemQuantity_AddArticle);
            addRemove.Click += delegate 
            {
                StorageItemQuantityResult storageItemQuantity = new StorageItemQuantityResult();
				storageItemQuantity.ArticleId    = this.articleId;
                storageItemQuantity.Quantity     = 1;
                storageItemQuantity.QuantityDiff = 1;
                storageItemQuantity.BestBefore    = DateTime.Today;

                StorageItemQuantityListView itemView = new StorageItemQuantityListView(storageItemQuantity);

                ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
                ((StorageItemQuantityListViewAdapter)listView.Adapter).Add(itemView);
                listView.InvalidateViews();

                if (!this.durableInfinity)
                {
                    // Haltbarkeitsdatum erfassen (kann aber auch weggelassen werden)
				    DatePickerFragment frag = DatePickerFragment.NewInstance(delegate(DateTime? time) 
						    {
                                if (time.HasValue)
							        storageItemQuantity.BestBefore = time.Value;
                                else
							        storageItemQuantity.BestBefore = null;

                                listView.InvalidateViews();
						    });
				    frag.ShowsDialog = true;
				    frag.Show(FragmentManager, DatePickerFragment.TAG);
                }
                else
                {
                    // Ist ohne Haltbarkeitsdatum (unendlich haltbar)
                    // Datum muss nicht erfasst werden.
                    storageItemQuantity.BestBefore = null;
                    listView.InvalidateViews();
                }
            };
        }

        public static void Reload()
        {
            StorageItemQuantityActivity.liste = null;
            StorageItemQuantityActivity.article = null;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StorageItemQuantity_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnPrepareOptionsMenu(IMenu menu)
        {
            menu.FindItem(Resource.Id.StorageItemQuantity_Edit).SetVisible(!this.isEditMode);
            menu.FindItem(Resource.Id.StorageItemQuantity_Cancel).SetVisible(this.isEditMode);
            menu.FindItem(Resource.Id.StorageItemQuantity_Save).SetVisible(this.isEditMode);

            if (StorageItemQuantityActivity.article.ImageLarge == null)
                menu.FindItem(Resource.Id.StorageItemQuantity_EditPicture).SetVisible(false);

            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.StorageItemQuantity_Edit:
					this.SetEditMode(true);

                    return true;

                case Resource.Id.StorageItemQuantity_Save:
					this.isChanged = true;
                    this.SaveNewQuantity();

					this.SetEditMode(false);
                    break;

                case Resource.Id.StorageItemQuantity_Cancel:
                    this.ShowStorageListForArticle(this.articleId);

					this.SetEditMode(false);

                    break;

                case Resource.Id.StorageItemQuantity_EditPicture:
                    var articleImage = new Intent (this, typeof(ArticleImageActivity));
                    articleImage.PutExtra("Heading", text);
                    articleImage.PutExtra("ArticleId", articleId);
                    this.StartActivityForResult(articleImage, 1000);
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
                FindViewById(Resource.Id.StorageItemQuantity_AddArticle).Visibility = ViewStates.Visible;
                adapter.ActivateButtons();
            }
            else
            {
                FindViewById(Resource.Id.StorageItemQuantity_AddArticle).Visibility = ViewStates.Gone;
                adapter.DeactivateButtons();
            }

            listView.InvalidateViews();
		}

		protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode == Result.Ok)
            {
                StorageItemQuantityActivity.article = null;
                this.ShowPictureAndDetails(this.articleId, this.text);
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

            StorageItemQuantityActivity.Reload();
        }

        private void SaveNewQuantity()
        {
            foreach(StorageItemQuantityListView item in StorageItemQuantityActivity.liste)
            {
                if (item.StorageItem.QuantityDiff == 0)
                    continue;

                System.Diagnostics.Trace.WriteLine(item.StorageItem.Quantity);

                Database.UpdateStorageItemQuantity(
					item.StorageItem);

                item.StorageItem.QuantityDiff = 0;  // Änderung gespeichert.
            }
        }

        private void ShowPictureAndDetails(int articleId, string title)
        {
            var headerView = FindViewById<TextView>  (Resource.Id.ArticleDetailHeader);
            var imageView  = FindViewById<ImageView> (Resource.Id.StorageItemQuantity_Image);
            var detailView = FindViewById<TextView>  (Resource.Id.ArticleDetailList);

            if (articleId == 0)
            {
                headerView.Text = title;
                detailView.Text = "Details zum " + title;
                imageView.SetImageResource(Resource.Drawable.ic_add_a_photo_white_24dp);

                return;
            }

            if (StorageItemQuantityActivity.article == null)
            {
                StorageItemQuantityActivity.article = Database.GetArticle(articleId);
            }

            Article article = StorageItemQuantityActivity.article;

            this.durableInfinity = article.DurableInfinity;

            headerView.Text = article.Name;
			string info = string.Empty;

			if (!string.IsNullOrEmpty(article.Manufacturer))
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Hersteller: {0}", article.Manufacturer);
			}

			if (article.Size.HasValue)
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Menge: {0} {1}", article.Size.Value, article.Unit);
			}
			if (article.Calorie.HasValue)
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Kalorien: {0:n0}", article.Calorie.Value);
			}
            if (article.DurableInfinity == false && article.WarnInDays.HasValue)
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Warnen: {0} Tage(n) vor Ablauf", article.WarnInDays.Value);
			}

			if (!string.IsNullOrEmpty(article.Category))
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Kategorie: {0}", article.Category);
			}

			if (!string.IsNullOrEmpty(article.SubCategory))
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("Unterkategorie: {0}", article.SubCategory);
			}

			if (!string.IsNullOrEmpty(article.EANCode))
			{
				if (!string.IsNullOrEmpty(info)) info += "\r\n";
				info += string.Format("EAN Code: {0}", article.EANCode);
			}

            detailView.Text = info;

            if (article.ImageLarge != null)
            {
                Bitmap image= BitmapFactory.DecodeByteArray (article.ImageLarge, 0, article.ImageLarge.Length);

                imageView.SetImageBitmap(image);
            }
            else
                imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
        }


        private void ShowStorageListForArticle(int articleId)
        {
            StorageItemQuantityActivity.liste = new List<StorageItemQuantityListView>();

            var storageItemQuantityList = Database.GetStorageItemQuantityList(articleId);

            foreach(StorageItemQuantityResult storegeItem in storageItemQuantityList)
            {
                StorageItemQuantityListView item = new StorageItemQuantityListView(storegeItem);

                StorageItemQuantityActivity.liste.Add(item);
            }

            StorageItemQuantityListViewAdapter listAdapter = new StorageItemQuantityListViewAdapter(this, StorageItemQuantityActivity.liste);
            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.Adapter = listAdapter;
        }
    }
}