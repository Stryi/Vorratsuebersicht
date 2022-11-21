using System;
using SQLite;

namespace VorratsUebersicht
{
    public class ArticleImage
    {
        [PrimaryKey] [AutoIncrement]
        public int ImageId {get; set;}
        public int ArticleId {get; set;}
        public int Type {get; set;}
        public DateTime CreatedAt {get; set;}
        public byte[] ImageSmall {get; set;}
        public byte[] ImageLarge {get; set;}
    }
}