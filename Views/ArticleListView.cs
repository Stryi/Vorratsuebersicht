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
                    info += MainActivity.Strings_Manufacturer;
					info += string.Format(" {0}", this.Article.Manufacturer);
				}

				if (this.Article.Size.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += MainActivity.Strings_Size;
					info += string.Format(" {0} {1}", this.Article.Size.Value, this.Article.Unit).TrimEnd();
				}

				if (this.Article.Calorie.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += MainActivity.Strings_Calories;
					info += string.Format(" {0:n0}", this.Article.Calorie.Value);
				}

				if (this.Article.DurableInfinity == false && this.Article.WarnInDays.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += MainActivity.Strings_WarnenInTagen;
					info += string.Format(" {0}", this.Article.WarnInDays.Value);
				}

				if (!string.IsNullOrEmpty(this.Article.Category))
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += MainActivity.Strings_Category;
				    info += string.Format(" {0}", this.Article.Category);
				}

				if (!string.IsNullOrEmpty(this.Article.SubCategory))
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += MainActivity.Strings_SubCategory;
				    info += string.Format(" {0}", this.Article.SubCategory);
				}

                if (!string.IsNullOrEmpty(this.Article.StorageName))
                {
                    if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += MainActivity.Strings_Storage;
                    info += string.Format(" {0}", this.Article.StorageName);
                }

                return info;
			}
        }

        Bitmap bitmp;
        bool noImage = false;

        public override Bitmap Image
        {
            get
            {
                if (this.bitmp != null)         // Image bereits erstellt
                    return this.bitmp;

                if (this.noImage)               // Kein Image definiert
                    return null;

                byte[] image = Database.GetArticleImage(this.Id, false).Image;
                if (image == null)
                {
                    this.noImage = true;
                    return null;
                }

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                this.bitmp = unScaledBitmap;
                System.Diagnostics.Trace.WriteLine(this.Article.Name);

                return this.bitmp;
            }
            /*
            set
            {
                this.bitmp = value;
            }
            */
        }

    }
}