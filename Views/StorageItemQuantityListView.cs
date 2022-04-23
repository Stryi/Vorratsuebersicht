using System;
using System.Diagnostics;
using System.Globalization;

using Android.Content.Res;

namespace VorratsUebersicht
{
    [DebuggerDisplay("{AnzahlText}, {BestBeforeText}, {LagerText}")]
    public class StorageItemQuantityListView : StorageItemListView
    {
        internal string String_BestBefore = string.Empty;

        public StorageItemQuantityListView(StorageItemQuantityResult storageItem, Resources resources) : base(storageItem, resources)
        { 
            this.String_BestBefore   = resources.GetString(Resource.String.StorageItemQuantityList_BestBefore);
        }

        public string AnzahlText
        {
             get { return string.Format(CultureInfo.CurrentUICulture, "{0} {1}", this.String_Quantity, this.StorageItem.Quantity); }
        }

        public string LagerText
        {
             get 
             {
                if (string.IsNullOrEmpty(this.StorageItem.StorageName))
                    return string.Empty;

                return string.Format("{0} {1}", this.String_Storage, this.StorageItem.StorageName); }
        }

        public string BestBeforeText
        {
            get
			{
				if (this.StorageItem.BestBefore == null)
					return string.Empty;

				return string.Format("{0} {1}", this.String_BestBefore, this.StorageItem.BestBefore.Value.ToShortDateString());
			}
        }
    }
}