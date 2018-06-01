
using System;
using SQLite;

namespace VorratsUebersicht
{
    public class Article
    {
        [PrimaryKey] [AutoIncrement]
        public int ArticleId {get; set;}
        public string Name {get; set;}
        public string Manufacturer {get; set;}
        public string Category {get; set;}
        public string SubCategory {get; set;}
        public bool DurableInfinity {get; set;}
        public int? WarnInDays {get; set;}
        public decimal? Size {get; set; }
        public string Unit {get; set;}
		public int?   Calorie {get; set;}
        public string Notes {get; set;}
        public string EANCode {get; set;}
        public byte[] Image {get; set;}
        public byte[] ImageLarge {get; set;}
    }
}