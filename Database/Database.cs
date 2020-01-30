using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

using SQLite;

namespace VorratsUebersicht
{
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

                cmd += "INSERT INTO StorageItem (StorageId, ArticleId, Quantity, BestBefore) VALUES (?, ?, ?, ?)";
                command = databaseConnection.CreateCommand(cmd, new object[] { defaultStorageId, storageItem.ArticleId, storageItem.Quantity, storageItem.BestBefore});
            }
            else
            {
                if (storageItem.Quantity == 0)
                {
                    cmd += "DELETE FROM StorageItem WHERE StorageItemId = ?";
                    command = databaseConnection.CreateCommand(cmd, new object[] { storageItem.StorageItemId});
                }
                else
                {
                    cmd += "UPDATE StorageItem SET Quantity = ?, BestBefore = ? WHERE StorageItemId = ?";
                    command = databaseConnection.CreateCommand(cmd, new object[] { storageItem.Quantity, storageItem.BestBefore, storageItem.StorageItemId});
                }
            }

            int result = command.ExecuteNonQuery();

        }

        internal static IList<ShoppingItemListResult> GetShoppingList(string supermarket, string textFilter = null)
        {
            List<ShoppingItemListResult> result = new List<ShoppingItemListResult>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            SQLiteCommand command;

            cmd += "SELECT ShoppingListId, Article.ArticleId, Name, Manufacturer, Supermarket, Size, Unit, Calorie, Quantity, Notes, Price, Bought";
            cmd += " FROM ShoppingList";
            cmd += " LEFT JOIN Article ON ShoppingList.ArticleId = Article.ArticleId";

            IList<object> parameter = new List<object>();

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (parameter.Count > 0)
                    cmd += " AND ";
                else
                    cmd += " WHERE ";

                cmd += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?)";
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

            cmd += " ORDER BY Bought, Supermarket, Name";

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

        internal static int GetShoppingListQuantiy(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            string cmd = "SELECT Quantity FROM ShoppingList WHERE ArticleId = ?";
            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            int isQuantity = command.ExecuteScalar<int>();

            return isQuantity;
        }


        internal static double AddToShoppingList(int articleId, double addQuantity)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return 0;

            SQLiteCommand command;
            string cmd = string.Empty;

            double isQuantity = Database.GetShoppingListQuantiy(articleId);

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

        internal static int GetToShoppingListQuantity(int articleId)
        {
            ArticleData article = Database.GetArticleData(articleId);

            int minQuantity  = article.MinQuantity.HasValue  ? article.MinQuantity.Value  : 0;
            int prefQuantity = article.PrefQuantity.HasValue ? article.PrefQuantity.Value : 0;

            return Database.GetToShoppingListQuantity(articleId, minQuantity, prefQuantity);
        }

        internal static int GetToShoppingListQuantity(int articleId, int minQuantity, int prefQuantity)
        {
            int isQuantity  = (int)Database.GetArticleQuantityInStorage(articleId);

            int toBuyQuantity = ShoppingListHelper.GetToBuyQuantity(minQuantity, prefQuantity, isQuantity);
            
            int shoppingListQuantiy = Database.GetShoppingListQuantiy(articleId);

            toBuyQuantity = toBuyQuantity - shoppingListQuantiy;

            if (toBuyQuantity < 0) // Mehr auf der Einkaufsliste als berechnet?
                return 0;

            return toBuyQuantity;
        }

        internal static IList<StorageItemQuantityResult> GetBestBeforeItemQuantity(int articleId)
        {
            IList<StorageItemQuantityResult> result = new List<StorageItemQuantityResult>();

            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            SQLiteCommand command;

            cmd += "SELECT BestBefore, SUM(Quantity) AS Quantity, Article.WarnInDays";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE StorageItem.ArticleId = ?";
            cmd += " GROUP BY BestBefore";
            cmd += " ORDER BY BestBefore";

            command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

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

            return command.ExecuteQuery<ArticleData>().First();
        }

        internal static ArticleImage GetArticleImage(int articleId, bool showLarge)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;
            cmd += "SELECT ImageId, ArticleId, Type, ";

            if (showLarge)
                cmd += "ImageLarge";
            else
                cmd += "ImageSmall";

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
            cmd += " ORDER BY Category";

            var command = databaseConnection.CreateCommand(cmd);
            IList<StringResult> result = command.ExecuteQuery<StringResult>();

            string[] stringList = new string[result.Count];
            for(int i = 0; i < result.Count; i++)
            {
                stringList[i] = result[i].Value;
            }

            return stringList;
        }

        internal static string[] GetSubcategoriesOf(string category = null)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT SubCategory AS Value";
            cmd += " FROM Article";
            cmd += " WHERE SubCategory IS NOT NULL";
            if (category != null)
            {
                cmd += " AND Category = ?";
            }
            cmd += " AND ArticleId IN (SELECT ArticleId FROM StorageItem)";
            cmd += " ORDER BY SubCategory";

            SQLiteCommand command;

            if (category != null)
            {
                command = databaseConnection.CreateCommand(cmd, new object[] { category });
            }
            else
            {
                command = databaseConnection.CreateCommand(cmd, new object[] { });
            }

            IList<StringResult> result = command.ExecuteQuery<StringResult>();

            string[] stringList = new string[result.Count];
            for(int i = 0; i < result.Count; i++)
            {
                stringList[i] = result[i].Value;
            }

            return stringList;
        }

        internal static List<string> GetStorageNames()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT StorageName AS Value";
            cmd += " FROM Article";
            cmd += " WHERE StorageName IS NOT NULL";
            cmd += " ORDER BY StorageName";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r DISTINCT StorageName: {0}", stopWatch.Elapsed.ToString());

            List<string> stringList = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                string storageName = result[i].Value;
                if (string.IsNullOrEmpty(storageName))
                    continue;

                stringList.Add(storageName);
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
            cmd += " ORDER BY Category";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r DISTINCT Category: {0}", stopWatch.Elapsed.ToString());

            List<string> stringList = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                string category = result[i].Value;
                if (string.IsNullOrEmpty(category))
                    continue;

                stringList.Add(category);
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
            cmd += " ORDER BY Category, Subcategory";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var command = databaseConnection.CreateCommand(cmd);
            IList<StringPairResult> result = command.ExecuteQuery<StringPairResult>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r DISTINCT Category, Subcategory: {0}", stopWatch.Elapsed.ToString());

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
                    // Die Zeichenf�lge "  - " vor dem {0} ist wichtig
                    // f�r das Erkennen der Unterkategorie bei Auswahl.

                    stringList.Add(string.Format("  - {0}", subCategoryName));
                }
            }

            return stringList;
        }

        internal static string[] GetManufacturerNames()
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Manufacturer AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Manufacturer IS NOT NULL";
            cmd += " ORDER BY Manufacturer";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = command.ExecuteQuery<StringResult>();
            
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r DISTINCT Manufacturer: {0}", stopWatch.Elapsed.ToString());

            string[] stringList = new string[result.Count];
            for (int i = 0; i < result.Count; i++)
            {
                stringList[i] = result[i].Value;
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
            cmd += " ORDER BY Supermarket";

            SQLiteCommand command = databaseConnection.CreateCommand(cmd, new object[] { });

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = command.ExecuteQuery<StringResult>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r DISTINCT Supermarket: {0}", stopWatch.Elapsed.ToString());


            List<string> stringList = new List<string>();
            for (int i = 0; i < result.Count; i++)
            {
                string supermarketName = result[i].Value;
                if (string.IsNullOrEmpty(supermarketName))
                    continue;

                stringList.Add(supermarketName);
            }

            return stringList;
        }

        internal static IList<Article> GetArticleListNoImages(string category, string subCategory, bool notInStorage, string textFilter = null)
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

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?)";
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
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
            cmd += " Size, Unit, Notes, EANCode, Calorie, Price, StorageName";
            cmd += " FROM Article";
            cmd += filter;
            cmd += " ORDER BY Name COLLATE NOCASE";

            var command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            result = command.ExecuteQuery<Article>();

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage f�r Artikelliste: {0}", stopWatch.Elapsed.ToString());

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
            cmd += " WHERE EANCode = ?";
            

            var command = databaseConnection.CreateCommand(cmd, new object[] { eanCode });

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
            cmd += " AND Article.DurableInfinity = 0";
            //cmd += " AND 1 = 2";


            var command = databaseConnection.CreateCommand(cmd);
            IList<QuantityResult> result = command.ExecuteQuery<QuantityResult>();
            return result[0].Quantity;
        }

        /// <summary>
        /// Artikel suchen, f�r die eine Warnung ausgegeben werden soll.
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
            cmd += " AND Article.DurableInfinity = 0";
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

            string cmd = string.Empty;
            cmd += "SELECT ArticleId, Name, WarnInDays, Size, Unit, DurableInfinity, MinQuantity, PrefQuantity, Price, Calorie, StorageName, ";
            cmd += " (SELECT SUM(Quantity) FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId) AS Quantity,";
            cmd += " IFNULL((SELECT BestBefore FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId AND BestBefore IS NOT NULL ORDER BY BestBefore ASC LIMIT 1), '9999.12.31') AS BestBefore";
            cmd += " FROM Article";

            string filter = string.Empty;
            IList<object> parameter = new List<object>();

            if (!showNotInStorageArticles)
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += "ArticleId IN (SELECT ArticleId FROM StorageItem)";
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
                filter += "Article.EANCode = ?";
                parameter.Add(eanCode);
            }

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?)";
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
                parameter.Add("%" + textFilter + "%");
            }

            if (!string.IsNullOrEmpty(storageName))
            {
                if (string.IsNullOrEmpty(filter)) { filter += " WHERE "; } else { filter += " AND "; }
                filter += " Article.StorageName = ?";
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
            Tools.TRACE("Dauer der Abfrage f�r Lagerbestand: {0}", stopWatch.Elapsed.ToString());

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
    }
}