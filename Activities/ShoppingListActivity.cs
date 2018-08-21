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
        private IParcelable listViewState;

        List<ShoppingListView> liste = new List<ShoppingListView>();

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
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            ShoppingListView item = Tools.Cast<ShoppingListView>(itemObject);

            string removeText = Resources.GetString(Resource.String.ShoppingList_Remove);
            string toStorage  = Resources.GetString(Resource.String.ShoppingList_ToStorage);

            string[] actions = { "+10", "+1", "-1", "-10", removeText, toStorage };

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(item.Heading);
            builder.SetItems(actions, (sender2, args) =>
            {

                switch (args.Which)
                {
                    case 0: // +10
                        Database.AddToShoppingList(item.ArticleId, 10);
                        ShowShoppingList();
                        break;

                    case 1: // +1
                        Database.AddToShoppingList(item.ArticleId, 1);
                        ShowShoppingList();
                        break;

                    case 2: // -1
                        Database.AddToShoppingList(item.ArticleId, -1);
                        ShowShoppingList();
                        break;

                    case 3: // -10
                        Database.AddToShoppingList(item.ArticleId, -10);
                        ShowShoppingList();
                        break;

                    case 4: // Entfernen
                        Database.RemoveFromShoppingList(item.ArticleId);
                        ShowShoppingList();
                        break;

                    case 5: // Ins Lagerbestand
                        var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                        storageDetails.PutExtra("ArticleId", item.ArticleId);
                        storageDetails.PutExtra("EditMode",  true);
                        this.StartActivityForResult(storageDetails, EditStorageQuantity);
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
            }
            return true;
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
            }

            if ((requestCode == EditStorageQuantity) && (resultCode == Result.Ok) && (data != null))
            {
                int id = data.GetIntExtra("ArticleId", -1);
                if (id == -1)
                    return;

                Database.RemoveFromShoppingList(id);
                ShowShoppingList();
            }
        }

        private void ShowShoppingList(string filter = null)
        {
            this.liste = new List<ShoppingListView>();

            var shoppingList = Database.GetShoppingList(filter);

            foreach (ShoppingItemListResult ShoppingItem in shoppingList)
            {
                this.liste.Add(new ShoppingListView(ShoppingItem));
            }

            ShoppingListViewAdapter listAdapter = new ShoppingListViewAdapter(this, this.liste);

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            this.listViewState = listView.OnSaveInstanceState();        // Zustand der Liste merken (wo der Anfang angezeigt wird)
            listView.Adapter = listAdapter;
            listView.OnRestoreInstanceState(this.listViewState);        // Zustand der Liste wiederherstellen
        }

        public bool OnQueryTextChange(string newText)
        {
            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            this.ShowShoppingList(newText);
            return true;
        }

        public bool OnQueryTextSubmit(string query)
        {
            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            this.ShowShoppingList(query);
            return true;
        }
    }
}