using System;
using System.Collections.Generic;
using System.Globalization;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Support.V7.App;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "@string/Main_Button_BestandNachKategorie", Icon = "@drawable/ic_storage_white_48dp")]
    public class SubCategoryActivity : Activity
    {
        string category;
        List<SimpleListItem2View> items;

        string noSubCategory_ItemEntry;
        string anySubCategory_ItemEntry;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 60, 60);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            // Soll noch umgestellt werden
            SetContentView(Resource.Layout.SubCategoryActivity);

            this.category = Intent.GetStringExtra ("Category") ?? string.Empty;

            this.noSubCategory_ItemEntry  = Resources.GetString(Resource.String.NoSubCategory_ItemEntry);
            this.anySubCategory_ItemEntry = Resources.GetString(Resource.String.AnySubCategory_ItemEntry);

            List<string> subCategories = Database.GetSubcategoriesOf(this.category);
            
            this.items = new List<SimpleListItem2View>();
            
            var item = new SimpleListItem2View()
            {
                Heading = this.anySubCategory_ItemEntry
            };

            items.Add(item);

            for (int i = 1; i <= subCategories.Count; i++)
            {
                item = new SimpleListItem2View()
                {
                    Heading = subCategories[i-1]
                };
                
                if (string.IsNullOrEmpty(item.Heading))
                {
                    item.Heading = this.noSubCategory_ItemEntry;
                }
                items.Add(item);
            }

            this.Title = this.category;

            var listView = FindViewById<ListView>(Resource.Id.SubCategoryList);
            var listAdapter = new SimpleListItem2Adapter(this, this.items, Android.Resource.Layout.SimpleListItem1);
            listView.Adapter = listAdapter;

            listView.ItemClick += OnListItemClick;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.Finish();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void OnListItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            SimpleListItem2View item = Tools.Cast<SimpleListItem2View>(itemObject);

            string subCategory = item.Heading;

            if (subCategory == this.noSubCategory_ItemEntry)
            {
                subCategory = string.Empty;
            }

            if (subCategory == this.anySubCategory_ItemEntry)
            {
                subCategory = null;
            }

            var storageitemList = new Intent (this, typeof(StorageItemListActivity));
            storageitemList.PutExtra("Category",    this.category);
            storageitemList.PutExtra("SubCategory", subCategory);
            StartActivity (storageitemList);
        }
    }
}