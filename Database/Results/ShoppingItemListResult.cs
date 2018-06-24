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

namespace VorratsUebersicht
{
    /// <summary>
    /// Einkaufsliste mit Artikelangaben
    /// </summary>
    public class ShoppingItemListResult
    {
        public int ShoppingListId { get; set; }
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public decimal? Size { get; set; }
        public string Unit { get; set; }
        public int? Calorie { get; set; }
        public decimal Quantity { get; set; }
    }
}