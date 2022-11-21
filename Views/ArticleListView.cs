using System;
using System.Globalization;

using Android.Graphics;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using static Tools;

    public class ArticleListView : ArticleListBaseView
    {

        private ArticleQuantity ArticleQuantity {set; get;}
        
        public ArticleListView(ArticleQuantity articleQuantity, Resources resources) : base (articleQuantity, resources)
        {
            this.ArticleQuantity = articleQuantity;
        }



        public bool IsOnShoppingList
        {
            get
            {
                return this.ArticleQuantity.ShoppingListQuantity >= 0; // Menge 0 bedeutet: Auf Einkaufszettel, aber ohne Menge.
            }
        }

        public string ShoppingQuantity
        {
            get
            {
                if (!this.IsOnShoppingList)
                    return string.Empty;

                return string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######}", this.ArticleQuantity.ShoppingListQuantity);
            }
        }

        public bool IsInStorage
        {
            get
            {
                return this.ArticleQuantity.StorageItemQuantity > 0;
            }
        }

        public string StorageQuantity
        {
            get
            {
                if (!this.IsInStorage)
                    return string.Empty;

                return string.Format(CultureInfo.CurrentUICulture, "{0:#,0.######}", this.ArticleQuantity.StorageItemQuantity);;
            }
        }

        public string CacheFileName
        {
            get
            {
                return string.Format("{0}_{1}_{2}", 
                    this.ArticleQuantity.ArticleId,
                    this.ArticleQuantity.ImageSmallLength,
                    this.ArticleQuantity.ImageLargeLength);
            }
        }
    }
}