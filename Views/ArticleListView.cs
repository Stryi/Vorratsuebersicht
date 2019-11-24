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
    using static Tools;

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
                    info += MainActivity.Strings_Manufacturer;
					info += string.Format(" {0}", this.Article.Manufacturer);
				}

                // Kategorie / Unterkategorie

                string categoryText = string.Empty;

				if (!string.IsNullOrEmpty(this.Article.Category))
				{
                    categoryText += this.Article.Category;
                }
				if (!string.IsNullOrEmpty(this.Article.SubCategory))
                {
					if (!string.IsNullOrEmpty(categoryText)) categoryText += " / ";

                    categoryText += this.Article.SubCategory;
                }

				if (!string.IsNullOrEmpty(categoryText))
				{
					if (!string.IsNullOrEmpty(info)) info += "\n";
					info += MainActivity.Strings_Category;
				    info += string.Format(" {0}", categoryText);
				}

                // Lagert

                if (!string.IsNullOrEmpty(this.Article.StorageName))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += MainActivity.Strings_Storage;
                    info += string.Format(" {0}", this.Article.StorageName);
                }

                // Warnung in ... Tagen

				if (this.Article.DurableInfinity == false && this.Article.WarnInDays.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += MainActivity.Strings_WarnenInTagen;
					info += string.Format(" {0}", this.Article.WarnInDays.Value);
				}

                // Preis

				if (!string.IsNullOrEmpty(info)) info += "\n";
                info += MainActivity.Strings_Price;
                if (this.Article.Price.HasValue)
                {
					info += string.Format(" {0}", this.Article.Price.Value);
                }
                else
                {
					info += " -";
                }

				if (this.Article.Size.HasValue)
				{
				    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += MainActivity.Strings_Size;
					info += string.Format(" {0} {1}", this.Article.Size.Value, this.Article.Unit).TrimEnd();
				}

				if (!string.IsNullOrEmpty(info)) info += "\n";
                info += MainActivity.Strings_Calories;
				if (this.Article.Calorie.HasValue)
				{
					info += string.Format(" {0:n0}", this.Article.Calorie.Value);
				}
                else
                {
					info += " -";
                }

                if (!string.IsNullOrEmpty(this.Article.Notes))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += MainActivity.Strings_Notes;
                    info += string.Format(" {0}", this.Article.Notes);
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

                byte[] image = Database.GetArticleImage(this.Id, false)?.ImageSmall;
                if (image == null)
                {
                    this.noImage = true;
                    return null;
                }

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                this.bitmp = unScaledBitmap;
                //TRACE("Article: {0}", this.Article.Name);

                return this.bitmp;
            }
        }
    }
}