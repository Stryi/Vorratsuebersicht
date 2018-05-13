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

				if (this.Article.Size != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Menge: {0} {1}", this.Article.Size, this.Article.Unit);
				}
				if (this.Article.Calorie != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Kalorien: {0:n0}", this.Article.Calorie);
				}
				if (this.Article.DurableInfinity == false && this.Article.WarnInDays != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Warnen: {0} Tage(n) vor Ablauf", this.Article.WarnInDays);
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
                if (this.Article.Image == null)
                    return null;

                if (this.bitmp == null)
                {
                    Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (this.Article.Image, 0,this.Article.Image.Length);
                    
                    this.bitmp = unScaledBitmap;
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