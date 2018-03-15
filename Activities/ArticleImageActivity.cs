using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Graphics;
using Android.Provider;
using MatrixGuide;

namespace VorratsUebersicht
{
    [Activity(Label = "ArticleImageActivity")]
    //[Activity(Label = "ArticleImageActivity", MainLauncher = true)]
    
    public class ArticleImageActivity : Activity
    {
        int articleId;
        ImageView imageView;

        public static readonly int PickImageId = 1000;
        public static readonly int TakePhotoId = 1001;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.ArticleImage);

            string text    = Intent.GetStringExtra ("Heading") ?? string.Empty;
            this.articleId = Intent.GetIntExtra    ("ArticleId", 0);
            bool large     = Intent.GetBooleanExtra("Large", true);

            this.imageView  = FindViewById<ImageView> (Resource.Id.ArticleImage_Image);

            ActionBar.Title = text;
            ActionBar.SetHomeButtonEnabled(true);
            ActionBar.SetIcon(Resource.Drawable.ic_photo_camera_white_24dp);

            this.ShowPictureFromDatabase(large);
        }

        private void ShowPictureFromDatabase(bool showLarge)
        {
            if (this.articleId == 0)
            {
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_white_24dp);
                return;
            }

            Article article = Database.GetArticleImage(this.articleId, showLarge);
            if (article == null)
                return;

            if (article.Image != null)
            {
                try
                {
                    Bitmap image= BitmapFactory.DecodeByteArray (article.Image, 0, article.Image.Length);
                    this.imageView.SetImageBitmap(image);

                    string text = string.Format("Bild: {0:n0} X {1:n0} ({2:n0})", image.Height, image.Width, Tools.ToFuzzyByteString(image.ByteCount));
                    FindViewById<TextView> (Resource.Id.ArticleImage_Info).Text = text;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Trace.WriteLine(e.ToString());
                    this.imageView.SetImageResource(Resource.Drawable.ic_broken_image_white_24dp);
                }               
            }
            else
                this.imageView.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
        }
    }
}