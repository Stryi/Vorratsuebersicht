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
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Hersteller: {0}", this.ShoppingItem.Manufacturer);
				}

				if (this.ShoppingItem.Size.HasValue)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Größe: {0} {1}", this.ShoppingItem.Size.Value, this.ShoppingItem.Unit);
				}

				return info;
			}
        }

        public string Information
        {
            get
            {
                return  string.Format("Anzahl: {0}", this.ShoppingItem.Quantity);
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

                byte[] image = Database.GetArticleImage(this.ArticleId, false).Image;
                if (image == null)
                {
                    this.noImage = true;
                    return null;
                }

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                this.bitmp = unScaledBitmap;
                System.Diagnostics.Trace.WriteLine(this.ShoppingItem.Name);

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