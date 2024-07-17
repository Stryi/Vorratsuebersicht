using System;
using System.Collections.Generic;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;
using static Android.Widget.AdapterView;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Main_Button_ArtikelListe", Icon = "@drawable/ic_local_offer_white_48dp")]
    public class ArticleListActivity : Activity, SearchView.IOnQueryTextListener
    {
        List<ArticleListView> liste = new List<ArticleListView>();
        private IParcelable listViewState;

		private bool selectArticleOnly;
        private string category;
        private string subCategory;
        private bool   withoutCategory;
        private bool   notInStorage;
        private bool   notInShoppingList;
        private string eanCode;
        private string lastSearchText = string.Empty;
        private int    specialFilter = 0;
        private List<string> categoryList;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.selectArticleOnly = Intent.GetBooleanExtra("SelectArticleOnly", false);

            this.category          = Intent.GetStringExtra ("Category") ?? string.Empty;
            this.subCategory       = Intent.GetStringExtra ("SubCategory") ?? string.Empty;
            this.notInStorage      = Intent.GetBooleanExtra("NotInStorage", false);
            this.notInShoppingList = Intent.GetBooleanExtra("NotInShoppingList", false);
            this.eanCode           = Intent.GetStringExtra ("EANCode") ?? string.Empty;

            if (!string.IsNullOrEmpty(this.subCategory))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.subCategory);
            }

            SetContentView(Resource.Layout.ArticleList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            this.categoryList = new List<string>();
            this.categoryList.Add(Resources.GetString(Resource.String.ArticleList_AllCategories));
            this.categoryList.Add(Resources.GetString(Resource.String.ArticleList_NoCategories));
            this.categoryList.AddRange(Database.GetCategoryAndSubCategoryNames());

            if (categoryList.Count > 1)
            {
                var categorySelection = FindViewById<LinearLayout>(Resource.Id.ArticleList_SelectCategory);
                categorySelection.Visibility = ViewStates.Visible;

                var spinnerCategory = FindViewById<Spinner>(Resource.Id.ArticleList_Categories);
                
                ArrayAdapter<String> dataAdapter = new ArrayAdapter<String>(this, Resource.Layout.Spinner_Black, this.categoryList);
                dataAdapter.SetDropDownViewResource(Android.Resource.Layout.SimpleSpinnerDropDownItem);
                spinnerCategory.Adapter = dataAdapter;

                spinnerCategory.ItemSelected += SpinnerCategory_ItemSelected;
            }

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.ItemClick += OnOpenArticleDetails;

            TextView articleListFilter = FindViewById<TextView>(Resource.Id.ArticleList_Filter);
            articleListFilter.Click += ArticleListFilter_Click;

            ImageView imageView = FindViewById<ImageView>(Resource.Id.ArticleList_FilterClear);
            imageView.Click += ArticleFilterClear_Click;

            ImageButton addButton = FindViewById<ImageButton>(Resource.Id.ArticleList_AddPosition);
            addButton.Click += AddArticle_Click;

            this.RegisterForContextMenu(listView);

            ShowArticleList();
        }

        private void ArticleListFilter_Click(object sender, EventArgs e)
        {
            this.FilterArticleList();
        }

        private void ArticleFilterClear_Click(object sender, EventArgs e)
        {
            this.specialFilter = 0;
            this.FindViewById<FrameLayout>(Resource.Id.ArticleList_FilterBanner).Visibility = ViewStates.Gone;

            this.ShowArticleList();
        }

        private void AddArticle_Click(object sender, EventArgs e)
        {
            // Create New Article
            this.ShowArticleDetails(0, null);
        }

        public override void OnCreateContextMenu(IContextMenu menu, View view, IContextMenuContextMenuInfo menuInfo)
        {
            if (view.Id == Resource.Id.ArticleList) 
            {
                menu.Add(IMenu.None, 1, IMenu.None, Resource.String.ArticleList_Lagerbestand);    // Lagerbestand
                menu.Add(IMenu.None, 2, IMenu.None, Resource.String.ArticleList_ToShoppingList);  // Auf Einkaufszettel
            }
        }

        public override bool OnContextItemSelected(IMenuItem item)
        {
            AdapterContextMenuInfo info = (AdapterContextMenuInfo) item.MenuInfo;

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            ArticleListView  selectedItem = Tools.Cast<ArticleListView>(listView.Adapter.GetItem(info.Position));
            
            switch(item.ItemId)
            {
                case 1: // Lagerbestand
                    var storageDetails = new Intent(this, typeof(StorageItemQuantityActivity));
                    storageDetails.PutExtra("ArticleId", selectedItem.ArticleId);

                    this.SaveListState();
                    this.StartActivityForResult(storageDetails, 20);

                    return true;

                case 2: // Auf Einkaufsliste
                    AddToShoppingListDialog.ShowDialog(
                        this,
                        selectedItem.ArticleId,
                        null, null, this.RefreshArticleList);

                    return true;

                default:
                    return base.OnContextItemSelected(item);
            }
        }

        private void SpinnerCategory_ItemSelected(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            string newCategoryName    = string.Empty;
            string newSubCategoryName = string.Empty;
            bool   withoutCategory    = false;

            string name = this.categoryList[e.Position];

            if (e.Position == 1)
            {
                withoutCategory = true;
            }

            if (e.Position > 1)
            {
                if (name.StartsWith("  - "))    // Ist das ein SubCategory?
                {
                    name = name.Substring(4);  // Mach aus "  - Gulasch" ein "Gulasch"
                    newSubCategoryName = name;
                }
                else
                {
                    newCategoryName = name;
                }
            }

            if ((newCategoryName != this.category) || (newSubCategoryName != this.subCategory) || withoutCategory != this.withoutCategory)
            {
                this.category        = newCategoryName;
                this.subCategory     = newSubCategoryName;
                this.withoutCategory = withoutCategory;

                this.ShowArticleList(this.lastSearchText);
            }
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ArticleList_menu, menu);

            var searchMenuItem = menu.FindItem(Resource.Id.ArticleList_Menu_Search);
            var searchView = (SearchView) searchMenuItem.ActionView;

            // https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            searchView.SetOnQueryTextListener(this);

            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;

                case Resource.Id.ArticleList_Menu_Add:
                    // Create New Article
                    this.ShowArticleDetails(0, null);
                    return true;

                case Resource.Id.ArticleList_Menu_Filter:

                    this.FilterArticleList();
                    break;

                case Resource.Id.ArticleList_Menu_Share:
                    this.ShareList();
                    return true;
            }
            return true;
        }

        private void FilterArticleList()
        {
            string[] actions = Resources.GetStringArray(Resource.Array.ArticleListeSpecialFilter);

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetItems(actions, (sender2, args) =>
            {
                this.specialFilter = args.Which + 1;

                string filterText = this.Resources.GetString(Resource.String.ArticleList_SpecialFilter);
                filterText = String.Format(filterText, actions[args.Which]);

                this.FindViewById<FrameLayout>(Resource.Id.ArticleList_FilterBanner).Visibility = ViewStates.Visible;
                this.FindViewById<TextView>(Resource.Id.ArticleList_Filter).Text = filterText;

                this.ShowArticleList();
            });
            builder.Show();
        }

        private void ShareList()
        {
            if (MainActivity.IsGooglePlayPreLaunchTestMode)
            {
                return;
            }

            string text = string.Empty;

            foreach(ArticleListView view in this.liste)
            {
                if (!string.IsNullOrEmpty(view.Heading))     text += view.Heading     + "\n";
                if (!string.IsNullOrEmpty(view.SubHeading))  text += view.SubHeading  + "\n";
                if (!string.IsNullOrEmpty(view.Notes))       text += view.Notes + "\n";
                text += "\n";
            }

            TextView footer = FindViewById<TextView>(Resource.Id.ArticleList_Footer);
            text += footer.Text;

            string subject = string.Format("{0} - {1} {2}",
                Resources.GetString(Resource.String.Main_Button_Artikelangaben),
                DateTime.Now.ToShortDateString(),
                DateTime.Now.ToShortTimeString());

            Intent intentsend = new Intent();
            intentsend.SetAction(Intent.ActionSend);
            intentsend.PutExtra(Intent.ExtraSubject, subject);
            intentsend.PutExtra(Intent.ExtraText, text);
            intentsend.SetType("text/plain");
            
            StartActivity(intentsend);
        }

        private void RefreshArticleList()
        {
            this.SaveListState();
            this.ShowArticleList(this.lastSearchText);
            this.RestoreListState();
        }

        private void ShowArticleList(string text = null)
        {
            this.liste = new List<ArticleListView>();

            var articleList = Database.GetArticleList(
                this.category,
                this.subCategory,
                this.eanCode,
                this.notInStorage,
                this.notInShoppingList,
                this.withoutCategory,
                this.specialFilter,
                text);

            foreach(Article article in articleList)
            {
                liste.Add(new ArticleListView(article, this.Resources));
            }

            ArticleListViewAdapter listAdapter = new ArticleListViewAdapter(this, liste);
            listAdapter.OptionMenu += ListAdapter_OptionMenu;

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.Adapter = listAdapter;

            string status;

            if (articleList.Count == 1)
                status = string.Format(this.Resources.GetString(Resource.String.ArticleListSummary_Position), articleList.Count);
            else
                status = string.Format(this.Resources.GetString(Resource.String.ArticleListSummary_Positions), articleList.Count);

            TextView footer = FindViewById<TextView>(Resource.Id.ArticleList_Footer);
            footer.Text = status;
        }

        private void ListAdapter_OptionMenu(object sender, EventArgs e)
        {
            this.OpenContextMenu((TextView)sender);
        }

        private void OnOpenArticleDetails(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            ArticleListView item = Tools.Cast<ArticleListView>(itemObject);

			// Nur Artikelauswahl, keine Detailsbearbeitung
			if (this.selectArticleOnly)
			{
                Intent intent = new Intent();
				intent.PutExtra("Heading",   item.Heading);
				intent.PutExtra("ArticleId", item.ArticleId);

                this.SetResult(Result.Ok, intent);

                this.Finish();
				return;
			}

            this.ShowArticleDetails(item.ArticleId, item.Heading);
        }

        private void SaveListState()
        {
            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            this.listViewState = listView.OnSaveInstanceState();
        }

        private void RestoreListState()
        {
            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView?.OnRestoreInstanceState(this.listViewState);
        }

        private void ShowArticleDetails(int articleId, string name)
        {
            var articleDetails = new Intent (this, typeof(ArticleDetailsActivity));
            articleDetails.PutExtra("Name", name);
            articleDetails.PutExtra("ArticleId", articleId);

            // Zum voranstellen
            articleDetails.PutExtra("Category",    this.category);
            articleDetails.PutExtra("SubCategory", this.subCategory);

            StartActivityForResult(articleDetails, 10);

            this.SaveListState();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                this.ShowArticleList(this.lastSearchText);

                if (this.listViewState == null)
                    return;

                this.RestoreListState();    
            }
        }

        public bool OnQueryTextChange(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            ShowArticleList(filter);
            this.lastSearchText = filter;
            return true;
        }

        public bool OnQueryTextSubmit(string filter)
        {
            if (this.lastSearchText == filter)
                return true;

            // Filter ggf. mit Adapter, siehe https://coderwall.com/p/zpwrsg/add-search-function-to-list-view-in-android
            ShowArticleList(filter);
            this.lastSearchText = filter;
            return true;
        }

    }
}