using System;
using System.Diagnostics;

namespace VorratsUebersicht
{
    /// <summary>
    /// Einkaufsliste mit Artikelangaben
    /// </summary>
    [DebuggerDisplay("{Name}, {Quantity}")]
    public class ShoppingItemListResult
    {
        public int ShoppingListId { get; set; }
        public int ArticleId { get; set; }
        public string Name { get; set; }
        public string Manufacturer { get; set; }
        public string Supermarket { get; set; }
        public decimal? Size { get; set; }
        public string Unit { get; set; }
        public int? Calorie { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
        public decimal? Price { get; set; }
        public bool Bought { get; set; }
    }
}