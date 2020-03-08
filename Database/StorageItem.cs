using System;
using SQLite;

namespace VorratsUebersicht
{
	[Table("StorageItem")]
    public class StorageItem
    {
        [PrimaryKey] [AutoIncrement]
        public int StorageItemId  {get; set;}
        public int StorageId {get; set;}
        public int ArticleId {get; set;}
        public decimal Quantity {get; set; }
        public string BestBefore {get; set;}
        public string StorageName {get; set;}        
    }
}