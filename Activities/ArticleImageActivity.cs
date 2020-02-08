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
using System.Threading;

namespace VorratsUebersicht
{
    [Activity(Label = "ArticleImageActivity")]
    public class ArticleImageActivity : Activity
    {
        int articleId;
        bool editMode;
        ImageView imageView;
        TextView  imageInfo;
        Bitmap rotatedBitmap;
        bool isChanged = false;

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
            this.imageView.Click += OnClickImage;

            this.imageInfo = FindViewById<TextView>(Resource.Id.ArticleImage_Info);

            ActionBar.Title = text;
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetIcon(Resource.Drawable.ic_photo_camera_white_24dp);

            if (ArticleDetailsActivity.imageLarge != null)
                this.ShowPictureFromBitmap();
            else
                this.ShowPictureFromDatabase();
        }

        private void OnClickImage(object sender, EventArgs e)
        {
            if (this.imageInfo.Visibility != ViewStates.Visible)
            {
                this.imageInfo.Visibility = ViewStates.Visible;
            }
            else
            {
                this.imageInfo.Visibility = ViewStates.Gone;
            }
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

                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override void OnBackPressed()
        {
            if (!this.isChanged)
            {
                base.OnBackPressed();
                return;
            }

            var progressDialog = this.CreateProgressBar();
            new Thread(new ThreadStart(delegate             
            {
                this.SaveBitmap();
                GC.Collect();
                
                RunOnUiThread(() => base.OnBackPressed());

                this.HideProgressBar(progressDialog);
            })).Start();
        }

        private void SaveBitmap()
        {
            if (this.rotatedBitmap == null)
                return;

            if (!this.isChanged)
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
            this.isChanged = false;
        }

        private void ShowPictureFromDatabase()
        {
            if (this.articleId == 0)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);
                this.imageView.Alpha = 1f;
                return;
            }

            ArticleImage article = Database.GetArticleImage(this.articleId, true);
            if (article == null)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                this.imageView.Alpha = 0.5f;

                return;
            }

            if (article.ImageLarge == null)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
                this.imageView.Alpha = 0.5f;

                return;
            }

            string message = string.Empty;
            try
            {
                Bitmap image= BitmapFactory.DecodeByteArray (article.ImageLarge, 0, article.ImageLarge.Length);

                this.imageView.SetImageBitmap(image);

                message = string.Format("Bild: {0:n0} X {1:n0} (Größe: {2:n0}, Komprimiert: {3:n0})", 
                    image.Height, 
                    image.Width, 
                    Tools.ToFuzzyByteString(image.ByteCount),
                    Tools.ToFuzzyByteString(article.ImageLarge.Length));

                image = null;
            }
            catch (Exception e)
            {
                message = e.Message;
                this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                this.imageView.Alpha = 1f;
            }               

            this.imageInfo.Text = message;
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
                ArticleImage article = Database.GetArticleImage(this.articleId, true);
                if (article == null)
                    return;

                if (article.ImageLarge == null)
                    return;
                try
                {
                    this.rotatedBitmap = BitmapFactory.DecodeByteArray (article.ImageLarge, 0, article.ImageLarge.Length);
                    GC.Collect();
                }
                catch(Exception ex)
                {
                    this.rotatedBitmap = null;
                    this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                    this.imageView.Alpha = 1f;
                    this.imageInfo.Text = ex.Message;
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
                this.isChanged = true;

                string message = string.Format("Bild: {0:n0} X {1:n0} (Größe: {2:n0})", 
                    this.rotatedBitmap.Height, 
                    this.rotatedBitmap.Width, 
                    Tools.ToFuzzyByteString(this.rotatedBitmap.ByteCount));

                this.imageInfo.Text = message;

            }
            catch(Exception ex)
            {
                this.rotatedBitmap = null;
                this.imageView.SetImageResource(Resource.Drawable.baseline_error_outline_black_24);
                this.imageView.Alpha = 1f;
                this.imageInfo.Text = ex.Message;
            }
        }

        private ProgressBar CreateProgressBar()
        {
            var progressBar = FindViewById<ProgressBar>(Resource.Id.ArticleImage_ProgressBar);
            progressBar.Visibility = ViewStates.Visible;
            this.Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
            return progressBar;
        }

        private void HideProgressBar(ProgressBar progressBar)
        {
            RunOnUiThread(() =>
            {
                progressBar.Visibility = ViewStates.Invisible;
                this.Window.ClearFlags(WindowManagerFlags.NotTouchable);
            });
        }
   }
}