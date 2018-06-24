using System;
using System.Collections.Generic;

using Android.App;
using Android.OS;
using Android.Support.V4.Content;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Main_Button_Einkaufsliste", Icon = "@drawable/ic_shopping_cart_white_48dp")]
    public class ShoppingListActivity : Activity
    {
        List<ShoppingListView> liste = new List<ShoppingListView>();

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ShoppingItemList);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            this.ShowShoppingList();

            ListView listView = FindViewById<ListView>(Resource.Id.ShoppingItemList);
            listView.ItemClick += ListView_ItemClick;

        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            Java.Lang.Object itemObject = ((ListView)sender).GetItemAtPosition(e.Position);

            ShoppingListView listView = Tools.Cast<ShoppingListView>(itemObject);

            var message = new AlertDialog.Builder(this);
            message.SetMessage("Gekauft?");
            message.SetPositiveButton(Resource.String.App_Yes, (s, ev) =>
            {
                Database.RemoveFromShoppingList(listView.ShoppingListId);
                ShowShoppingList();
                //listView.InvalidateViews();
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