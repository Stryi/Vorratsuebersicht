using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Support.V4.Content;
using static Android.Widget.AdapterView;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Main_Button_Lagerbestand", Icon = "@drawable/ic_assignment_white_48dp")]
    public class StorageItemListActivity : Activity, SearchView.IOnQueryTextListener
    {
        private static bool oderByToConsumeDate;

        List<StorageItemListView> liste = new List<StorageItemListView>();
        private IParcelable listViewState;
        private List<string> storageList;

        private string category;
        private string subCategory;
        private string eanCode;
        private bool   showEmptyStorageArticles;
        private string storageNameFilter = string.Empty;
        private string lastSearchText = string.Empty;
        Toast toast;

        public static readonly int StorageItemQuantityId = 1000;
        public static readonly int SelectArticleId = 1001;
        public static readonly int ArticleDetailId = 1002;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.StorageItemList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            listView.ItemClick += OnOpenArticleStorageItemQuentity;

            this.RegisterForContextMenu(listView);

            this.category                 = Intent.GetStringExtra ("Category");
            this.subCategory              = Intent.GetStringExtra ("SubCategory");
            bool oderByDate               = Intent.GetBooleanExtra("OderByToConsumeDate", false);
            this.eanCode                  = Intent.GetStringExtra("EANCode") ?? string.Empty;
            this.showEmptyStorageArticles = Intent.GetBooleanExtra("ShowEmptyStorageArticles", false); // Auch Artikel ohne Lagerbestand anzeigen

            if (oderByDate)
            {
                StorageItemListActivity.oderByToConsumeDate = true;
            }

            if (!string.IsNullOrEmpty(this.subCategory))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.subCategory);
            }
            else if (!string.IsNullOrEmpty(this.category))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.category);
            }

            if (!string.IsNullOrEmpty(this.eanCode))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.eanCode);
            }

            this.storageList = new List<string>();
            this.storageList.Add(Resources.GetString(Resource.String.StorageItem_AllStoragesStorage));
            this.storageList.AddRange(Database.GetStorageNames());

            if (storageList.Count > 1)
            {
                var storageSelection = FindViewById<LinearLayout>(Resource.Id.StorageItemList_SelectStorageSection);
                storageSelection.Visibility = ViewStates.Visible;

                var spinnerStorage = FindViewById<Spinner>(Resource.Id.StorageItemList_Storages);
                ArrayAdapter<String> dataAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, this.storageList);
                dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinnerStorage.Adapter = dataAdapter;

                spinnerStorage.ItemSelected += SpinnerStorage_ItemSelected;
            }

            this.ShowStorageItemList();
        }

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            if (view.Id == Resource.Id.StorageItemView) 
            {
                ListView listView = (ListView)view;
                AdapterView.AdapterContextMenuInfo acmi = (AdapterContextMenuInfo) menuInfo;
                //ArticleListView obj = (ArticleListView)listView.GetItemAtPosition(acmi.Position);

                menu.Add(Menu.None, 1, Menu.None, Resource.String.StorageItem_Artikelangaben);
                menu.Add(Menu.None, 2, Menu.None, Resource.String.StorageItem_ToShoppingList);
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.MenuInfo;

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            StorageItemListView  selectedItem = Tools.Cast<StorageItemListView>(listView.Adapter.GetItem(info.Position));

            switch(item.ItemId)
            {
                case 1: // Artikelangabe
                    this.OnOpenArticleDetails(selectedItem.ArticleId);
                    return true;

                case 2: // Auf Einkaufszettel
                    this.AddToShoppingListAutomatically(selectedItem.ArticleId);

                    this.SaveListState();
                    this.ShowStorageItemList(this.lastSearchText);
                    this.RestoreListState();

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        private void SpinnerStorage_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string newStorageName = string.Empty;
            if (e.Position > 0)
            {
                newStorageName = this.storageList[e.Position];
            }

            if (newStorageName != this.storageNameFilter)
            {
                this.storageNameFilter = newStorageName;
                this.ShowStorageItemList(this.lastSearchText);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.StorageItemList_menu, menu);

            var sortMenuItem = menu.FindItem(Resource.Id.StorageItemList_Sort);

            if (StorageItemListActivity.oderByToConsumeDate)
            {
                sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_DATE_white_24);
            }
            else
            {
                sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_AZ_white_24);
            }

            var searchMenuItem = menu.FindItem(Resource.Id.StorageItemList_Search);
            var searchView = (SearchView)searchMenuItem.ActionView;

            // https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            searchView.SetOnQueryTextListener(this);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;

                case Resource.Id.StorageItemList_Add:

                    // Select Article -> Menge erfassen
					var articleListIntent = new Intent (this, typeof(ArticleListActivity));
					articleListIntent.PutExtra("SelectArticleOnly", true);

					articleListIntent.PutExtra("Category",    this.category);
					articleListIntent.PutExtra("SubCategory", this.subCategory);
					articleListIntent.PutExtra("NotInStorage", true);

					this.StartActivityForResult (articleListIntent, SelectArticleId);

                    return true;

                case Resource.Id.StorageItemList_Sort:

                    StorageItemListActivity.oderByToConsumeDate = !StorageItemListActivity.oderByToConsumeDate;
                    this.ShowStorageItemList(this.lastSearchText);
                    this.InvalidateOptionsMenu();

                    return true;

                case Resource.Id.StorageItemList_Share:
                    this.ShareList();
                    return true;
            }
            return true;
        }

        private void ShareList()
        {
            string text = string.Empty;

            foreach(StorageItemListView view in this.liste)
            {
                if (!string.IsNullOrEmpty(view.Heading))     text += view.Heading     + "\n";
                if (!string.IsNullOrEmpty(view.SubHeading))  text += view.SubHeading  + "\n";
                if (!string.IsNullOrEmpty(view.ErrorText))   text += view.ErrorText   + "\n";
                if (!string.IsNullOrEmpty(view.WarningText)) text += view.WarningText + "\n";
                if (!string.IsNullOrEmpty(view.InfoText))    text += view.InfoText + "\n";
                text += "\n";
            }

            TextView footer = FindViewById<TextView>(Resource.Id.StorageItemList_Footer);
            text += footer.Text;

            string subject = string.Format("{0} - {1} {2}",
                Resources.GetString(Resource.String.Main_Button_Lagerbestand),
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString());

            Intent intentsend = new Intent();
            intentsend.SetAction(Intent.ActionSend);
            intentsend.PutExtra(Intent.ExtraSubject, subject);
            intentsend.PutExtra(Intent.ExtraText, text);
            intentsend.SetType("text/plain");
            
            StartActivity(intentsend);
        }

        private void OnOpenArticleStorageItemQuentity(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            StorageItemListView item = Tools.Cast<StorageItemListView>(itemObject);

            StorageItemQuantityActivity.Reload();   // Artikel neu laden

            var storageItemQuantity = new Intent (this, typeof(StorageItemQuantityActivity));
            storageItemQuantity.PutExtra("Heading",   item.Heading);
            storageItemQuantity.PutExtra("ArticleId", item.ArticleId);
            this.StartActivityForResult(storageItemQuantity, StorageItemQuantityId);

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            this.listViewState = listView.OnSaveInstanceState();
        }

        private void OnOpenArticleDetails(int articleId)
        {
            var articleDetails = new Intent(this, typeof(ArticleDetailsActivity));
            articleDetails.PutExtra("ArticleId", articleId);
            this.StartActivityForResult(articleDetails, ArticleDetailId);

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            this.listViewState = listView.OnSaveInstanceState();

            this.SaveListState();
        }

        private void SaveListState()
        {
            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            this.listViewState = listView.OnSaveInstanceState();
        }

        private void RestoreListState()
        {
            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            listView?.OnRestoreInstanceState(this.listViewState);
        }


        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == StorageItemQuantityId) && (resultCode == Result.Ok))
            {
                this.ShowStorageItemList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                this.RestoreListState();
            }

            if ((requestCode == ArticleDetailId) && (resultCode == Result.Ok))
            {
                this.ShowStorageItemList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                this.RestoreListState();
            }

            if ((requestCode == SelectArticleId) && (resultCode == Result.Ok) && (data != null))
            {
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

				ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
				this.listViewState = listView.OnSaveInstanceState();
			}

        }

        private void ShowStorageItemList(string filter = null)
        {
            this.liste = new List<StorageItemListView>();

            StockStatistic statistic = new StockStatistic();

            var storageItemQuantityList = Database.GetStorageItemQuantityListNoImage(
                this.category,
                this.subCategory,
                this.eanCode,
                this.showEmptyStorageArticles,
                filter, 
                this.storageNameFilter,
                StorageItemListActivity.oderByToConsumeDate);
            
            foreach(StorageItemQuantityResult storegeItem in storageItemQuantityList)
            {
				// Informationen über die Mengen zum Ablaufdatum.
				var storageItemBestList = Database.GetBestBeforeItemQuantity(storegeItem.ArticleId);

                string info    = string.Empty;
			    string warning = string.Empty;
			    string error   = string.Empty;
			
			    foreach(StorageItemQuantityResult result in storageItemBestList)
			    {
				    if (result.WarningLevel == 0)
				    {
                        if (result.BestBefore == null)
                        {
					        if (!string.IsNullOrEmpty(info)) info += "\r\n";
					        info += string.Format("{0} ohne Ablaufdatum", result.Quantity);
                        }
                        else
                        {
					        if (!string.IsNullOrEmpty(info)) info += "\r\n";
					        info += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());
                        }
                    }

				    if (result.WarningLevel == 1)
				    {
					    if (!string.IsNullOrEmpty(warning)) warning += "\r\n";
					    warning += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());

                        statistic.AddWarningLevel1(result.Quantity);
				    }
				    if (result.WarningLevel == 2)
				    {
					    if (!string.IsNullOrEmpty(error)) error += "\r\n";
					    error += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());

                        statistic.AddWarningLevel2(result.Quantity);
				    }
			    }

			    storegeItem.BestBeforeInfoText    = info;
			    storegeItem.BestBeforeWarningText = warning;
			    storegeItem.BestBeforeErrorText   = error;

                statistic.AddStorageItem(storegeItem);

                liste.Add(new StorageItemListView(storegeItem));
            }

            StorageItemListViewAdapter listAdapter = new StorageItemListViewAdapter(this, liste);

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            listView.Adapter = listAdapter;
            listView.Focusable = true;

            TextView footer = FindViewById<TextView>(Resource.Id.StorageItemList_Footer);
            footer.Text = statistic.GetStatistic();
        }

        public bool OnQueryTextChange(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            //ggf. auf Adapter Filter umstellen: ...listAdapter.Filter.InvokeFilter(newText);
            this.ShowStorageItemList(filter);
            this.lastSearchText = filter;
            
            return true;
        }

        public bool OnQueryTextSubmit(string filter)
        {
            if (this.lastSearchText == filter)
                return false;

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            this.ShowStorageItemList(filter);
            this.lastSearchText = filter;
            return true;
        }

        private void AddToShoppingListAutomatically(int articleId)
        {
            int toBuyQuantity = Database.GetToShoppingListQuantity(articleId);
            if (toBuyQuantity == 0)
                toBuyQuantity = 1;

            double count = Database.AddToShoppingList(articleId, toBuyQuantity);

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
    }

}