using System;

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
        public string Supermarket { get; set; }
        public decimal? Size { get; set; }
        public string Unit { get; set; }
        public int? Calorie { get; set; }
        public decimal Quantity { get; set; }
        public string Notes { get; set; }
        public decimal? Price { get; set; }
    }
}