using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    [Activity(Label = "Lagerbestand", Icon = "@drawable/ic_assignment_white_48dp")]
    public class StorageItemListActivity : Activity
    {
        List<StorageItemListView> liste = new List<StorageItemListView>();
        private IParcelable listViewState;

        private string category;
        private string subCategory;
        private bool   showToConsumerOnly;
        private string eanCode;
        private bool   showEmptyArticles;

        public static readonly int StorageItemQuantityId = 1000;
        public static readonly int SelectArticleId = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.StorageItemList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            ListView listView = FindViewById<ListView>(Resource.Id.MyListView);

            //listView.ItemClick += ShowArticlePopupMenu;
            listView.ItemClick += OnOpenArticleDetails;

            this.category           = Intent.GetStringExtra ("Category") ?? string.Empty;
            this.subCategory        = Intent.GetStringExtra ("SubCategory") ?? string.Empty;
            this.showToConsumerOnly = Intent.GetBooleanExtra("ShowToConsumerOnly", false);
            this.eanCode            = Intent.GetStringExtra("EANCode") ?? string.Empty;
            this.showEmptyArticles  = Intent.GetBooleanExtra("ShowEmptyStorageArticles", false); // Auch Artikel ohne Lagerbestand anzeigen

            if (!string.IsNullOrEmpty(this.subCategory))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.subCategory);
            }

            if (!string.IsNullOrEmpty(this.eanCode))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.eanCode);
            }

            this.ShowStorageItemList();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StorageItemList_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.StorageItemList_Add:

                    // Select Article -> Menge erfassen
					var articleListIntent = new Intent (this, typeof(ArticleListActivity));
					articleListIntent.PutExtra("SelectArticleOnly", true);

					articleListIntent.PutExtra("Category",    this.category);
					articleListIntent.PutExtra("SubCategory", this.subCategory);

					this.StartActivityForResult (articleListIntent, SelectArticleId);

                    return true;
            }
            return true;
        }

        private void OnOpenArticleDetails(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            StorageItemListView item = Tools.Cast<StorageItemListView>(itemObject);

            StorageItemQuantityActivity.Reload();   // Artikel neu laden

            var storageItemQuantity = new Intent (this, typeof(StorageItemQuantityActivity));
            storageItemQuantity.PutExtra("Heading",   item.Heading);
            storageItemQuantity.PutExtra("ArticleId", item.Id);
            this.StartActivityForResult(storageItemQuantity, StorageItemQuantityId);

            ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
            this.listViewState = listView.OnSaveInstanceState();

        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == StorageItemQuantityId) && (resultCode == Result.Ok))
            {
                this.ShowStorageItemList();

                ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
                listView.OnRestoreInstanceState(this.listViewState);
            }

            if ((requestCode == SelectArticleId) && (resultCode == Result.Ok) && (data != null))
            {

				StorageItemListView item = Tools.Cast<StorageItemListView>(data);

				// Anzeige Menge prü Artikel
				var storageItemQuantity = new Intent (this, typeof(StorageItemQuantityActivity));
				string heading = data.GetStringExtra("Heading");
				int id         = data.GetIntExtra("ArticleId", -1);
				if (id == -1)
					return;

				storageItemQuantity.PutExtra("Heading",   heading);
				storageItemQuantity.PutExtra("ArticleId", id);
				storageItemQuantity.PutExtra("EditMode",  true);

				this.StartActivityForResult(storageItemQuantity, StorageItemQuantityId);

				ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
				this.listViewState = listView.OnSaveInstanceState();
			}

        }

        private void ShowStorageItemList()
        {
            this.liste = new List<StorageItemListView>();

            var storageItemQuantityList = Database.GetStorageItemQuantityList(this.category, this.subCategory, this.eanCode, this.showEmptyArticles);
            foreach(StorageItemQuantityResult storegeItem in storageItemQuantityList)
            {
                bool isWarning = false;

				// Mindestens eine Position mit Ablaufdatum überschritten oder nur Warnung für Ablaufdatum.
				if (storegeItem.WarningLevel > 0)
				{
					// Informationen über die Mengen zum Ablaufdatum.
					var storageItemBestList = Database.GetBestBeforeItemQuantity(storegeItem);

			        string info    = string.Empty;
			        string warning = string.Empty;
			
			        foreach(StorageItemQuantityResult result in storageItemBestList)
			        {
				        if (result.WarningLevel == 1)
				        {
					        if (!string.IsNullOrEmpty(info)) info += "\r\n";
					        info += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());
				        }
				        if (result.WarningLevel == 2)
				        {
					        if (!string.IsNullOrEmpty(warning)) warning += "\r\n";
					        warning += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());
				        }
			        }

			        storegeItem.BestBeforeInfoText    = info;
			        storegeItem.BestBeforeWarningText = warning;

                    isWarning = true;
                }

                if (this.showToConsumerOnly)
                {
                    if (!isWarning)
                        continue;
                }

                liste.Add(new StorageItemListView(storegeItem));
            }

            StorageItemListViewAdapter listAdapter = new StorageItemListViewAdapter(this, liste);

            ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
            listView.Adapter = listAdapter;
            listView.Focusable = true;
        }
    }
}