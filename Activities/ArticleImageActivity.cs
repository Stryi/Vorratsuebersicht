using System;
using System.IO;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Android.Graphics;
using Android.Support.V4.Content;
using Android.Views;
using MatrixGuide;

namespace VorratsUebersicht
{
    [Activity(Label = "ArticleImageActivity")]
    public class ArticleImageActivity : Activity
    {
        int articleId;
        bool editMode;
        ImageView imageView;
        Bitmap rotatedBitmap;

        public static readonly int PickImageId = 1000;
        public static readonly int TakePhotoId = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ArticleImage);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);

            this.articleId = Intent.GetIntExtra    ("ArticleId", 0);
            this.editMode  = Intent.GetBooleanExtra("EditMode", false);
            string text    = Database.GetArticleName(this.articleId);

            this.imageView  = FindViewById<ImageView> (Resource.Id.ArticleImage_Image);

            ActionBar.Title = text;
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetIcon(Resource.Drawable.ic_photo_camera_white_24dp);

            if (ArticleDetailsActivity.imageLarge != null)
                this.ShowPictureFromBitmap();
            else
                this.ShowPictureFromDatabase();
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            if (this.editMode)
            {
                MenuInflater.Inflate(Resource.Menu.ArticleImage_menu, menu);
            }
            return base.OnCreateOptionsMenu(menu);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.ArticleImageMenu_RotateRight:
                    this.RotateImage();
                    return true;

                /*
                case Resource.Id.ArticleImageMenu_Save:
                    this.SaveBitmap();
                    this.OnBackPressed();
                    return true;

                case Resource.Id.ArticleImageMenu_Cancel:
                    this.OnBackPressed();
                    return true;
                */

                case Android.Resource.Id.Home:
                    this.SaveBitmap();
                    this.OnBackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void SaveBitmap()
        {
            if (this.rotatedBitmap == null)
                return;

            MemoryStream stream = new MemoryStream();
            this.rotatedBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            ArticleDetailsActivity.imageLarge = stream.ToArray();


            byte[] resizedImage = ImageResizer.ResizeImageAndroid(this.rotatedBitmap, 48*2, 85*2);

            Bitmap smallBitmap = BitmapFactory.DecodeByteArray(resizedImage, 0, resizedImage.Length);

            stream = new MemoryStream();
            smallBitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            ArticleDetailsActivity.imageSmall = stream.ToArray();

            Intent intent = new Intent();
            this.SetResult(Result.Ok, intent);

            this.rotatedBitmap = null;
        }

        private void ShowPictureFromDatabase()
        {
            if (this.articleId == 0)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);
                return;
            }

            Article article = Database.GetArticleImage(this.articleId, true);
            if (article == null)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                return;
            }

            if (article.Image == null)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                return;
            }

            string message = string.Empty;
            try
            {
                Bitmap image= BitmapFactory.DecodeByteArray (article.Image, 0, article.Image.Length);

                this.imageView.SetImageBitmap(image);

                message = string.Format("Bild: {0:n0} X {1:n0} (Größe: {2:n0}, Komprimiert: {3:n0})", 
                    image.Height, 
                    image.Width, 
                    Tools.ToFuzzyByteString(image.ByteCount),
                    Tools.ToFuzzyByteString(article.Image.Length));

                image = null;
            }
            catch (Exception e)
            {
                message = e.Message;
                this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
            }               

            FindViewById<TextView> (Resource.Id.ArticleImage_Info).Text = message;
            article = null;
        }

        private void ShowPictureFromBitmap()
        {
            Bitmap largeBitmap = BitmapFactory.DecodeByteArray(ArticleDetailsActivity.imageLarge, 0, ArticleDetailsActivity.imageLarge.Length);                
            this.rotatedBitmap = largeBitmap;
            this.imageView.SetImageBitmap(largeBitmap);
        }

        private void RotateImage()
        {
            if (this.rotatedBitmap == null)
            {
                Article article = Database.GetArticleImage(this.articleId, true);
                if (article == null)
                    return;

                if (article.Image == null)
                    return;
                try
                {
                    this.rotatedBitmap = BitmapFactory.DecodeByteArray (article.Image, 0, article.Image.Length);
                    GC.Collect();
                }
                catch(Exception ex)
                {
                    this.rotatedBitmap = null;
                    this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                    FindViewById<TextView> (Resource.Id.ArticleImage_Info).Text = ex.Message;
                    return;
                }
            }

            try
            {
                // Bild um 90 Grad drehen
                Matrix mat = new Matrix();
                mat.PostRotate(90);
                Bitmap bMapRotate = Bitmap.CreateBitmap(this.rotatedBitmap, 0, 0, this.rotatedBitmap.Width, this.rotatedBitmap.Height, mat, true);
                this.imageView.SetImageBitmap(bMapRotate);
                this.rotatedBitmap = bMapRotate;

                string message = string.Format("Bild: {0:n0} X {1:n0} (Größe: {2:n0})", 
                    this.rotatedBitmap.Height, 
                    this.rotatedBitmap.Width, 
                    Tools.ToFuzzyByteString(this.rotatedBitmap.ByteCount));

                FindViewById<TextView> (Resource.Id.ArticleImage_Info).Text = message;
            }
            catch(Exception ex)
            {
                this.rotatedBitmap = null;
                this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                FindViewById<TextView> (Resource.Id.ArticleImage_Info).Text = ex.Message;
            }
        }
    }
}