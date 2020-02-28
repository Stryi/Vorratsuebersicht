using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public delegate void ShoppingListViewHandler(object sender, ShoppingListViewEventArgs e);

    public class ShoppingListViewEventArgs : EventArgs
    {
        public ShoppingListView ShoppingListView;

        public ShoppingListViewEventArgs(ShoppingListView view)
        {
            this.ShoppingListView = view;
        }
    }

    public class ShoppingListViewAdapter : BaseAdapter<ShoppingListView>
    {
        public event EventHandler CheckedChanged;
        public event ShoppingListViewHandler QuantityClicked;

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
                view = context.LayoutInflater.Inflate(Resource.Layout.ShoppingItemListView, null);
            }

            view.FindViewById<TextView>(Resource.Id.ShoppingItemListView_Heading).Text        = item.Heading;
            view.FindViewById<TextView>(Resource.Id.ShoppingItemListView_SubHeading).Text     = item.SubHeading;

            TextView quantity = view.FindViewById<TextView>(Resource.Id.ShoppingItemListView_Quantity);
            quantity.Text       = item.QuantityText;
            quantity.Visibility = ViewStates.Visible;
            quantity.Tag   =  position;
            quantity.Click -= OnQuantityClick;
            quantity.Click += OnQuantityClick;
            
            CheckBox bought = view.FindViewById<CheckBox>(Resource.Id.ShoppingItemListView_Bought);
            bought.Visibility = ViewStates.Visible;
            bought.Checked = item.Bought;
            bought.Tag     = position;
            bought.Click  -= OnBoughtClick;
            bought.Click  += OnBoughtClick;

            ImageView image = view.FindViewById<ImageView>(Resource.Id.ShoppingItemListView_Image);
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

            return view;
        }

        private void OnQuantityClick(object sender, EventArgs e)
        {
            TextView quantityView = (TextView)sender;
            int position = (int)quantityView.Tag;

            ShoppingListView shoppingItem = this[position];

            if (this.QuantityClicked == null)
                return;

            this.QuantityClicked.Invoke(this, new ShoppingListViewEventArgs(shoppingItem));
        }

        private void OnImageClicked(object sender, EventArgs e)
        {
            ImageView imageToView = (ImageView)sender;

            int articleId = (int)imageToView.Tag;

            var articleImage = new Intent(context, typeof(ArticleImageActivity));
            articleImage.PutExtra("ArticleId", articleId);
            context.StartActivity(articleImage);
        }

        private void OnBoughtClick(object sender, EventArgs e)
        {
            CheckBox checkBox = (CheckBox)sender;
            int position = (int)checkBox.Tag;

            ShoppingListView shoppingItem = this[position];
            shoppingItem.Bought = checkBox.Checked;
            
            Database.SetShoppingItemBought(shoppingItem.ArticleId, shoppingItem.Bought);

            // CheckChanged hier aufrufen, da beim OnCheckChanged die CheckBox noch nicht gesetzt war.
            if (this.CheckedChanged == null)
                return;

            this.CheckedChanged.Invoke(this, EventArgs.Empty);
        }
    }
}