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
    public class ShoppingListActivity : Activity
    {
        public static readonly int SelectArticleId = 1001;

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

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.ShoppingList_menu, menu);
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {

                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleList_Add:
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
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            ShoppingListView listView = Tools.Cast<ShoppingListView>(itemObject);

            string question = string.Format("Artikel '{0}' aus der Liste entfernen?", listView.Heading);

            var message = new AlertDialog.Builder(this);
            message.SetMessage(question);
            message.SetPositiveButton(Resource.String.App_Yes, (s, ev) =>
            {
                Database.RemoveFromShoppingList(listView.ShoppingListId);
                ShowShoppingList();
            });
            message.SetNegativeButton(Resource.String.App_No, (s, ev) => { });
            message.Create().Show();
        }

        private void ShowShoppingList()
        {
            this.liste = new List<ShoppingListView>();

            var shoppingList = Database.GetShoppingItemList();

            foreach (ShoppingItemListResult ShoppingItem in shoppingList)
            {
                this.liste.Add(new ShoppingListView(ShoppingItem));
            }

            ShoppingListViewAdapter listAdapter = new ShoppingListViewAdapter(this, this.liste);

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            listView.Adapter = listAdapter;
        }
    }
}