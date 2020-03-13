using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    [DebuggerDisplay("{AnzahlText}, {BestBeforeText}, {LagerText}")]
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