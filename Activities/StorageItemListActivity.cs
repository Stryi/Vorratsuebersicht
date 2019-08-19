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

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            if (view.Id == Resource.Id.StorageItemView) 
            {
                ListView listView = (ListView)view;
                AdapterView.AdapterContextMenuInfo acmi = (AdapterContextMenuInfo) menuInfo;
                //ArticleListView obj = (ArticleListView)listView.GetItemAtPosition(acmi.Position);

                menu.Add(Menu.None, 1, Menu.None, Resource.String.StorageItem_ToShoppingList);
                menu.Add(Menu.None, 2, Menu.None, Resource.String.StorageItem_Artikelangaben);
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.MenuInfo;

            ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
            StorageItemListView  selectedItem = Tools.Cast<StorageItemListView>(listView.Adapter.GetItem(info.Position));

            switch(item.ItemId)
            {
                case 1: // Auf Einkaufszettel
                    this.AddToShoppingListAutomatically(selectedItem.Id);
                    return true;

                case 2: // Artikelangabe
                    this.OnOpenArticleDetails(selectedItem.Id);
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

        private void OnOpenArticleStorageItemQuentity(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            StorageItemListView item = Tools.Cast<StorageItemListView>(itemObject);

            StorageItemQuantityActivity.Reload();   // Artikel neu laden

            var storageItemQuantity = new Intent (this, typeof(StorageItemQuantityActivity));
            storageItemQuantity.PutExtra("Heading",   item.Heading);
            storageItemQuantity.PutExtra("ArticleId", item.Id);
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
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == StorageItemQuantityId) && (resultCode == Result.Ok))
            {
                this.ShowStorageItemList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
                listView?.OnRestoreInstanceState(this.listViewState);
            }

            if ((requestCode == ArticleDetailId) && (resultCode == Result.Ok))
            {
                this.ShowStorageItemList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
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

				ListView listView = FindViewById<ListView>(Resource.Id.StorageItemView);
				this.listViewState = listView.OnSaveInstanceState();
			}

        }

        private void ShowStorageItemList(string filter = null)
        {
            this.liste = new List<StorageItemListView>();

            StockStatistic statistic = new StockStatistic();

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

                            statistic.AddWarningLevel1(result.Quantity);
				        }
				        if (result.WarningLevel == 2)
				        {
					        if (!string.IsNullOrEmpty(warning)) warning += "\r\n";
					        warning += string.Format("{0} mit Ablaufdatum {1}", result.Quantity, result.BestBefore.Value.ToShortDateString());

                            statistic.AddWarningLevel2(result.Quantity);
				        }
			        }

			        storegeItem.BestBeforeInfoText    = info;
			        storegeItem.BestBeforeWarningText = warning;
                }

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

    internal class StockStatistic
    {
        int count = 0;
        int quantity = 0;
        Dictionary<string, decimal> sum_menge = new Dictionary<string, decimal>();
        int sum_warnung = 0;
        int sum_abgelaufen = 0;
        int sum_kcal = 0;
        decimal sum_price = 0;

        internal void AddWarningLevel1(int quantity)
        {
            sum_warnung += quantity;
        }

        internal void AddWarningLevel2(int quantity)
        {
            sum_abgelaufen += quantity;
        }
        
        private void AddUnitQuantity(string unit, decimal size, int quantity)
        {
            if (string.IsNullOrEmpty(unit))
                unit = string.Empty;

            if (!sum_menge.ContainsKey(unit))
            {
                sum_menge.Add(unit, size * quantity);
            }
            else
            {
                sum_menge[unit] += size * quantity;
            }
        }

        private void AddCalorie(int quantity, int calorie)
        {
            sum_kcal += quantity * calorie;
        }

        internal void AddStorageItem(StorageItemQuantityResult storegeItem)
        {
            this.count ++;
            this.quantity += storegeItem.Quantity;
            this.AddUnitQuantity(storegeItem.Unit, storegeItem.Size, storegeItem.Quantity);
            this.AddCalorie(storegeItem.Quantity, storegeItem.Calorie);
            this.AddCosts(storegeItem.Quantity, storegeItem.Price);
        }

        private void AddCosts(int quantity, decimal price)
        {
            this.sum_price += quantity * price;
        }

        internal string GetStatistic()
        {
            string status;
            if (this.count == 1)
                status = string.Format("{0:n0} Position", this.count);
            else
                status = string.Format("{0:n0} Positionen", this.count);

            status += string.Format(", Anzahl: {0:n0}", this.quantity);

            if (sum_menge.Count > 0)
            {
                string mengeListe = string.Empty;

                foreach(var menge in sum_menge)
                {
                    if (menge.Value == 0)
                        continue;

                    if (!string.IsNullOrEmpty(mengeListe))
                        mengeListe += ", ";
                    mengeListe += string.Format("{0:n0} {1}", menge.Value, menge.Key);
                }
                if (!string.IsNullOrEmpty(mengeListe))
                {
                    status += string.Format(", Menge: {0:n0}", mengeListe);
                }
            }

            if (sum_kcal        > 0) status += string.Format(", Kalorien: {0:n0}",   sum_kcal);
            if (sum_price       > 0) status += string.Format(", Wert: {0:n2} €",       sum_price);
            if (sum_warnung     > 0) status += string.Format(", {0:n0} Warnung(en)", sum_warnung);
            if (sum_abgelaufen  > 0) status += string.Format(", {0:n0} Abgelaufen",  sum_abgelaufen);
            
            return status;
        }
    }
}