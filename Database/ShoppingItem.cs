using System;
using SQLite;

namespace VorratsUebersicht
{
    public class ShoppingItem
    {
        [PrimaryKey] [AutoIncrement]
        public int ShoppingListId { get; set; }
        public int ArticleId { get; set; }
        public decimal Quantity { get; set; }
        public bool? Bought { get; set; }
    }
}