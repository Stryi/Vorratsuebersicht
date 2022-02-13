using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public class StorageItemListViewAdapter : BaseAdapter<StorageItemListView>   // , IFilterable
    {
        public List<StorageItemListView> items;
        private Activity context;

        public event EventHandler OptionMenu;

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

            ImageView image   = view.FindViewById<ImageView>(Resource.Id.StorageItemListView_Image);
            TextView  header  = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextHeader);
            TextView  details = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextDetails);
            TextView  info    = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextInfo);
            TextView  warning = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextWarning);
            TextView  error   = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextError);
            ImageView inList  = view.FindViewById<ImageView>(Resource.Id.StorageItemListView_OnShoppingList);
            TextView  listQty = view.FindViewById<TextView >(Resource.Id.StorageItemListView_ShoppingQuantity);

            header.Text  = item.Heading;
            details.Text = item.SubHeading;
            inList.Visibility  = item.IsOnShoppingList ? ViewStates.Visible : ViewStates.Invisible;
            listQty.Visibility = item.IsOnShoppingList ? ViewStates.Visible : ViewStates.Invisible;
            listQty.Text       = item.ShoppingQuantity;

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

			if (!string.IsNullOrEmpty(item.ErrorText))
			{
	            error.Text    = item.ErrorText;
				error.Visibility = ViewStates.Visible;
			}
			else
			{
				error.Visibility = ViewStates.Gone;
			}

            image.Click -= OnImageClicked;

            if (item.Image == null)
            {
                image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                image.Alpha = 0.5f;
            }
            else
            {
                image.SetImageBitmap(item.Image);
                image.Alpha = 1f;

                image.Tag = item.ArticleId;
                image.Click += OnImageClicked;
            }

            TextView option = view.FindViewById<TextView>(Resource.Id.StorageItemListView_Option);
            option.Click -= Option_Click;
            option.Click += Option_Click;

            return view;
        }

        private void Option_Click(object sender, EventArgs e)
        {
            this.OptionMenu?.Invoke(sender, EventArgs.Empty);
        }

        private void OnImageClicked(object sender, EventArgs e)
        {
            ImageView imageToView = (ImageView)sender;

            int articleId = (int)imageToView.Tag;

            var articleImage = new Intent(context, typeof(ArticleImageActivity));
            articleImage.PutExtra("ArticleId", articleId);
            context.StartActivity(articleImage);
        }

    }
}