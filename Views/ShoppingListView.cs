using System.Diagnostics;
using System.Globalization;

using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using static Tools;

    [DebuggerDisplay("{ShoppingItem}")]
    public class ShoppingListView : ListItemViewBase
    {
        internal static int sparseView = 1;

        internal string String_Manufacturer = string.Empty;
        internal string String_Size         = string.Empty;
        internal string String_Supermarket  = string.Empty;
        internal string String_Price        = string.Empty;
        internal string String_Category     = string.Empty;
        internal string String_Notes        = string.Empty;
        internal string String_Amount       = string.Empty;

        public ShoppingListView(ShoppingItemListResult article, Resources resources)
        {
            this.String_Manufacturer = resources.GetString(Resource.String.ArticleDetails_Manufacturer);
            this.String_Size         = resources.GetString(Resource.String.ArticleDetails_Size);
            this.String_Supermarket  = resources.GetString(Resource.String.ArticleDetails_SupermarketLabel);
            this.String_Price        = resources.GetString(Resource.String.ArticleDetails_Price);
            this.String_Category     = resources.GetString(Resource.String.ArticleDetails_Category);
            this.String_Notes        = resources.GetString(Resource.String.ArticleDetails_Notes);
            this.String_Amount       = resources.GetString(Resource.String.ArticleDetails_Amount);

            this.ShoppingItem = article;
        }
        public ShoppingItemListResult ShoppingItem { set; get;}

        public int ShoppingListId
        {
            get { return this.ShoppingItem.ShoppingListId; }
        }

        public int ArticleId
        {
            get { return this.ShoppingItem.ArticleId; }
        }

        public override string Heading
        {
             get { return this.ShoppingItem.Name; }
        }
        public override string SubHeading
        {
             get
			{
				string info = string.Empty;

				if (!string.IsNullOrEmpty(this.ShoppingItem.Manufacturer))
				{
					if (!string.IsNullOrEmpty(info)) info += "\r\n";
					info += string.Format("{0} {1}", this.String_Manufacturer, this.ShoppingItem.Manufacturer);
				}

				if (this.ShoppingItem.Size.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += "\r\n";
					info += string.Format(CultureInfo.CurrentUICulture, "{0} {1} {2}", this.String_Size, this.ShoppingItem.Size.Value, this.ShoppingItem.Unit);
				}
                if (!string.IsNullOrEmpty(this.ShoppingItem.Supermarket))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format("{0} {1}", this.String_Supermarket, this.ShoppingItem.Supermarket);
                }
                if (this.ShoppingItem.Price.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format(CultureInfo.CurrentUICulture, "{0} {1} {2}", this.String_Price, this.ShoppingItem.Price.Value, CultureInfo.CurrentCulture.NumberFormat.CurrencySymbol);

                    string pricePerUnit = PricePerUnit.Calculate(this.ShoppingItem.Price, this.ShoppingItem.Size, this.ShoppingItem.Unit);
                    if (!string.IsNullOrEmpty(pricePerUnit))
                    {
                        info += string.Format(" ({0})", pricePerUnit);
                    }
                }

                if (ShoppingListView.sparseView < 1)
                {
                    string categoryText = string.Empty;

                    if (!string.IsNullOrEmpty(this.ShoppingItem.Category))
                    {
                        categoryText += this.ShoppingItem.Category;
                    }
                    if (!string.IsNullOrEmpty(this.ShoppingItem.SubCategory))
                    {
                        if (!string.IsNullOrEmpty(categoryText)) categoryText += " / ";

                        categoryText += this.ShoppingItem.SubCategory;
                    }


                    if (!string.IsNullOrEmpty(categoryText))
                    {
                        if (!string.IsNullOrEmpty(info)) info += "\r\n";
                        info += string.Format("{0} {1}", this.String_Category, categoryText);
                    }

                    if (!string.IsNullOrEmpty(this.ShoppingItem.Notes))
                    {
                        if (!string.IsNullOrEmpty(info)) info += "\r\n";
                        info += string.Format("{0} {1}", this.String_Notes, this.ShoppingItem.Notes);
                    }
                }
                return info;
			}
        }

        public string QuantityText
        {
            get
            {
                if (this.ShoppingItem.Quantity == 0)
                    return string.Empty;

                return  string.Format(CultureInfo.CurrentUICulture, "{0} {1}", this.String_Amount, this.ShoppingItem.Quantity);
            }
        }

        public decimal Quantity
        {
            get
            {
                return this.ShoppingItem.Quantity;
            }
        }

        public bool Bought
        {
            get 
            {
                return this.ShoppingItem.Bought;
            }
            set
            {
                this.ShoppingItem.Bought = value;
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

                byte[] image = Database.GetArticleImage(this.ArticleId, false)?.ImageSmall;
                if (image == null)
                {
                    this.noImage = true;
                    return null;
                }

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                this.bitmp = unScaledBitmap;
                //TRACE("Article: {0}", this.ShoppingItem.Name);

                return this.bitmp;
            }
        }
    }
}