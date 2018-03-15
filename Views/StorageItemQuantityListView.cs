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

        public override string Heading
        {
             get { return string.Format("Anzahl: {0}", this.StorageItem.Quantity); }
        }

        public override string SubHeading
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