using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "Kategorie", Icon = "@drawable/ic_storage_white_48dp")]
    public class SubCategoryActivity : ListActivity
    {
        string category;
        string[] items;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            this.category = Intent.GetStringExtra ("Category") ?? string.Empty;

            this.items = Database.GetSubcategoriesOf(this.category);

            this.Title = this.category;

            this.ListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, items);

            //this.ListView.TextFilterEnabled = true;
        }

        protected override void OnListItemClick(ListView listView, View view, int position, long id)
        {
            string subCategory = ((TextView)view).Text;

            var storageitemList = new Intent (this, typeof(StorageItemListActivity));
            storageitemList.PutExtra("Category",    this.category);
            storageitemList.PutExtra("SubCategory", subCategory);
            StartActivity (storageitemList);
        }
    }
}