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
    public class ArticleListView : ListItemViewBase
    {
        private bool imageSelected = false;

        public ArticleListView(Article article)
        {
            this.Article = article;
        }
        public Article Article {set; get;}

        public override int Id
        {
            get { return this.Article.ArticleId; }
        }

        public override string Heading
        {
             get { return this.Article.Name; }
        }
        public override string SubHeading
        {
             get
			{
				string info = string.Empty;

				if (!string.IsNullOrEmpty(this.Article.Manufacturer))
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Hersteller: {0}", this.Article.Manufacturer);
				}

				if (this.Article.Size.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Menge: {0} {1}", this.Article.Size.Value, this.Article.Unit);
				}
				if (this.Article.Calorie.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Kalorien: {0:n0}", this.Article.Calorie.Value);
				}
				if (this.Article.DurableInfinity == false && this.Article.WarnInDays.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Warnen: {0} Tage(n) vor Ablauf", this.Article.WarnInDays.Value);
				}

				if (!string.IsNullOrEmpty(this.Article.Category))
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Kategorie: {0}", this.Article.Category);
				}

				if (!string.IsNullOrEmpty(this.Article.SubCategory))
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Unterkategorie: {0}", this.Article.SubCategory);
				}

				return info;
			}
        }

        Bitmap bitmp;
        public override Bitmap Image
        {
            get
            {
                byte[] image = null;

                //if (!this.imageSelected)
                {
                    image = Database.GetArticleImage(this.Id, false).Image;
                    this.imageSelected = true;
                }

                if (image == null)
                    return null;

                if (this.bitmp == null)
                {
                    Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                    this.bitmp = unScaledBitmap;
                    System.Diagnostics.Trace.WriteLine(this.Article.Name);
                }

                return this.bitmp;
            }
            set
            {
                this.bitmp = value;
            }
        }

    }
}