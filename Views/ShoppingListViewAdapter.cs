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
    public class ShoppingListViewAdapter : BaseAdapter<ShoppingListView>
    {
        List<ShoppingListView> items;
        Activity context;

        public ShoppingListViewAdapter(Activity context, List<ShoppingListView> items) : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override ShoppingListView this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.ArticleListView, null);
                ImageView imageView = view.FindViewById<ImageView>(Resource.Id.Image);
                imageView.Click += delegate
                {
                    if (item.Image == null)
                        return;

                    var articleImage = new Intent(context, typeof(ArticleImageActivity));
                    articleImage.PutExtra("Heading", item.Heading);
                    articleImage.PutExtra("ArticleId", item.ArticleId);
                    context.StartActivity(articleImage);
                };
            }

            view.FindViewById<TextView>(Resource.Id.Text1).Text = item.Heading;
            view.FindViewById<TextView>(Resource.Id.Text2).Text = item.SubHeading;
            view.FindViewById<TextView>(Resource.Id.Text3).Text = item.Information;
            view.FindViewById<TextView>(Resource.Id.Text3).Visibility = ViewStates.Visible;

            ImageView image = view.FindViewById<ImageView>(Resource.Id.Image);

            if (item.Image == null)
                image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
            else
                view.FindViewById<ImageView>(Resource.Id.Image).SetImageBitmap(item.Image);

            return view;
       }
    }
}