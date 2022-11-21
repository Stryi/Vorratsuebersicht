using System;
using System.Globalization;

using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using static Tools;

    public class StorageItemListView : ListItemViewBase
    {
        internal string String_Quantity     = String.Empty;
        internal string String_Size         = String.Empty;
        internal string String_MinQuantity  = String.Empty;
        internal string String_PrefQuantity = String.Empty;
        internal string String_Storage      = String.Empty;
        internal string String_Price        = String.Empty;
        internal string String_Amount       = String.Empty;
        internal string String_Sum          = String.Empty;

        public StorageItemListView(StorageItemQuantityResult storageItem, Resources resources)
        {
            this.StorageItem = storageItem;

            this.String_Quantity     = resources.GetString(Resource.String.StorageItemQuantityList_Quantity);
            this.String_Storage      = resources.GetString(Resource.String.StorageItemQuantityList_StorageLabel);
            this.String_Size         = resources.GetString(Resource.String.ArticleDetails_Size);
            this.String_MinQuantity  = resources.GetString(Resource.String.ArticleDetails_MinQuantityLabel);
            this.String_PrefQuantity = resources.GetString(Resource.String.ArticleDetails_PrefQuantityLabel);
            this.String_Price        = resources.GetString(Resource.String.ArticleDetails_Price);
            this.String_Amount       = resources.GetString(Resource.String.StorageItem_Amount);
            this.String_Sum          = resources.GetString(Resource.String.StorageItem_SumQuantityAndSize);
        }
        public StorageItemListView()
        {
            this.StorageItem = new StorageItemQuantityResult();
        }
        public StorageItemQuantityResult StorageItem {set; get;}

        public int ArticleId
        {
            get { return this.StorageItem.ArticleId; }
        }

        public override string Heading
        {
             get { return this.StorageItem.Name; }
        }

        public override string SubHeading
        {
            get
			{
				string info = string.Empty;

                info += this.String_Quantity;
                info += string.Format(CultureInfo.CurrentUICulture, " {0}", this.StorageItem.Quantity);

                if (this.StorageItem.Size != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += this.String_Size;
					info += string.Format(CultureInfo.CurrentUICulture, " {0} {1}", this.StorageItem.Size, this.StorageItem.Unit).TrimEnd();
                    if ((this.StorageItem.Quantity != 1) && (this.StorageItem.Size != 1))
                    {
                        // TODO: Menge ggf. umrechnen, z.B. "2.500 g" als "2,5 Kg"
                        info += "\r\n";
                        info += string.Format(CultureInfo.CurrentUICulture, "{0} {1:#,0.######} {2}", this.String_Sum, (this.StorageItem.Size * this.StorageItem.Quantity), this.StorageItem.Unit).TrimEnd();
                    }
				}

                if (this.StorageItem.MinQuantity.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += this.String_MinQuantity;
                    info += string.Format(" {0}", this.StorageItem.MinQuantity);
                }

                if (this.StorageItem.PrefQuantity.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += this.String_PrefQuantity;
                    info += string.Format(" {0}", this.StorageItem.PrefQuantity);
                }

                if (!string.IsNullOrEmpty(this.StorageItem.StorageName))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += this.String_Storage;
                    info += string.Format(" {0}", this.StorageItem.StorageName);
                }

                if (this.StorageItem.Price.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format(CultureInfo.CurrentUICulture, "{0} {1:n2}", this.String_Price, this.StorageItem.Price.Value);
                    info += string.Format(CultureInfo.CurrentUICulture, " ({0} {1:n2})", this.String_Amount, this.StorageItem.Quantity * this.StorageItem.Price.Value);
                }

                return info;
			}
        }

        public bool IsOnShoppingList
        {
            get
            {
                if (this.StorageItem.ShoppingListQuantity == null)
                    return false;

                return true;
            }
        }

        public string ShoppingQuantity
        {
            get
            {
                if (this.StorageItem.ShoppingListQuantity == null)
                    return string.Empty;

                if (this.StorageItem.ShoppingListQuantity.Value == 0)
                    return string.Empty;

                return string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######}", this.StorageItem.ShoppingListQuantity.Value);
            }
        }

		public override int WarningLevel
        {
            get
            {
                return this.StorageItem.WarningLevel;
			}
		}

		public string InfoText
        {
            get
            {
                return this.StorageItem.BestBeforeInfoText;
			}
		}

		public string ErrorText
        {
            get
            {
                return this.StorageItem.BestBeforeErrorText;
			}
		}

		public string WarningText
        {
            get
            {
                return this.StorageItem.BestBeforeWarningText;
			}
		}

        public override Color WarningColor
        {
            get
            {
                switch(this.StorageItem.WarningLevel)
                {
                    case 1: return Color.Blue;
                    case 2: return Color.Red;
                }
                return Color.Black;
            }

        }

        public string CacheFileName
        {
            get
            {
                return string.Format("{0}_{1}_{2}", 
                    this.StorageItem.ArticleId,
                    this.StorageItem.ImageSmallLength,
                    this.StorageItem.ImageLargeLength);
            }
        }

    }
}