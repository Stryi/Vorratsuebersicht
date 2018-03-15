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
using System.Diagnostics;

namespace VorratsUebersicht
{
    public class StorageItemListViewAdapter : BaseAdapter<StorageItemListView>
    {
        List<StorageItemListView> items;
        Activity context;

        public StorageItemListViewAdapter(Activity context, List<StorageItemListView> items) : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override StorageItemListView this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            StorageItemListView item = items[position];
            View view = convertView;

            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.StorageItemListView, null);

            ImageView image   = view.FindViewById<ImageView>(Resource.Id.Image);
            TextView  header  = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextHeader);
            TextView  details = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextDetails);
            TextView  info    = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextInfo);
            TextView  warning = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextWarning);

            header.Text  = item.Heading;
            details.Text = item.SubHeading;

			if (!string.IsNullOrEmpty(item.InfoText))
			{
	            info.Text    = item.InfoText;
				info.Visibility = ViewStates.Visible;
			}
			else
			{
				info.Visibility = ViewStates.Gone;
			}
			if (!string.IsNullOrEmpty(item.WarningText))
			{
	            warning.Text    = item.WarningText;
				warning.Visibility = ViewStates.Visible;
			}
			else
			{
				warning.Visibility = ViewStates.Gone;
			}

            if (item.Image == null)
                image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
            else
                image.SetImageBitmap(item.Image);

            //if (item.WarningLevel > 0)
            //    details.SetTextColor(item.WarningColor);
            //else
            //    details.SetTextColor(Color.Gray);
            
            return view;
       }
    }
}