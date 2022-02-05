using System.Diagnostics;
using System.Globalization;

using Android.Graphics;

namespace VorratsUebersicht
{
    using static Tools;

    [DebuggerDisplay("{ShoppingItem}")]
    public class ShoppingListView : ListItemViewBase
    {
        public ShoppingListView(ShoppingItemListResult article)
        {
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
					info += string.Format("{0} {1}", MainActivity.Strings_Manufacturer, this.ShoppingItem.Manufacturer);
				}

				if (this.ShoppingItem.Size.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += "\r\n";
					info += string.Format(CultureInfo.CurrentUICulture, "{0} {1} {2}", MainActivity.Strings_Size, this.ShoppingItem.Size.Value, this.ShoppingItem.Unit);
				}
                if (!string.IsNullOrEmpty(this.ShoppingItem.Supermarket))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format("{0} {1}", MainActivity.Strings_Supermarket, this.ShoppingItem.Supermarket);
                }
                if (this.ShoppingItem.Price.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format(CultureInfo.CurrentUICulture, "{0} {1} €", MainActivity.Strings_Price, this.ShoppingItem.Price.Value);

                    string pricePerUnit = PricePerUnit.Calculate(this.ShoppingItem.Price, this.ShoppingItem.Size, this.ShoppingItem.Unit);
                    if (!string.IsNullOrEmpty(pricePerUnit))
                    {
                        info += string.Format(" ({0})", pricePerUnit);
                    }
                }
                if (!string.IsNullOrEmpty(this.ShoppingItem.Category))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format("{0} {1}", MainActivity.Strings_Category, this.ShoppingItem.Category);
                }

                if (!string.IsNullOrEmpty(this.ShoppingItem.Notes))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format("{0} {1}", MainActivity.Strings_Notes, this.ShoppingItem.Notes);
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

                return  string.Format(CultureInfo.CurrentUICulture, "Anzahl: {0}", this.ShoppingItem.Quantity);
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