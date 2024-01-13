using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Text;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    using static Tools;

    [Activity(Label = "@string/Main_Button_Einkaufsliste", Icon = "@drawable/ic_shopping_cart_white_48dp")]
    public class ShoppingListActivity : Activity, SearchView.IOnQueryTextListener
    {
        internal static int oderBy = 1;

        public static readonly int SelectArticleId = 1001;
        public static readonly int EditStorageQuantity = 1002;
        public static readonly int EditArticle = 1003;

        private IParcelable listViewState;
        private Handler textSearchDelayHandler = new Handler();

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

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            listView.ItemClick += ListView_ItemClick;

            ImageButton addButton = FindViewById<ImageButton>(Resource.Id.ShoppingItemList_AddPosition);
            addButton.Click += AddArticle_Click;

            try
            {
                this.ShowShoppingList();

                this.LoadSupermarketList();
            }
            catch (Exception ex)
            {
                TRACE(ex);
                Toast.MakeText(this, ex.Message, ToastLength.Short);
            }
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
                ArrayAdapter<String> dataAdapter = new ArrayAdapter<String>(this, Resource.Layout.Spinner_Black, this.supermarketList);
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

        private void AddArticle_Click(object sender, EventArgs e)
        {
            // Select Article
            var articleListIntent = new Intent(this, typeof(ArticleListActivity));
            articleListIntent.PutExtra("SelectArticleOnly", true);
			articleListIntent.PutExtra("NotInShoppingList", true);

            this.StartActivityForResult(articleListIntent, SelectArticleId);
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            ShoppingListView item = Tools.Cast<ShoppingListView>(itemObject);

            string removeText    = Resources.GetString(Resource.String.ShoppingList_Remove);
            string toStorage     = Resources.GetString(Resource.String.ShoppingList_ToStorage);
            string articleDetail = Resources.GetString(Resource.String.ShoppingList_ArticleDetails);
            string gekauft       = Resources.GetString(Resource.String.ShoppingList_MarkAsBought);

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
                        decimal shoppingItemCount = item.Quantity;
                        if (shoppingItemCount == 0)
                            shoppingItemCount = 1;

                        var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                        storageDetails.PutExtra("ArticleId", item.ArticleId);
                        storageDetails.PutExtra("EditMode",  true);
                        storageDetails.PutExtra("Quantity",  (double)shoppingItemCount);
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

            var sortMenuItem = menu.FindItem(Resource.Id.ShoppingList_Sort);

            switch(ShoppingListActivity.oderBy)
            {
                case 1:
                    sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_CHECK_white_24);
                    break;

                case 2:
                    sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_SHOP_white_24);
                    break;

                case 3:
                    sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_TIME_white_24);
                    break;

                case 4:
                    sortMenuItem.SetIcon(Resource.Drawable.baseline_sort_AZ_white_24);
                    break;
            }

            var searchMenuItem = menu.FindItem(Resource.Id.ShoppingList_Search);
            var searchView = (SearchView)searchMenuItem.ActionView;

            var viewType = menu.FindItem(Resource.Id.ShoppingList_Sparse);
            viewType.SetChecked(ShoppingListView.sparseView == 1);

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
        			articleListIntent.PutExtra("NotInShoppingList", true);

                    this.StartActivityForResult(articleListIntent, SelectArticleId);

                    return true;

                case Resource.Id.ShoppingList_Sort:

                    ShoppingListActivity.oderBy++;
                    if (ShoppingListActivity.oderBy > 4)
                        ShoppingListActivity.oderBy = 1;

                    this.ShowShoppingList();
                    this.InvalidateOptionsMenu();

                    Settings.PutInt("ShoppingListOrder", ShoppingListActivity.oderBy);

                    return true;

                case Resource.Id.ShoppingList_Share:
                    this.ShareList();
                    return true;

                case Resource.Id.ShoppingList_Sparse:
                    ShoppingListView.sparseView = 1 - ShoppingListView.sparseView;

                    this.ShowShoppingList();
                    this.InvalidateOptionsMenu();

                    Settings.PutInt("ShoppingListViewType", ShoppingListView.sparseView);

                    return true;

            }
            return true;
        }

        private void ShareList()
        {
            if (MainActivity.IsGooglePlayPreLaunchTestMode)
                return;

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
                Resources.GetString(Resource.String.Main_Button_AufEinkaufsliste),
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString());

            Intent intentsend = new Intent();
            intentsend.SetAction(Intent.ActionSend);
            intentsend.PutExtra(Intent.ExtraSubject, Resources.GetString(Resource.String.Main_Button_AufEinkaufsliste));
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

            var shoppingList = Database.GetShoppingList(this.supermarket, filter, ShoppingListActivity.oderBy);

            foreach (ShoppingItemListResult shoppingItem in shoppingList)
            {
                this.liste.Add(new ShoppingListView(shoppingItem, this.Resources));
            }

            ShoppingListViewAdapter listAdapter = new ShoppingListViewAdapter(this, this.liste);

            listAdapter.CheckedChanged += ListAdapter_CheckedChanged;
            listAdapter.QuantityClicked += ListAdapter_QuantityClicked;

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

                    if (shoppingItem.Bought.HasValue && shoppingItem.Bought.Value)
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
                status = string.Format(this.Resources.GetString(Resource.String.ShoppingListSummary_Position), this.liste.Count);
            else
                status = string.Format(this.Resources.GetString(Resource.String.ShoppingListSummary_Positions), this.liste.Count);

            if (sum_quantity > 0) status += ", " + string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ShoppingListSummary_Quantity),     sum_quantity);
            if (sum_amount   > 0) status += ", " + string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ShoppingListSummary_Amount),       sum_amount, CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);
            if (sum_noPrice  == 1)
                status += ", " + string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ShoppingListSummary_WithoutPrice), sum_noPrice);
            if (sum_noPrice > 1)
                status += ", " + string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ShoppingListSummary_WithoutPriceN), sum_noPrice);

            if (to_pay > 0)       status += "\n" + string.Format(CultureInfo.CurrentUICulture, this.Resources.GetString(Resource.String.ShoppingListSummary_ToPay), to_pay, CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);

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

        private void ListAdapter_QuantityClicked(object sender, ShoppingListViewEventArgs ea)
        {
            var shoppingListView = ea.ShoppingListView;

            var quantityDialog = new AlertDialog.Builder(this);
            quantityDialog.SetTitle(shoppingListView.Heading);
            quantityDialog.SetMessage(this.Resources.GetString(Resource.String.App_EnterQuantity));
            EditText input = new EditText(this);
            input.InputType = InputTypes.ClassNumber | InputTypes.NumberFlagDecimal;

            if (shoppingListView.ShoppingItem.Quantity > 0)
            {
                input.Text = shoppingListView.ShoppingItem.Quantity.ToString();
            }
            input.SetSelection(input.Text.Length);
            quantityDialog.SetView(input);
            quantityDialog.SetPositiveButton(this.Resources.GetString(Resource.String.App_Ok), (dialog, whichButton) =>
                {
                    if (string.IsNullOrEmpty(input.Text))
                        input.Text = "0";

                    decimal neueAnzahl = 0;

                    bool decialOk = Decimal.TryParse(input.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out neueAnzahl);
                    if (decialOk)
                    {
                        shoppingListView.ShoppingItem.Quantity = neueAnzahl;
                        Database.SetShoppingItemQuantity(shoppingListView.ArticleId, neueAnzahl);
                    }
                });
            quantityDialog.SetNegativeButton(this.Resources.GetString(Resource.String.App_Cancel), (s, e) => {});
            quantityDialog.Show();
        }

        public bool OnQueryTextChange(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            this.textSearchDelayHandler.RemoveCallbacksAndMessages(null);

            this.textSearchDelayHandler.PostDelayed( () => 
                {
                    this.ShowShoppingList(filter);
                    this.lastSearchText = filter;
                },
                500);

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            //this.ShowShoppingList(filter);
            //this.lastSearchText = filter;
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