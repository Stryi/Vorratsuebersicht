using System;

using Android.App;
using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
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

                byte[] image = Database.GetArticleImage(this.Id, false).Image;
                if (image == null)
                {
                    this.noImage = true;
                    return null;
                }

                Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (image, 0, image.Length);

                this.bitmp = unScaledBitmap;
                System.Diagnostics.Trace.WriteLine(this.StorageItem.Name);

                return this.bitmp;
            }
        }
    }
}