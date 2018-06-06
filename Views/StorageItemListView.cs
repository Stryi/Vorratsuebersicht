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

				info = string.Format("Anzahl: {0}", this.StorageItem.Quantity);

				if (this.StorageItem.Size != 0)
				{
					if (!string.IsNullOrEmpty(info)) info += ", ";
					info += string.Format("Größe: {0} {1}", this.StorageItem.Size, this.StorageItem.Unit);
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
        public override Bitmap Image
        {
            get
            {
                if (this.StorageItem.Image == null)
                    return null;

                if (this.bitmp == null)
                {
                    Bitmap unScaledBitmap = BitmapFactory.DecodeByteArray (this.StorageItem.Image, 0,this.StorageItem.Image.Length);

                    this.bitmp = unScaledBitmap;
                    //this.bitmp = Bitmap.CreateScaledBitmap(unScaledBitmap, 45, 45, true);
                }
                
                return this.bitmp;
            }
        }
    }
}