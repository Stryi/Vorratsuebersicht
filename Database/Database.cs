using System;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using System.Diagnostics;

using SQLite;

namespace VorratsUebersicht
{
    using static Tools;

    public static class Database
    {
        internal static void UpdateStorageItemQuantity(StorageItemQuantityResult storageItem)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            string cmd = string.Empty;
            SQLiteCommand command;

            int defaultStorageId = 1;

            if (storageItem.StorageItemId == 0)
            {
                // Neuanlage, aber Menge = 0
                if (storageItem.Quantity == 0)
                {
                    return;
                }

                TRACE("Lagerposition Neuanlage: {0}, {1}, {2}", storageItem.Quantity, storageItem.BestBefore_DebuggerDisplay, storageItem.StorageName);
                cmd += "INSERT INTO StorageItem (StorageId, ArticleId, Quantity, BestBefore, StorageName) VALUES (?, ?, ?, ?, ?)";
                command = databaseConnection.CreateCommand(cmd, new object[] 
                {
                    defaultStorageId,
                    storageItem.ArticleId,
                    storageItem.Quantity,
                    storageItem.BestBefore,
                    storageItem.StorageName
                });

                command.ExecuteNonQuery();

                // Neue Lagerposition-Id übernehmen.
                command = databaseConnection.CreateCommand("SELECT last_insert_rowid()");
                storageItem.StorageItemId = command.ExecuteScalar<int>();
            }
            else
            {
                if (storageItem.Quantity == 0)
                {
                    TRACE("Lagerposition Löschung: {0}, {1}, {2}", storageItem.StorageItemId, storageItem.BestBefore_DebuggerDisplay, storageItem.StorageName);
                    cmd += "DELETE FROM StorageItem WHERE StorageItemId = ?";
                    command = databaseConnection.CreateCommand(cmd, new object[] { storageItem.StorageItemId});
                    storageItem.StorageItemId = 0;
                }
                else
                {
                    TRACE("Lagerposition Änderung: {0}, {1}, {2}", storageItem.Quantity, storageItem.BestBefore_DebuggerDisplay, storageItem.StorageName);
                    cmd += "UPDATE StorageItem SET Quantity = ?, BestBefore = ?, StorageName = ? WHERE StorageItemId = ?";
                    command = databaseConnection.CreateCommand(cmd, new object[]
                    {
                        storageItem.Quantity,
                        storageItem.BestBefore,
                        storageItem.StorageName,
                        storageItem.StorageItemId
                    });
                }
                command.ExecuteNonQuery();
            }
        }

        #region Shopping List

