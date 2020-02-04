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
using Android.Graphics;

namespace VorratsUebersicht
{
    public class ArticleListViewAdapter : BaseAdapter<ArticleListView>
    {
        List<ArticleListView> items;
        Activity context;

        public ArticleListViewAdapter(Activity context, List<ArticleListView> items) : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override ArticleListView this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ArticleListView item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.ArticleListView, null);

            view.FindViewById<TextView> (Resource.Id.ArticleListView_Heading).Text    = item.Heading;
            view.FindViewById<TextView> (Resource.Id.ArticleListView_SubHeading).Text = item.SubHeading;
            view.FindViewById<ImageView>(Resource.Id.ArticleListView_OnShoppingList).Visibility   = item.IsOnShoppingList ? ViewStates.Visible : ViewStates.Invisible;
            view.FindViewById<TextView> (Resource.Id.ArticleListView_ShoppingQuantity).Visibility = item.IsOnShoppingList ? ViewStates.Visible : ViewStates.Invisible;
            view.FindViewById<TextView> (Resource.Id.ArticleListView_ShoppingQuantity).Text       = item.ShoppingQuantity;
            view.FindViewById<ImageView>(Resource.Id.ArticleListView_IsInStorage).Visibility      = item.IsInStorage ? ViewStates.Visible : ViewStates.Invisible;
            view.FindViewById<TextView> (Resource.Id.ArticleListView_StorageQuantity).Visibility  = item.IsInStorage ? ViewStates.Visible : ViewStates.Invisible;
            view.FindViewById<TextView> (Resource.Id.ArticleListView_StorageQuantity).Text        = item.StorageQuantity;

            ImageView image = view.FindViewById<ImageView>(Resource.Id.ArticleListView_Image);

            if (item.Image == null)
                image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
            else
                image.SetImageBitmap(item.Image);

            return view;
       }
    }
}