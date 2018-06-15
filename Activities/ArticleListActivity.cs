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
    [Activity(Label = "@string/Main_Button_ArtikelListe", Icon = "@drawable/ic_local_offer_white_48dp")]
    public class ArticleListActivity : Activity
    {
        List<ArticleListView> liste = new List<ArticleListView>();
        private IParcelable listViewState;

		private bool selectArticleOnly;
        private string category;
        private string subCategory;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.selectArticleOnly = Intent.GetBooleanExtra("SelectArticleOnly", false);

            this.category    = Intent.GetStringExtra ("Category") ?? string.Empty;
            this.subCategory = Intent.GetStringExtra ("SubCategory") ?? string.Empty;

            if (!string.IsNullOrEmpty(this.subCategory))
            {
                this.Title = string.Format("{0} - {1}", this.Title, this.subCategory);
            }

            SetContentView(Resource.Layout.ArticleList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            ShowArticleList();

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.ItemClick += OnOpenArticleDetails;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ArticleList_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.ArticleList_Add:
                    // Create New Article
                    this.ShowArticleDetails(0, null);

                    return true;
            }
            return true;
        }

        private void ShowArticleList()
        {
            this.liste = new List<ArticleListView>();

            var articleList = Database.GetArticleListNoImages(this.category, this.subCategory);

            foreach(Article article in articleList)
            {
                liste.Add(new ArticleListView(article));
            }

            ArticleListViewAdapter listAdapter = new ArticleListViewAdapter(this, liste);

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            listView.Adapter = listAdapter;
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
				intent.PutExtra("ArticleId", item.Id);

                this.SetResult(Result.Ok, intent);

                this.OnBackPressed();
				return;
			}

            this.ShowArticleDetails(item.Id, item.Heading);
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

            ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
            this.listViewState = listView.OnSaveInstanceState();
        }
        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok)
            {
                this.ShowArticleList();

                ListView listView = FindViewById<ListView>(Resource.Id.ArticleList);
                listView.OnRestoreInstanceState(this.listViewState);
            }
        }
    }
}