        internal static IList<ShoppingItemListResult> GetShoppingList(string supermarket, string textFilter = null)
        {
            List<ShoppingItemListResult> result = new List<ShoppingItemListResult>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            SQLiteCommand command;

            cmd += "SELECT ShoppingListId, Article.ArticleId, Name, Manufacturer, Supermarket, Size, Unit, Calorie, Quantity, Notes, Price, Bought, Category";
            cmd += " FROM ShoppingList";
            cmd += " LEFT JOIN Article ON ShoppingList.ArticleId = Article.ArticleId";

            IList<object> parameter = new List<object>();

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (parameter.Count > 0)
                    cmd += " AND ";
                else
                    cmd += " WHERE ";

                cmd += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?";
                cmd += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ?)";
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
            }

            if (!string.IsNullOrEmpty(supermarket))
            {
                if (parameter.Count > 0)
                    cmd += " AND ";
                else
                    cmd += " WHERE ";

                cmd += " Supermarket = ?";
                parameter.Add(supermarket);
            }

            cmd += " ORDER BY Bought, Supermarket COLLATE NOCASE, Category COLLATE NOCASE, Name COLLATE NOCASE";

            command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());

            return command.ExecuteQuery<ShoppingItemListResult>();
        }

        internal static void SetShoppingItemBought(int articleId, bool isChecked)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            string cmd = string.Empty;
            SQLiteCommand command;

            command = databaseConnection.CreateCommand(
                "UPDATE ShoppingList SET Bought = ? WHERE ArticleId = ?", 
                new object[] { isChecked, articleId});

            command.ExecuteNonQuery();
        }

        internal static List<ArticleData> GetArticlesToCopyImages()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return new List<ArticleData>();

            return databaseConnection.Query<ArticleData>(
                "SELECT ArticleId, Name" +
                " FROM Article" +
                " WHERE Image IS NOT NULL" +
                " AND ArticleId NOT IN (SELECT ArticleId FROM ArticleImage)" +
                " ORDER BY Name COLLATE NOCASE");
        }

        internal static decimal GetShoppingListQuantiy(int articleId, decimal notFoundDefault = 0)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            string cmd = "SELECT Quantity FROM ShoppingList WHERE ArticleId = ?";
            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            IList<QuantityResult> result = command.ExecuteQuery<QuantityResult>();

            if (result.Count == 0)
            {
                return notFoundDefault;
            }

            return result[0].Quantity;
        }

        internal static double AddToShoppingList(int articleId, double addQuantity)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            SQLiteCommand command;
            string cmd = string.Empty;

            double isQuantity = (double)Database.GetShoppingListQuantiy(articleId);

            double newQuantity = isQuantity + addQuantity;

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (!isInList)
            {
                cmd = "INSERT INTO ShoppingList (ArticleId, Quantity) VALUES (?, ?)";
                command = databaseConnection.CreateCommand(cmd, new object[] { articleId, newQuantity });
            }
            else
            {
                if (newQuantity < 0)
                    newQuantity = 0;

                cmd = "UPDATE ShoppingList SET Quantity = ? WHERE ArticleId = ?";
                command = databaseConnection.CreateCommand(cmd, new object[] { newQuantity, articleId });
            }

            command.ExecuteNonQuery();

            return newQuantity;
        }

        internal static void SetShoppingItemQuantity(int articleId, decimal newdQuantity)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            SQLiteCommand command;
            string cmd = string.Empty;

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (!isInList)
            {
                cmd = "INSERT INTO ShoppingList (ArticleId, Quantity) VALUES (?, ?)";
                command = databaseConnection.CreateCommand(cmd, new object[] { articleId, newdQuantity });
            }
            else
            {
                cmd = "UPDATE ShoppingList SET Quantity = ? WHERE ArticleId = ?";
                command = databaseConnection.CreateCommand(cmd, new object[] { newdQuantity, articleId });
            }

            command.ExecuteNonQuery();
        }

        internal static void RemoveFromShoppingList(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            SQLiteCommand command;
            string cmd = string.Empty;

            cmd += "DELETE FROM ShoppingList WHERE ArticleId = ?";
            command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            command.ExecuteNonQuery();
        }

        internal static bool IsArticleInShoppingList(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return false;

            string cmd = "SELECT COUNT(*) FROM ShoppingList WHERE ArticleId = ?";
            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            int count = command.ExecuteScalar<int>();

            return count > 0;
        }

        internal static int GetToShoppingListQuantity(int articleId, int? minQuantity = null, int? prefQuantity = null)
        {

            if ((minQuantity == null) || (prefQuantity == null))
            {
                ArticleData article = Database.GetArticleData(articleId);

                // Artikle ist noch (gar) nicht angelegt?
                // (Laut Absturzbericht ist so ein Fall vorhanden, konnte aber nicht reproduziert werden.)
                if (article == null)
                {
                    return -1;
                }

                minQuantity  = article.MinQuantity.HasValue  ? article.MinQuantity.Value  : 0;
                prefQuantity = article.PrefQuantity.HasValue ? article.PrefQuantity.Value : 0;
            }

            int isQuantityInStorage  = (int)Database.GetArticleQuantityInStorage(articleId);

            int toBuyQuantity = ShoppingListHelper.GetToBuyQuantity(minQuantity.Value, prefQuantity.Value, isQuantityInStorage);
            
            int shoppingListQuantiy = (int)Database.GetShoppingListQuantiy(articleId);

            toBuyQuantity = toBuyQuantity - shoppingListQuantiy;

            if (toBuyQuantity < 0) // Mehr auf der Einkaufsliste als berechnet?
                return 0;

            return toBuyQuantity;
        }

        #endregion

        internal static IList<StorageItemQuantityResult> GetBestBeforeItemQuantity(int articleId, string storageName = null)
        {
            IList<StorageItemQuantityResult> result = new List<StorageItemQuantityResult>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            string filter = string.Empty;
            SQLiteCommand command;

            IList<object> parameter = new List<object>();

            filter = " WHERE StorageItem.ArticleId = ?";
            parameter.Add(articleId);

            if (!string.IsNullOrEmpty(storageName))
            {
                // Positionen direkt mit dem Lager oder Positionen ohne Lager, aber wenn der Artikel das Lager hat.
                filter += " AND (StorageItem.StorageName = ?";
                filter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT ArticleId FROM Article WHERE Article.StorageName = ?))";

                parameter.Add(storageName);
                parameter.Add(storageName);
            }

            cmd += "SELECT BestBefore, SUM(Quantity) AS Quantity, Article.WarnInDays";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += filter;
            cmd += " GROUP BY BestBefore";
            cmd += " ORDER BY BestBefore";

            command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());

            return command.ExecuteQuery<StorageItemQuantityResult>();
        }

        /// <summary>
        /// Artikelangaben ohne die Bilder (braucht weniger Speicher und ist schneller).
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        internal static ArticleData GetArticleData(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            cmd += "SELECT ArticleId, Name, Manufacturer, Category, SubCategory, StorageName, Supermarket,";
            cmd += " DurableInfinity, WarnInDays, Size, Unit, Calorie, MinQuantity, PrefQuantity,";
            cmd += " EANCode, Notes";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            // TODO: Gemeldeter Fehler: 
            // android.runtime.JavaProxyThrowable: at System.Linq.Enumerable.First[TSource] (System.Collections.Generic.IEnumerable`1[T] source) [0x00010] in <715c2ff6913942e6aa8535593b3ef35a>:0
            // at VorratsUebersicht.Database.GetArticleData (System.Int32 articleId) [0x00078] in <8f65cfdb5fac4bad9251caa1b2de7fec>:0

            return command.ExecuteQuery<ArticleData>().FirstOrDefault();
        }

        internal static ArticleImage GetArticleImage(int articleId, bool? showLarge = null)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;
            cmd += "SELECT ImageId, ArticleId, Type, ";

            if (showLarge == null)
            {
                cmd += "ImageLarge, ImageSmall";
            }
            else
            {
                if (showLarge.Value == true)
                    cmd += "ImageLarge";
                else
                    cmd += "ImageSmall";
            }

            cmd += " FROM ArticleImage";
            cmd += " WHERE ArticleId = ?";
            cmd += " AND Type = 0";


            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            return command.ExecuteQuery<ArticleImage>().FirstOrDefault();
        }

        internal static void SaveArticleImages(int articleId, byte[] imageLarge, byte[] image)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            string cmd = string.Empty;

            cmd += "UPDATE Article SET";
            cmd += " ImageLarge = ?,";
            cmd += " Image = ?";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { imageLarge, imageLarge, articleId });

            command.ExecuteNonQuery();
        }

        internal static decimal GetArticleQuantityInStorage(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            IList<QuantityResult> result = command.ExecuteQuery<QuantityResult>();
            decimal anzahl = result[0].Quantity;

            return anzahl;
        }

        internal static string[] GetCategoriesInUse()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Category AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Category IS NOT NULL";
            cmd += " AND ArticleId IN (SELECT ArticleId FROM StorageItem)";
            cmd += " ORDER BY Category COLLATE NOCASE";

            var command = databaseConnection.CreateCommand(cmd);
            IList<StringResult> result = command.ExecuteQuery<StringResult>();

            string[] stringList = new string[result.Count];
            for(int i = 0; i < result.Count; i++)
            {
                stringList[i] = result[i].Value;
            }

            return stringList;
        }

        internal static List<string> GetSubcategoriesOf(string category = null, bool inStorageArticlesOnly = false)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT SubCategory AS Value";
            cmd += " FROM Article";
            cmd += " WHERE SubCategory IS NOT NULL";
            cmd += " AND SubCategory <> ''";
            if (category != null)
            {
                cmd += " AND Category = ?";
            }
            if (inStorageArticlesOnly)
            {
                cmd += " AND ArticleId IN (SELECT StorageItem.ArticleId FROM StorageItem)";
            }
            cmd += " ORDER BY SubCategory COLLATE NOCASE";

            SQLiteCommand command;

            if (category != null)
            {
                command = databaseConnection.CreateCommand(cmd, new object[] { category });
            }
            else
            {
                command = databaseConnection.CreateCommand(cmd, new object[] { });
            }

            List<StringResult> result = command.ExecuteQuery<StringResult>();

            List<string> stringList = new List<string>();
            foreach(StringResult item in result)
            {
                stringList.Add(item.Value);
            }

            return stringList;
        }

        internal static List<string> GetStorageNames(bool inStorageArticlesOnly = false)
        {
            List<string> stringList = new List<string>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return stringList;

            // Lagernamen aus dem Artikelstamm und den Positionen ermitteln.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Article.StorageName AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Article.StorageName IS NOT NULL AND Article.StorageName <> ''";
            if (inStorageArticlesOnly)
            {
                cmd += " AND Article.ArticleId IN (SELECT StorageItem.ArticleId FROM StorageItem)";
            }
            cmd += " UNION";
            cmd += " SELECT StorageName AS Value";
            cmd += " FROM StorageItem";
            cmd += " WHERE StorageName IS NOT NULL AND StorageName <> ''";
            cmd += " ORDER BY 1 COLLATE NOCASE";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT StorageName: {0}", stopWatch.Elapsed.ToString());

            foreach(StringResult item in result)
            {
                stringList.Add(item.Value);
            }

            return stringList;
        }

        internal static List<string> GetCategoryNames()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Category AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Category IS NOT NULL";
            cmd += " ORDER BY Category COLLATE NOCASE";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Category: {0}", stopWatch.Elapsed.ToString());

            List<string> stringList = new List<string>();
            foreach(StringResult item in result)
            {
                stringList.Add(item.Value);
            }

            return stringList;
        }

        internal static List<string> GetCategoryAndSubCategoryNames()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Category AS Value1, Subcategory AS Value2";
            cmd += " FROM Article";
            cmd += " WHERE Category IS NOT NULL";
            cmd += " ORDER BY Category COLLATE NOCASE, Subcategory COLLATE NOCASE";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var command = databaseConnection.CreateCommand(cmd);
            IList<StringPairResult> result = command.ExecuteQuery<StringPairResult>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Category, Subcategory: {0}", stopWatch.Elapsed.ToString());

            string lastCategory = string.Empty;

            List<string> stringList = new List<string>();
            foreach(StringPairResult item in result)
            {
                string categoryName    = item.Value1;
                string subCategoryName = item.Value2;

                if (string.IsNullOrEmpty(categoryName) && string.IsNullOrEmpty(subCategoryName))
                    continue;
                if (categoryName != lastCategory)
                {
                    stringList.Add(categoryName);
                    lastCategory = categoryName;
                }

                if (!string.IsNullOrEmpty(subCategoryName))
                {
                    // Die Zeichenfülge "  - " vor dem {0} ist wichtig
                    // für das Erkennen der Unterkategorie bei Auswahl.

                    stringList.Add(string.Format("  - {0}", subCategoryName));
                }
            }

            return stringList;
        }

        internal static List<string> GetManufacturerNames()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Manufacturer AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Manufacturer IS NOT NULL";
            cmd += " AND Manufacturer <> ''";
            cmd += " ORDER BY Manufacturer COLLATE NOCASE";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Manufacturer: {0}", stopWatch.Elapsed.ToString());

            List<string> stringList = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                string supermarketName = result[i].Value;
                stringList.Add(supermarketName);
            }

            return stringList;
        }

        internal static List<string> GetSupermarketNames(bool shoppingListOnly = false)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Supermarket AS Value";
            cmd += " FROM Article";

            if (shoppingListOnly)
            {
                cmd += " JOIN ShoppingList ON ShoppingList.ArticleId = Article.ArticleId";
            }

            cmd += " WHERE Supermarket IS NOT NULL";
            cmd += " AND Supermarket <> ''";
            cmd += " ORDER BY Supermarket COLLATE NOCASE";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = command.ExecuteQuery<StringResult>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Supermarket: {0}", stopWatch.Elapsed.ToString());


            List<string> stringList = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                string supermarketName = result[i].Value;
                stringList.Add(supermarketName);
            }

            return stringList;
        }

        internal static IList<Article> GetArticleListNoImages(string category, string subCategory, string eanCode, bool notInStorage, string textFilter = null)
        {
            IList<Article> result = new Article[0];

            var databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            IList<object> parameter = new List<object>();

            string filter = string.Empty;

            if (!string.IsNullOrEmpty(category))
            {
                filter += " WHERE Article.Category = ?";
                parameter.Add(category);
            }

            if (!string.IsNullOrEmpty(subCategory))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                filter += " Article.SubCategory = ?";
                parameter.Add(subCategory);
            }

            if (!string.IsNullOrEmpty(eanCode))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                filter += " Article.EANCode LIKE ?";
                parameter.Add("%" + eanCode + "%");
            }

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                switch (textFilter.ToUpper())
                {
                    case "P-":
                        filter += " Article.Price IS NULL";
                        break;

                    case "K-":
                        filter += " Article.Calorie IS NULL";
                        break;

                    default:

                        filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?";
                        filter += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ?)";
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        parameter.Add("%" + textFilter + "%");
                        break;
                }
            }

            if (notInStorage)
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                filter += " ArticleId NOT IN (SELECT ArticleId FROM StorageItem)";

            }

            string cmd = string.Empty;
            cmd += "SELECT ArticleId, Name, Manufacturer, Category, SubCategory, DurableInfinity, WarnInDays,";
            cmd += " Size, Unit, Notes, EANCode, Calorie, Price, StorageName, Supermarket";
            cmd += " FROM Article";
            cmd += filter;
            cmd += " ORDER BY Name COLLATE NOCASE";

            var command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            result = command.ExecuteQuery<Article>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für Artikelliste: {0}", stopWatch.Elapsed.ToString());

            return result;
        }

        internal static IList<Article> GetArticlesByEanCode(string eanCode)
        {
            IList<Article> result = new List<Article>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;

            cmd = string.Empty;
            cmd += "SELECT ArticleId";
            cmd += " FROM Article";
            cmd += " WHERE EANCode LIKE ?";
            

            var command = databaseConnection.CreateCommand(cmd, new object[] { "%" + eanCode + "%" });

            return command.ExecuteQuery<Article>();
        }

        internal static Article GetArticle(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            cmd += "SELECT *";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            return command.ExecuteQuery<Article>().FirstOrDefault();
        }

        internal static string GetArticleName(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            cmd += "SELECT Name";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            return command.ExecuteScalar<string>();
        }

        internal static decimal GetArticleCount()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT COUNT(*) AS Quantity";
            cmd += " FROM Article";

            var command = databaseConnection.CreateCommand(cmd);
            IList<QuantityResult> result = command.ExecuteQuery<QuantityResult>();
            return result[0].Quantity;
        }


        internal static decimal GetArticleCount_Abgelaufen()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;


            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE BestBefore < date('now')";
            //cmd += " AND 1 = 2";


            var command = databaseConnection.CreateCommand(cmd);
            IList<QuantityResult> result = command.ExecuteQuery<QuantityResult>();
            return result[0].Quantity;
        }

        /// <summary>
        /// Artikel suchen, für die eine Warnung ausgegeben werden soll.
        /// </summary>
        /// <returns></returns>
        internal static decimal GetArticleCount_BaldZuVerbrauchen()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            string cmd = string.Empty;
            cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE (date(BestBefore,  (-WarnInDays || ' day')) <= date('now'))";
            cmd += " AND BestBefore >= date('now')";
            cmd += " AND WarnInDays <> 0";
            //cmd += " OR 1 = 1";
            
            var command = databaseConnection.CreateCommand(cmd);
            var result = command.ExecuteQuery<QuantityResult>();
            return result[0].Quantity;
        }

        internal static IList<StorageItemQuantityResult> GetStorageItemQuantityListNoImage(
            string category, 
            string subCategory, 
            string eanCode, 
            bool showNotInStorageArticles, 
            string textFilter = null, 
            string storageName = null,
            bool oderByToConsumeDate = false)
        {
            var result = new List<StorageItemQuantityResult>();

            var databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            IList<object> parameter = new List<object>();

            string bestBeforeFilter = string.Empty;

            if (!string.IsNullOrEmpty(storageName))
            {
                // Positionen direkt mit dem Lager oder Positionen ohne Lager, aber wenn der Artikel das Lager hat.
                bestBeforeFilter += " AND (StorageItem.StorageName = ?";
                bestBeforeFilter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT Art1.ArticleId FROM Article AS Art1 WHERE Art1.StorageName = ?))";

                parameter.Add(storageName);
                parameter.Add(storageName);
            }

            string bestBeforeSelect = "SELECT BestBefore" +
                " FROM StorageItem" +
                " WHERE StorageItem.ArticleId = Article.ArticleId" +
                bestBeforeFilter +
                " AND BestBefore IS NOT NULL" +
                " ORDER BY BestBefore ASC LIMIT 1";

            string sumQuantityFilter = string.Empty;

            if (!string.IsNullOrEmpty(storageName))
            {
                sumQuantityFilter += " AND (StorageItem.StorageName = ?";
                sumQuantityFilter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT Art2.ArticleId FROM Article AS Art2 WHERE Art2.StorageName = ?))";

                parameter.Add(storageName);
                parameter.Add(storageName);
            }

            string sumQuantitySelect = "SELECT SUM(Quantity)" +
                " FROM StorageItem" +
                " WHERE StorageItem.ArticleId = Article.ArticleId" +
                sumQuantityFilter;

            string cmd = string.Empty;
            cmd += "SELECT Article.ArticleId, Name, WarnInDays, Size, Unit, DurableInfinity, MinQuantity, PrefQuantity, Price, Calorie, Article.StorageName, ";
            cmd += " (" + sumQuantitySelect + ") AS Quantity,";
            cmd += " IFNULL((" + bestBeforeSelect + "), '9999.12.31') AS BestBefore,";
            cmd += " ShoppingList.Quantity AS ShoppingListQuantity";
            cmd += " FROM Article";
            cmd += " LEFT JOIN ShoppingList ON ShoppingList.ArticleId = Article.ArticleId";

            string filter = string.Empty;

            if (!showNotInStorageArticles)
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += "Article.ArticleId IN (SELECT StorageItem.ArticleId FROM StorageItem)";
            }

            if (category != null)
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += "Article.Category = ?";
                parameter.Add(category);
            }

            if (subCategory != null)
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += "Article.SubCategory = ?";
                parameter.Add(subCategory);
            }

            if (!string.IsNullOrEmpty(eanCode))
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += "Article.EANCode LIKE ?";
                parameter.Add("%" + eanCode + "%");
            }

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?";
                filter += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ?)";
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
            }

            if (!string.IsNullOrEmpty(storageName))
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += " (Article.StorageName = ? OR Article.ArticleId IN (SELECT ArticleId FROM StorageItem WHERE StorageName = ?))";
                parameter.Add(storageName);
                parameter.Add(storageName);
            }
            
            cmd += filter;

            if (oderByToConsumeDate)
            {
                cmd += " ORDER BY BestBefore ASC, Article.Name COLLATE NOCASE";
            }
            else
            {
                cmd += " ORDER BY Article.Name COLLATE NOCASE";
            }

            var command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            result = command.ExecuteQuery<StorageItemQuantityResult>();
            
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für Lagerbestand: {0}", stopWatch.Elapsed.ToString());

            return result;
        }

        internal static IList<StorageItemQuantityResult> GetStorageItemQuantityList(int articleId)
        {
            var result = new List<StorageItemQuantityResult>();

            var databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;

            cmd += "SELECT StorageItem.*, Article.ArticleId, Article.WarnInDays";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE StorageItem.ArticleId = ?";
            cmd += " ORDER BY BestBefore ASC";


            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            return command.ExecuteQuery<StorageItemQuantityResult>();
        }

        internal static void DeleteArticle(int articleId)
        {
            var databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            databaseConnection.BeginTransaction();

            databaseConnection.Execute("DELETE FROM ShoppingList WHERE ArticleId = ?", 
                new object[] { articleId});

            databaseConnection.Execute("DELETE FROM ArticleImage WHERE ArticleId = ?", 
                new object[] { articleId});

            databaseConnection.Execute("DELETE FROM Article WHERE ArticleId = ?", 
                new object[] { articleId});

            databaseConnection.Commit();
        }

        internal static string GetSettingsString(string key)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            cmd += "SELECT Value";
            cmd += " FROM Settings";
            cmd += " WHERE Key = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { key });

            return command.ExecuteScalar<string>();
        }

        internal static DateTime? GetSettingsDate(string key)
        {
            var dateText = Database.GetSettingsString(key);
            if (dateText == null)
                return null;

            try
            {
                if (dateText.Length == 10)
                {
                    var date = DateTime.ParseExact(dateText, "yyyy.MM.dd", CultureInfo.InvariantCulture);
                    return date.Date;
                }

                var dateTime = DateTime.ParseExact(dateText, "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);

                return dateTime;
            }
            catch (Exception ex)
            {
                TRACE(ex);
            }

            return null;
        }

        internal static void SetSettingsDate(string key, DateTime date)
        {
            string dateText = date.ToString("yyyy.MM.dd HH:mm:ss");

            Database.SetSettings(key, dateText);
        }

        internal static void SetSettings(string key, string value)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return;

            string oldValue = GetSettingsString(key);

            if (oldValue == null)
            {
                string cmd = "INSERT INTO Settings (Key, Value) VALUES (?, ?)";

                var command = databaseConnection.CreateCommand(cmd, new object[] { key, value });

                command.ExecuteNonQuery();
            }
            else
            {
                string cmd = "UPDATE Settings SET Value = ? WHERE Key = ?";

                var command = databaseConnection.CreateCommand(cmd, new object[] { value, key });

                command.ExecuteNonQuery();
            }
        }

        internal static string ClearSettings(string key)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            cmd += "DELETE";
            cmd += " FROM Settings";
            cmd += " WHERE Key = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { key });

            return command.ExecuteScalar<string>();
        }

    }
}