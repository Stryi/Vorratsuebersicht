using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    [Activity(Label = "Kategorie", Icon = "@drawable/ic_storage_white_48dp")]
    public class SubCategoryActivity : ListActivity
    {
        string category;
        string[] items;
        string noSubCategory_ItemEntry;
        string anySubCategory_ItemEntry;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 60, 60);
            ActionBar.SetBackgroundDrawable(backgroundPaint);

            this.category = Intent.GetStringExtra ("Category") ?? string.Empty;

            this.noSubCategory_ItemEntry  = Resources.GetString(Resource.String.NoSubCategory_ItemEntry);
            this.anySubCategory_ItemEntry = Resources.GetString(Resource.String.AnySubCategory_ItemEntry);

            string[] subCategories = Database.GetSubcategoriesOf(this.category);
            
            this.items = new string[subCategories.Length +1];
            
            this.items[0] = this.anySubCategory_ItemEntry;

            for (int i = 1; i <= subCategories.Length; i++)
            {
                this.items[i] = subCategories[i-1];

                if (string.IsNullOrEmpty(this.items[i]))
                {
                    this.items[i] = this.noSubCategory_ItemEntry;
                }
            }

            this.Title = this.category;

            this.ListAdapter = new ArrayAdapter<String>(this, Android.Resource.Layout.SimpleListItem1, items);
        }

        protected override void OnListItemClick(ListView listView, View view, int position, long id)
        {
            string subCategory = ((TextView)view).Text;

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