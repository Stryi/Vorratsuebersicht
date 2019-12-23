using System;

using Android.App;
using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using static Tools;

    public class StorageItemListView : ListItemViewBase
    {
        public StorageItemListView(StorageItemQuantityResult storageItem)
        {
            this.StorageItem = storageItem;
        }
        public StorageItemListView()
        {
            this.StorageItem = new StorageItemQuantityResult();
        }
        public StorageItemQuantityResult StorageItem {set; get;}

        public override int Id
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

                info += MainActivity.Strings_Amount;
                info += string.Format(" {0}", this.StorageItem.Quantity);

                if (this.StorageItem.Size != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
                    info += MainActivity.Strings_Size;
					info += string.Format(" {0} {1}", this.StorageItem.Size, this.StorageItem.Unit).TrimEnd();
				}

                if (this.StorageItem.MinQuantity.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += MainActivity.Strings_MinQuantity;
                    info += string.Format(" {0}", this.StorageItem.MinQuantity);
                }

                if (this.StorageItem.PrefQuantity.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += MainActivity.Strings_PrefQuantity;
                    info += string.Format(" {0}", this.StorageItem.PrefQuantity);
                }

                if (!string.IsNullOrEmpty(this.StorageItem.StorageName))
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += MainActivity.Strings_Storage;
                    info += string.Format(" {0}", this.StorageItem.StorageName);
                }

                if (this.StorageItem.Price.HasValue)
                {
                    if (!string.IsNullOrEmpty(info)) info += "\r\n";
                    info += string.Format("Preis: {0:n2}", this.StorageItem.Price.Value);

                    if (this.StorageItem.Quantity > 1)
                    {
                        info += string.Format(" (Wert: {0:n2})", this.StorageItem.Quantity * this.StorageItem.Price.Value);
                    }
                }

                return info;
			}
        }

		public override int WarningLevel
        {
            get
            {
                return this.StorageItem.WarningLevel;
			}
		}

		public string WarningText
        {
            get
            {
                return this.StorageItem.BestBeforeWarningText;
			}
		}

		public string InfoText
        {
            get
            {
                return this.StorageItem.BestBeforeInfoText;
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
                TRACE("StorageItem Name {0}", this.StorageItem.Name);

                return this.bitmp;
            }
        }
    }
}