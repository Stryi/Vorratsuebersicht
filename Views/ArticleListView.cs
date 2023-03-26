using System;
using System.Globalization;

using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using static Tools;

    public class ArticleListView
    {
        internal string String_Manufacturer  = String.Empty;
        internal string String_Category      = String.Empty;
        internal string String_Supermarket   = String.Empty;
        internal string String_Storage       = String.Empty;
        internal string String_WarnenInTagen = String.Empty;
        internal string String_Price         = String.Empty;
        internal string String_Size          = String.Empty;
        internal string String_Calories      = String.Empty;
        internal string String_Notes         = String.Empty;

        private Article Article {set; get;}
        
        public ArticleListView(Article article, Resources resources)
        {
            this.Article = article;

            this.String_Manufacturer  = resources.GetString(Resource.String.ArticleDetails_Manufacturer);
            this.String_Category      = resources.GetString(Resource.String.ArticleDetails_Category);
            this.String_Supermarket   = resources.GetString(Resource.String.ArticleDetails_SupermarketLabel);
            this.String_Storage       = resources.GetString(Resource.String.ArticleDetails_StorageLabel);
            this.String_WarnenInTagen = resources.GetString(Resource.String.ArticleDetails_WarningInDays);
            this.String_Price         = resources.GetString(Resource.String.ArticleDetails_Price);
            this.String_Size          = resources.GetString(Resource.String.ArticleDetails_Size);
            this.String_Calories      = resources.GetString(Resource.String.ArticleDetails_Calories);
            this.String_Notes         = resources.GetString(Resource.String.ArticleDetails_Notes);
        }

        public int ArticleId
        {
            get { return this.Article.ArticleId; }
        }

        public string Heading
        {
             get { return this.Article.Name; }
        }

        public string SubHeading
        {
             get
			{
				string info = string.Empty;

				if (!string.IsNullOrEmpty(this.Article.Manufacturer))
				{
                    info += this.String_Manufacturer;
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
					info += this.String_Category;
				    info += string.Format(" {0}", categoryText);
				}

                // Einkaufsmarkt

                if (!string.IsNullOrEmpty(this.Article.Supermarket))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += this.String_Supermarket;
                    info += string.Format(" {0}", this.Article.Supermarket);
                }

                // Lagert

                if (!string.IsNullOrEmpty(this.Article.StorageName))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += this.String_Storage;
                    info += string.Format(" {0}", this.Article.StorageName);
                }

                // Warnung in ... Tagen

				if (this.Article.DurableInfinity == false && this.Article.WarnInDays.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += this.String_WarnenInTagen;
					info += string.Format(" {0}", this.Article.WarnInDays.Value);
				}

                // Preis

				if (!string.IsNullOrEmpty(info)) info += "\n";
                info += this.String_Price;
                if (this.Article.Price.HasValue)
                {
					info += string.Format(CultureInfo.CurrentUICulture, " {0:n2}", this.Article.Price.Value);

                    string pricePerUnit = PricePerUnit.Calculate(this.Article.Price, this.Article.Size, this.Article.Unit);
                    if (!string.IsNullOrEmpty(pricePerUnit))
                    {
                        info += string.Format(" ({0})", pricePerUnit);
                    }
                }
                else
                {
					info += " -";
                }

				if (this.Article.Size.HasValue)
				{
				    if (!string.IsNullOrEmpty(info)) info += "\n";
                    info += this.String_Size;
					info += string.Format(CultureInfo.CurrentUICulture, " {0} {1}", this.Article.Size.Value, this.Article.Unit).TrimEnd();
				}

				if (!string.IsNullOrEmpty(info)) info += "\n";
                info += this.String_Calories;
				if (this.Article.Calorie.HasValue)
				{
					info += string.Format(" {0:n0}", this.Article.Calorie.Value);

                    if (this.Article.Calorie.HasValue && this.Article.Calorie.Value != 0)
                    {
                        string unitPerX = UnitConvert.GetConvertUnit(this.Article.Unit);

                        string calPerUnit = UnitConvert.GetCaloriePerUnit(
                            this.Article.Size?.ToString(),
                            this.Article.Unit,
                            this.Article.Calorie?.ToString());

                        if (calPerUnit != "---")
                        {
                            info += string.Format(" (100 {0} = {1})", unitPerX, calPerUnit);
                        }
                    }
				}
                else
                {
					info += " -";
                }

                return info;
			}
        }

        public string Notes
        {
             get
			{
				string info = string.Empty;

                if (!string.IsNullOrEmpty(this.Article.Notes))
                {
                    info += this.String_Notes;
                    info += string.Format(" {0}", this.Article.Notes);
                }

                return info;
			}
        }

        public decimal? shoppingQuantity;

        public bool IsOnShoppingList
        {
            get
            {
                if (this.shoppingQuantity == null)
                {
                    this.shoppingQuantity = Database.GetShoppingListQuantiy(this.Article.ArticleId, -1);
                }

                return this.shoppingQuantity >= 0;  // Menge 0 bedeutet: Auf EInkaufszettel, aber ohne Menge.
            }
        }

        public string ShoppingQuantity
        {
            get
            {
                if (!this.IsOnShoppingList)
                    return string.Empty;

                if (this.shoppingQuantity.Value == 0)
                    return string.Empty;

                return string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######}", this.shoppingQuantity.Value);
            }
        }


        private decimal? storageQuantity;

        public bool IsInStorage
        {
            get
            {
                if (this.storageQuantity == null)
                {
                    this.storageQuantity = Database.GetArticleQuantityInStorage(this.Article.ArticleId);
                }

                return this.storageQuantity > 0;
            }
        }

        public string StorageQuantity
        {
            get
            {
                if (!this.IsInStorage)
                    return "0";

                return string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######}", this.storageQuantity.Value);
            }
        }

        Bitmap bitmp;
        bool noImage = false;

        public Bitmap Image
        {
            get
            {
                if (this.bitmp != null)         // Image bereits erstellt
                    return this.bitmp;

                if (this.noImage)               // Kein Image definiert
                    return null;

                byte[] image = Database.GetArticleImage(this.ArticleId, false)?.ImageSmall;
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