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
    public class StorageItemQuantityListView : StorageItemListView
    {
        public StorageItemQuantityListView(StorageItemQuantityResult storageItem) : base(storageItem)  { }

        public string AnzahlText
        {
             get { return string.Format("{0} {1}", MainActivity.Strings_Amount, this.StorageItem.Quantity); }
        }

        public string LagerText
        {
             get 
             {
                if (string.IsNullOrEmpty(this.StorageItem.StorageName))
                    return string.Empty;

                return string.Format("Lagerort: {0}", this.StorageItem.StorageName); }
        }

        public string BestBeforeText
        {
            get
			{
				if (this.StorageItem.BestBefore == null)
					return string.Empty;

				return string.Format("Ablaufdatum: {0}", this.StorageItem.BestBefore.Value.ToShortDateString());
			}
        }
    }
}