using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Main_Button_Einkaufsliste", Icon = "@drawable/ic_shopping_cart_white_48dp")]
    public class ShoppingListActivity : Activity, SearchView.IOnQueryTextListener
    {
        public static readonly int SelectArticleId = 1001;
        public static readonly int EditStorageQuantity = 1002;
        public static readonly int EditArticle = 1003;

        private IParcelable listViewState;

        List<ShoppingListView> liste = new List<ShoppingListView>();

        private string lastSearchText = string.Empty;
        private List<string> supermarketList;
        string supermarket;
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ShoppingItemList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            this.ShowShoppingList();

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            listView.ItemClick += ListView_ItemClick;

            this.LoadSupermarketList();
        }

        private void LoadSupermarketList()
        {
            this.supermarketList = new List<string>();
            this.supermarketList.Add(Resources.GetString(Resource.String.ShoppingList_AllSupermarkets));
            this.supermarketList.AddRange(Database.GetSupermarketNames(true));

            if (this.supermarketList.Count == 1)
            {
                // Mehr als ein Einkaufsladen: Auswahl anzeigen
                var supermarketSelection = FindViewById<LinearLayout>(Resource.Id.ShoppingItemList_SelectSupermarket);
                supermarketSelection.Visibility = ViewStates.Gone;

                this.supermarket = null;
            }

            if (this.supermarketList.Count > 1)
            {
                // Mehr als ein Einkaufsladen: Auswahl anzeigen
                var supermarketSelection = FindViewById<LinearLayout>(Resource.Id.ShoppingItemList_SelectSupermarket);
                supermarketSelection.Visibility = ViewStates.Visible;

                var spinnerSupermarket = FindViewById<Spinner>(Resource.Id.ShoppingItemList_Spinner);
                ArrayAdapter<String> dataAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleSpinnerItem, this.supermarketList);
                dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinnerSupermarket.Adapter = dataAdapter;

                spinnerSupermarket.ItemSelected += SpinnerSupermarket_ItemSelected;

                // Letzte Auswahl wieder aktivieren.
                if (!string.IsNullOrEmpty(this.supermarket))
                {
                    int position = dataAdapter.GetPosition(this.supermarket);
                    if (position >= 0)
                    {
                        spinnerSupermarket.SetSelection(position);
                    }
                }
            }
        }

        private void SpinnerSupermarket_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string newSupermarketName = string.Empty;
            if (e.Position > 0)
            {
                if (this.supermarketList.Count < e.Position)
                    return;

                newSupermarketName = this.supermarketList[e.Position];
            }

            if (newSupermarketName != this.supermarket)
            {
                this.supermarket = newSupermarketName;
                this.ShowShoppingList(this.lastSearchText);
            }
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            ShoppingListView item = Tools.Cast<ShoppingListView>(itemObject);

            string removeText    = Resources.GetString(Resource.String.ShoppingList_Remove);
            string toStorage     = Resources.GetString(Resource.String.ShoppingList_ToStorage);
            string articleDetail = Resources.GetString(Resource.String.ShoppingList_Artikelangaben);
            string gekauft       = Resources.GetString(Resource.String.ShoppingList_Gekauft);

            string[] actions = { "+10", "+1", "-1", "-10", removeText, toStorage, articleDetail, gekauft};

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(item.Heading);
            builder.SetItems(actions, (sender2, args) =>
            {

                switch (args.Which)
                {
                    case 0: // +10
                        Database.AddToShoppingList(item.ArticleId, 10);
                        this.ShowShoppingList();
                        break;

                    case 1: // +1
                        Database.AddToShoppingList(item.ArticleId, 1);
                        this.ShowShoppingList();
                        break;

                    case 2: // -1
                        Database.AddToShoppingList(item.ArticleId, -1);
                        this.ShowShoppingList();
                        break;

                    case 3: // -10
                        Database.AddToShoppingList(item.ArticleId, -10);
                        this.ShowShoppingList();
                        break;

                    case 4: // Entfernen (gekauft)
                        Database.RemoveFromShoppingList(item.ArticleId);
                        this.LoadSupermarketList();
                        this.ShowShoppingList();
                        break;

                    case 5: // Ins Lagerbestand
                        var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                        storageDetails.PutExtra("ArticleId", item.ArticleId);
                        storageDetails.PutExtra("EditMode",  true);
                        this.StartActivityForResult(storageDetails, EditStorageQuantity);
                        break;

                    case 6: // Artikelangaben...
                        var articleDetails = new Intent (this, typeof(ArticleDetailsActivity));
                        articleDetails.PutExtra("ArticleId", item.ArticleId);
                        this.StartActivityForResult(articleDetails, EditArticle);

                        break;

                    case 7: // Als 'Gekauft' markieren
                        Database.SetShoppingItemBought(item.ArticleId, true);
                        this.ShowShoppingList();

                        break;
                }

                return;
            });
            builder.Show();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ShoppingList_menu, menu);

            // https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            SearchManager searchManager = (SearchManager)GetSystemService(Context.SearchService);

            var searchMenuItem = menu.FindItem(Resource.Id.ShoppingList_Search);
            var searchView = (SearchView)searchMenuItem.ActionView;

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

                case Resource.Id.ShoppingList_Add:
                    // Select Article
                    var articleListIntent = new Intent(this, typeof(ArticleListActivity));
                    articleListIntent.PutExtra("SelectArticleOnly", true);

                    this.StartActivityForResult(articleListIntent, SelectArticleId);

                    return true;

                case Resource.Id.ShoppingList_Share:
                    this.ShareList();
                    return true;
            }
            return true;
        }

        private void ShareList()
        {
            string text = string.Empty;

            foreach(ShoppingListView view in this.liste)
            {
                if (!string.IsNullOrEmpty(view.Heading))      text += view.Heading      + "\n";
                if (!string.IsNullOrEmpty(view.SubHeading))   text += view.SubHeading   + "\n";
                if (!string.IsNullOrEmpty(view.QuantityText)) text += view.QuantityText + "\n";
                text += "\n";
            }

            TextView footer = FindViewById<TextView>(Resource.Id.ShoppingItemList_Footer);
            text += footer.Text;

            string subject = string.Format("{0} - {1} {2}",
                Resources.GetString(Resource.String.Main_Button_Einkaufsliste),
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString());

            Intent intentsend = new Intent();
            intentsend.SetAction(Intent.ActionSend);
            intentsend.PutExtra(Intent.ExtraSubject, Resources.GetString(Resource.String.Main_Button_Einkaufsliste));
            intentsend.PutExtra(Intent.ExtraText, text);
            intentsend.SetType("text/plain");
            
            StartActivity(intentsend);
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if ((requestCode == SelectArticleId) && (resultCode == Result.Ok) && (data != null))
            {
                int id = data.GetIntExtra("ArticleId", -1);
                if (id == -1)
                    return;

                Database.AddToShoppingList(id, 1);
                this.ShowShoppingList();
                this.LoadSupermarketList();
            }

            if ((requestCode == EditStorageQuantity) && (resultCode == Result.Ok) && (data != null))
            {
                int id = data.GetIntExtra("ArticleId", -1);
                if (id == -1)
                    return;

                Database.RemoveFromShoppingList(id);
                this.ShowShoppingList();
                this.LoadSupermarketList();
            }

            if ((requestCode == EditArticle) && (resultCode == Result.Ok))
            {
                // Liste aktualisieren
                this.ShowShoppingList();
                this.LoadSupermarketList();
            }
        }

        private void ShowShoppingList(string filter = null)
        {
            this.liste = new List<ShoppingListView>();

            var shoppingList = Database.GetShoppingList(this.supermarket, filter);

            foreach (ShoppingItemListResult shoppingItem in shoppingList)
            {
                this.liste.Add(new ShoppingListView(shoppingItem));
            }

            ShoppingListViewAdapter listAdapter = new ShoppingListViewAdapter(this, this.liste);

            listAdapter.CheckedChanged += ListAdapter_CheckedChanged;

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            this.listViewState = listView.OnSaveInstanceState();        // Zustand der Liste merken (wo der Anfang angezeigt wird)
            listView.Adapter = listAdapter;
            listView.OnRestoreInstanceState(this.listViewState);        // Zustand der Liste wiederherstellen
        
            this.UpdateStatistic();
        }

        private string GetStatistic()
        {
            decimal sum_quantity = 0;
            decimal sum_amount = 0;
            decimal to_pay = 0;
            int sum_noPrice = 0;

            foreach (ShoppingListView view in this.liste)
            {
                ShoppingItemListResult shoppingItem = view.ShoppingItem;

                sum_quantity += shoppingItem.Quantity;
                if (shoppingItem.Price != null)
                {
                    sum_amount += shoppingItem.Quantity * shoppingItem.Price.Value;

                    if (shoppingItem.Bought)
                    {
                        to_pay += shoppingItem.Quantity * shoppingItem.Price.Value;
                    }
                }
                else
                {
                    sum_noPrice++;
                }
            }

            string status;
            if (this.liste.Count == 1)
                status = string.Format("{0:n0} Position", this.liste.Count);
            else
                status = string.Format("{0:n0} Positionen", this.liste.Count);

            if (sum_quantity > 0) status += string.Format(", Anzahl {0:n0}",   sum_quantity);
            if (sum_amount   > 0) status += string.Format(", Betrag {0:n2} €", sum_amount);
            if (sum_noPrice  > 0) status += string.Format(", {0:n0} Artikel ohne Preisangabe", sum_noPrice);
            if (to_pay > 0)       status += string.Format("\nZu zahlen: {0:n2} €", to_pay);

            return status;
        }

        private void UpdateStatistic()
        {
            string status = this.GetStatistic();

            TextView footer = FindViewById<TextView>(Resource.Id.ShoppingItemList_Footer);
            footer.Text = status;
        }

        private void ListAdapter_CheckedChanged(object sender, EventArgs e)
        {
            this.UpdateStatistic();
        }

        public bool OnQueryTextChange(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            this.ShowShoppingList(filter);
            this.lastSearchText = filter;
            return true;
        }

        public bool OnQueryTextSubmit(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            this.ShowShoppingList(filter);
            this.lastSearchText = filter;
            return true;
        }
    }
}