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
    [Activity(Label = "@string/Main_Button_Lagerbestand", Icon = "@drawable/ic_assignment_white_48dp")]
    public class StorageItemListActivity : Activity, SearchView.IOnQueryTextListener
    {
        List<StorageItemListView> liste = new List<StorageItemListView>();
        private IParcelable listViewState;
        private List<string> storageList;

        private string category;
        private string subCategory;
        private bool   showToConsumerOnly;
        private string eanCode;
        private bool   showEmptyStorageArticles;
        private string storageNameFilter = string.Empty;
        private string lastSearchText = string.Empty;

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
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            ListView listView = FindViewById<ListView>(Resource.Id.MyListView);

            listView.ItemClick += OnOpenArticleDetails;

            this.category                 = Intent.GetStringExtra ("Category");
            this.subCategory              = Intent.GetStringExtra ("SubCategory");
            this.showToConsumerOnly       = Intent.GetBooleanExtra("ShowToConsumerOnly", false);
            this.eanCode                  = Intent.GetStringExtra("EANCode") ?? string.Empty;
            this.showEmptyStorageArticles = Intent.GetBooleanExtra("ShowEmptyStorageArticles", false); // Auch Artikel ohne Lagerbestand anzeigen

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

					this.StartActivityForResult (articleListIntent, SelectArticleId);

                    return true;

                case Resource.Id.StorageItemList_Filter:

                    this.showToConsumerOnly = !this.showToConsumerOnly;
                    this.ShowStorageItemList(this.lastSearchText);

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
                this.ShowStorageItemList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
                listView?.OnRestoreInstanceState(this.listViewState);
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

				ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
				this.listViewState = listView.OnSaveInstanceState();
			}

        }

        private void ShowStorageItemList(string filter = null)
        {
            this.liste = new List<StorageItemListView>();

            int sum_anzahl = 0;
            int sum_warnung = 0;
            int sum_abgelaufen = 0;
            int sum_kcal = 0;

            var storageItemQuantityList = Database.GetStorageItemQuantityListNoImage(this.category, this.subCategory, this.eanCode, this.showEmptyStorageArticles, filter, this.storageNameFilter);
            foreach(StorageItemQuantityResult storegeItem in storageItemQuantityList)
            {
                if (storegeItem.WarningLevel == 0)
                {
                    if (this.showToConsumerOnly)
                        continue;
                }

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
                            sum_warnung += result.Quantity;
				        }
				        if (result.WarningLevel == 2)
				        {
					        if (!string.IsNullOrEmpty(warning)) warning += "\r\n";
					        warning += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());
                            sum_abgelaufen += result.Quantity;
				        }
			        }

			        storegeItem.BestBeforeInfoText    = info;
			        storegeItem.BestBeforeWarningText = warning;
                }

                sum_anzahl += storegeItem.Quantity;
                sum_kcal   += storegeItem.Quantity * storegeItem.Calorie;

                liste.Add(new StorageItemListView(storegeItem));
            }

            StorageItemListViewAdapter listAdapter = new StorageItemListViewAdapter(this, liste);

            ListView listView = FindViewById<ListView>(Resource.Id.MyListView);
            listView.Adapter = listAdapter;
            listView.Focusable = true;

            TextView footer = FindViewById<TextView>(Resource.Id.StorageItemList_Footer);
            
            string status = string.Format("Anzahl: {0}", liste.Count);

            if (sum_anzahl     > 0) status += string.Format(", Menge: {0:n0}",      sum_anzahl);
            if (sum_warnung    > 0) status += string.Format(", Warnung: {0:n0}",    sum_warnung);
            if (sum_abgelaufen > 0) status += string.Format(", Abgelaufen: {0:n0}", sum_abgelaufen);
            if (sum_kcal       > 0) status += string.Format(", Kalorien: {0:n0}",   sum_kcal);

            footer.Text = status;
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
    }
}