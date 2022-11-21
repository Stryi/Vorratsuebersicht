using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using Android.Graphics;

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
            image.Tag = item.ArticleId;
            image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
            image.Alpha = 0.25f;

            if (string.IsNullOrEmpty(item.CacheFileName))
                return view;

            var dir = this.context.CacheDir;

            var cacheFileName = dir.AbsolutePath + "/" + item.CacheFileName + ".png";
            
            if (File.Exists(cacheFileName))
            {
                Bitmap bitmap = BitmapFactory.DecodeFile(cacheFileName);
                image.SetImageBitmap(bitmap);
                image.Alpha = 2f;
                return view;
            }

            new Thread(new ThreadStart(delegate
            {
                byte[] picture = null;

                try
                {
                    picture = Database.GetArticleImage(item.ArticleId, false)?.ImageSmall;
                }
                catch(Exception)
                {
                    this.context.RunOnUiThread( () =>
                    {
                        image.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                        image.Alpha = 0.5f;
                    });
                    return;
                }
                if (picture == null)
                {
                    return;
                }

                File.WriteAllBytes(cacheFileName, picture);

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (picture, 0, picture.Length);

                this.context.RunOnUiThread( () =>
                {
                    if (item.ArticleId == (int)image.Tag)
                    {
                        image.SetImageBitmap(unScaledBitmap);
                        image.Alpha = 1f;

                        image.Click += OnImageClicked;
                    }
                });
            })).Start();

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