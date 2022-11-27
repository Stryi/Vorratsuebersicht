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
            string cmd = string.Empty;

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
                storageItem.StorageItemId = DatabaseService.Instance.ExecuteInsert(cmd, new object[] 
                {
                    defaultStorageId,
                    storageItem.ArticleId,
                    storageItem.Quantity,
                    storageItem.BestBefore,
                    storageItem.StorageName
                });
            }
            else
            {
                if (storageItem.Quantity == 0)
                {
                    TRACE("Lagerposition Löschung: {0}, {1}, {2}", storageItem.StorageItemId, storageItem.BestBefore_DebuggerDisplay, storageItem.StorageName);
                    cmd += "DELETE FROM StorageItem WHERE StorageItemId = ?";
                    DatabaseService.Instance.ExecuteNonQuery(cmd, storageItem.StorageItemId);
                    storageItem.StorageItemId = 0;
                }
                else
                {
                    TRACE("Lagerposition Änderung: {0}, {1}, {2}", storageItem.Quantity, storageItem.BestBefore_DebuggerDisplay, storageItem.StorageName);
                    cmd += "UPDATE StorageItem SET Quantity = ?, BestBefore = ?, StorageName = ? WHERE StorageItemId = ?";
                    DatabaseService.Instance.ExecuteNonQuery(cmd, new object[]
                    {
                        storageItem.Quantity,
                        storageItem.BestBefore,
                        storageItem.StorageName,
                        storageItem.StorageItemId
                    });
                }
            }
        }

        #region Shopping List

        internal static IList<ShoppingItemListResult> GetShoppingList(string supermarket, string textFilter = null, int? orderBy = null)
        {
            List<ShoppingItemListResult> result = new List<ShoppingItemListResult>();

            string cmd = string.Empty;

            cmd += "SELECT ShoppingListId, Article.ArticleId, Name, Manufacturer, Supermarket, Size, Unit, Calorie, Quantity, Notes, Price, Bought, Category, SubCategory,";
            cmd += " (SELECT LENGTH(ImageSmall) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageSmallLength,";
            cmd += " (SELECT LENGTH(ImageLarge) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageLargeLength";
            cmd += " FROM ShoppingList";
            cmd += " JOIN Article ON ShoppingList.ArticleId = Article.ArticleId";

            IList<object> parameter = new List<object>();

            if (!string.IsNullOrEmpty(textFilter))
            {
                if (parameter.Count > 0)
                    cmd += " AND ";
                else
                    cmd += " WHERE ";

                cmd += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?";
                cmd += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)";
                parameter.Add("%" + textFilter + "%");
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

                cmd += " Supermarket LIKE  ?";
                parameter.Add("%" + supermarket + "%");
            }

            switch(orderBy)
            {
                case 1:
                    cmd += " ORDER BY Bought, Supermarket COLLATE NOCASE, Category COLLATE NOCASE, Name COLLATE NOCASE";
                    break;

                case 2:
                    cmd += " ORDER BY Supermarket COLLATE NOCASE, Bought, Category COLLATE NOCASE, Name COLLATE NOCASE";
                    break;

                case 3:
                    cmd += " ORDER BY ShoppingListId";
                    break;

                case 4:
                    cmd += " ORDER BY Name COLLATE NOCASE";
                    break;
            }

            return DatabaseService.Instance.ExecuteQuery<ShoppingItemListResult>(cmd, parameter.ToArray<object>());
        }

        internal static void SetShoppingItemBought(int articleId, bool isChecked)
        {
            DatabaseService.Instance.ExecuteNonQuery(
                "UPDATE ShoppingList SET Bought = ? WHERE ArticleId = ?", 
                isChecked, articleId);
        }

        internal static List<Article> GetArticlesToCopyImages()
        {
            return DatabaseService.Instance.ExecuteQuery<Article>(
                "SELECT ArticleId, Name" +
                " FROM Article" +
                " WHERE Image IS NOT NULL" +
                " AND ArticleId NOT IN (SELECT ArticleId FROM ArticleImage)" +
                " ORDER BY Name COLLATE NOCASE");
        }

        internal static decimal GetShoppingListQuantiy(int articleId, decimal notFoundDefault = 0)
        {
            string cmd = "SELECT Quantity FROM ShoppingList WHERE ArticleId = ?";

            decimal? result = DatabaseService.Instance.ExecuteScalar<decimal?>(cmd, articleId);

            if (!result.HasValue)
            {
                return notFoundDefault;
            }

            return result.Value;
        }

        internal static double AddToShoppingList(int articleId, double addQuantity)
        {
            string cmd = string.Empty;

            double isQuantity = (double)Database.GetShoppingListQuantiy(articleId);

            double newQuantity = isQuantity + addQuantity;

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (!isInList)
            {
                cmd = "INSERT INTO ShoppingList (ArticleId, Quantity) VALUES (?, ?)";
                DatabaseService.Instance.ExecuteNonQuery(cmd, articleId, newQuantity);
            }
            else
            {
                if (newQuantity < 0)
                    newQuantity = 0;

                cmd = "UPDATE ShoppingList SET Quantity = ? WHERE ArticleId = ?";
                DatabaseService.Instance.ExecuteNonQuery(cmd, newQuantity, articleId);
            }

            return newQuantity;
        }

        internal static void SetShoppingItemQuantity(int articleId, decimal newdQuantity)
        {
            string cmd = string.Empty;

            bool isInList = Database.IsArticleInShoppingList(articleId);
            if (!isInList)
            {
                cmd = "INSERT INTO ShoppingList (ArticleId, Quantity) VALUES (?, ?)";
                DatabaseService.Instance.ExecuteNonQuery(cmd, articleId, newdQuantity);
            }
            else
            {
                cmd = "UPDATE ShoppingList SET Quantity = ? WHERE ArticleId = ?";
                DatabaseService.Instance.ExecuteNonQuery(cmd, newdQuantity, articleId);
            }
        }

        internal static void RemoveFromShoppingList(int articleId)
        {
            string cmd = string.Empty;

            cmd += "DELETE FROM ShoppingList WHERE ArticleId = ?";

            DatabaseService.Instance.ExecuteNonQuery(cmd, articleId);
        }

        internal static bool IsArticleInShoppingList(int articleId)
        {
            string cmd = "SELECT COUNT(*) FROM ShoppingList WHERE ArticleId = ?";
            
            int count = DatabaseService.Instance.ExecuteScalar<int>(cmd, articleId);

            return count > 0;
        }

        internal static int GetToShoppingListQuantity(int articleId, int? minQuantity = null, int? prefQuantity = null)
        {

            if ((minQuantity == null) || (prefQuantity == null))
            {
                Article article = Database.GetArticleData(articleId);

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

        internal static IList<StorageItemQuantityResult> GetBestBeforeItemQuantity(int articleId)
        {
            string cmd = string.Empty;

            cmd += "SELECT BestBefore, SUM(Quantity) AS Quantity, Article.WarnInDays";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE StorageItem.ArticleId = ?";
            cmd += " GROUP BY BestBefore";
            cmd += " ORDER BY BestBefore";

            return DatabaseService.Instance.ExecuteQuery<StorageItemQuantityResult>(cmd, articleId);
        }

        internal static IList<StorageItemQuantityResult> GetBestBeforeItemQuantity(string storageName = null)
        {
            string cmd = string.Empty;
            string filter = string.Empty;

            IList<object> parameter = new List<object>();

            if (!string.IsNullOrEmpty(storageName))
            {
                // Positionen direkt mit dem Lager oder Positionen ohne Lager, aber wenn der Artikel das Lager hat.
                filter += " WHERE (StorageItem.StorageName = ?";
                filter += "  OR (IFNULL(StorageItem.StorageName, '') = '') AND StorageItem.ArticleId IN (SELECT ArticleId FROM Article WHERE Article.StorageName = ?))";

                parameter.Add(storageName);
                parameter.Add(storageName);
            }

            cmd += "SELECT BestBefore, SUM(Quantity) AS Quantity, Article.WarnInDays, StorageItem.ArticleId";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += filter;
            cmd += " GROUP BY BestBefore";
            cmd += " ORDER BY BestBefore";

            return DatabaseService.Instance.ExecuteQuery<StorageItemQuantityResult>(cmd, parameter.ToArray<object>());
        }

        /// <summary>
        /// Artikelangaben ohne die Bilder (braucht weniger Speicher und ist schneller).
        /// </summary>
        /// <param name="articleId"></param>
        /// <returns></returns>
        internal static Article GetArticleData(int articleId)
        {
            string cmd = string.Empty;

            cmd += "SELECT ArticleId, Name, Manufacturer, Category, SubCategory, StorageName, Supermarket,";
            cmd += " DurableInfinity, WarnInDays, Size, Unit, Calorie, MinQuantity, PrefQuantity,";
            cmd += " EANCode, Notes";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            var articleList = DatabaseService.Instance.ExecuteQuery<Article>(cmd, articleId);

            return articleList.FirstOrDefault();
        }

        internal static ArticleImage GetArticleImage(int articleId, bool showLarge)
        {
            string cmd = string.Empty;
            cmd += "SELECT ImageId, ArticleId, Type, ";

            if (showLarge == true)
                cmd += "ImageLarge";
            else
                cmd += "ImageSmall";

            cmd += " FROM ArticleImage";
            cmd += " WHERE ArticleId = ?";
            cmd += " AND Type = 0";

            var articleImages = DatabaseService.Instance.ExecuteQuery<ArticleImage>(cmd, articleId);

            return articleImages.FirstOrDefault();
        }

        internal static decimal GetArticleQuantityInStorage(int articleId)
        {
            string cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " WHERE ArticleId = ?";

            var result = DatabaseService.Instance.ExecuteScalar<decimal?>(cmd, articleId);

            return result.HasValue ? result.Value : 0;
        }

        internal static string[] GetCategoriesInUse()
        {
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Category AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Category IS NOT NULL";
            cmd += " AND ArticleId IN (SELECT ArticleId FROM StorageItem)";
            cmd += " ORDER BY Category COLLATE NOCASE";

            IList<StringResult> result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd);

            string[] stringList = new string[result.Count];
            for(int i = 0; i < result.Count; i++)
            {
                stringList[i] = result[i].Value;
            }

            return stringList;
        }

        internal static List<string> GetSubcategoriesOf(string category = null, bool inStorageArticlesOnly = false)
        {
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

            List<StringResult> result;

            if (category != null)
            {
                result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd, category);
            }
            else
            {
                result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd);
            }

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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            IList<StringResult> result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd);
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT StorageName: {0}", stopWatch.Elapsed.ToString());

            foreach(StringResult item in result)
            {
                stringList.Add(item.Value);
            }

            return stringList;
        }

        internal static List<string> GetCategoryAndSubCategoryNames()
        {
            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Category AS Value1, Subcategory AS Value2";
            cmd += " FROM Article";
            cmd += " WHERE Category IS NOT NULL";
            cmd += " ORDER BY Category COLLATE NOCASE, Subcategory COLLATE NOCASE";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringPairResult> result = DatabaseService.Instance.ExecuteQuery<StringPairResult>(cmd);

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
            List<string> stringList = new List<string>();

            string cmd = string.Empty;
            cmd += "SELECT DISTINCT Manufacturer AS Value";
            cmd += " FROM Article";
            cmd += " WHERE Manufacturer IS NOT NULL";
            cmd += " AND Manufacturer <> ''";
            cmd += " ORDER BY Manufacturer COLLATE NOCASE";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd);
            
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Manufacturer: {0}", stopWatch.Elapsed.ToString());

            for (int i = 0; i < result.Count; i++)
            {
                string supermarketName = result[i].Value;
                stringList.Add(supermarketName);
            }

            return stringList;
        }

        internal static List<string> GetSupermarketNames(bool shoppingListOnly = false)
        {
            List<string> stringList = new List<string>();

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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            IList<StringResult> result = DatabaseService.Instance.ExecuteQuery<StringResult>(cmd);

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für DISTINCT Supermarket: {0}", stopWatch.Elapsed.ToString());


            for (int i = 0; i < result.Count; i++)
            {
                string supermarketName = result[i].Value;
                if (!shoppingListOnly)
                {
                    stringList.Add(supermarketName);
                    continue;
                }

                foreach(var marketList in supermarketName.Split(','))
                {
                    var name = marketList.Trim();
                    if (!stringList.Contains(name))
                    {
                        stringList.Add(name);
                    }
                }
            }

            return stringList;
        }

        internal static IList<ArticleQuantity> GetArticleQuantityList(
            string category,
            string subCategory,
            string eanCode,
            bool notInStorage,
            int  specialFilter,
            string textFilter = null)
        {
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

                    case "B+":
                        // Artikel zum [B]estellen.
                        filter += " ArticleId NOT IN (SELECT ArticleId FROM ShoppingList)";
                        filter += " AND ";
                        filter += " ArticleId NOT IN (SELECT ArticleId FROM StorageItem)";
                        break;

                    default:

                        filter += " (Article.Name LIKE ? OR Article.Manufacturer LIKE ? OR Article.Notes LIKE ? OR Article.Supermarket LIKE ?";
                        filter += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)";
                        parameter.Add("%" + textFilter + "%");
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

                filter += "ArticleId NOT IN (SELECT ArticleId FROM StorageItem)";
            }

            if (specialFilter > 0)
            {
                if (!string.IsNullOrEmpty(filter))
                    filter += " AND ";
                else
                    filter += " WHERE ";

                switch (specialFilter)
                {
                    case 1:
                        filter += " Article.Price IS NULL";
                        break;

                    case 2:
                        filter += " Article.Calorie IS NULL";
                        break;

                    case 3:
                        // Artikel zum [B]estellen.
                        filter += " ArticleId NOT IN (SELECT ArticleId FROM ShoppingList)";
                        filter += " AND ";
                        filter += " ArticleId NOT IN (SELECT ArticleId FROM StorageItem)";
                        break;
                }
            }
            
            string cmd = string.Empty;
            cmd += "SELECT ArticleId, Name, Manufacturer, Category, SubCategory, DurableInfinity, WarnInDays,";
            cmd += " Size, Unit, Notes, EANCode, Calorie, Price, StorageName, Supermarket,";
            cmd += " (SELECT Quantity FROM ShoppingList WHERE ShoppingList.ArticleId = Article.ArticleId) AS ShoppingListQuantity,";
            cmd += " (SELECT SUM(Quantity) FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId) AS StorageItemQuantity,";
            cmd += " (SELECT LENGTH(ImageSmall) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageSmallLength,";
            cmd += " (SELECT LENGTH(ImageLarge) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageLargeLength";
            cmd += " FROM Article";
            cmd += filter;
            cmd += " ORDER BY Name COLLATE NOCASE";

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            var result = DatabaseService.Instance.ExecuteQuery<ArticleQuantity>(cmd, parameter.ToArray<object>());

            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für Artikelliste: {0}", stopWatch.Elapsed.ToString());

            return result;
        }

        internal static IList<Article> GetArticlesByEanCode(string eanCode)
        {
            string cmd = string.Empty;

            cmd = string.Empty;
            cmd += "SELECT ArticleId";
            cmd += " FROM Article";
            cmd += " WHERE EANCode LIKE ?";
            
            return DatabaseService.Instance.ExecuteQuery<Article>(cmd, "%" + eanCode + "%");
        }

        internal static Article GetArticle(int articleId)
        {
            string cmd = string.Empty;

            cmd += "SELECT *";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            var articleList = DatabaseService.Instance.ExecuteQuery<Article>(cmd, articleId);

            return articleList.FirstOrDefault();
        }

        internal static string GetArticleName(int articleId)
        {
            string cmd = string.Empty;

            cmd += "SELECT Name";
            cmd += " FROM Article";
            cmd += " WHERE ArticleId = ?";

            return DatabaseService.Instance.ExecuteScalar<string>(cmd, articleId);
        }

        internal static int GetArticleCount()
        {
            string cmd = string.Empty;
            cmd += "SELECT COUNT(*)";
            cmd += " FROM Article";

            var result = DatabaseService.Instance.ExecuteScalar<int>(cmd);

            return result;
        }


        internal static decimal GetArticleCount_Abgelaufen()
        {
            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE BestBefore < date('now')";
            //cmd += " AND 1 = 2";

            var result = DatabaseService.Instance.ExecuteScalar<decimal?>(cmd);

            return result.HasValue ? result.Value : 0;
        }

        /// <summary>
        /// Artikel suchen, für die eine Warnung ausgegeben werden soll.
        /// </summary>
        /// <returns></returns>
        internal static decimal GetArticleCount_BaldZuVerbrauchen()
        {
            string cmd = string.Empty;
            cmd = string.Empty;
            cmd += "SELECT SUM(Quantity) AS Quantity";
            cmd += " FROM StorageItem";
            cmd += " JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE (date(BestBefore,  (-WarnInDays || ' day')) <= date('now'))";
            cmd += " AND BestBefore >= date('now')";
            cmd += " AND WarnInDays <> 0";
            //cmd += " OR 1 = 1";
            
            var result = DatabaseService.Instance.ExecuteScalar<decimal?>(cmd);

            return result.HasValue ? result.Value : 0;
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
            cmd += " ShoppingList.Quantity AS ShoppingListQuantity,";
            cmd += " (SELECT LENGTH(ImageSmall) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageSmallLength,";
            cmd += " (SELECT LENGTH(ImageLarge) FROM ArticleImage WHERE ArticleImage.ArticleId = Article.ArticleId AND ArticleImage.Type = 0 ) AS ImageLargeLength";
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
                filter += " OR Article.StorageName LIKE ? OR Article.Category LIKE ? OR Article.SubCategory LIKE ? OR Article.EANCode LIKE ?)";
                parameter.Add("%" + textFilter + "%");
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

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            result = DatabaseService.Instance.ExecuteQuery<StorageItemQuantityResult>(cmd, parameter.ToArray<object>());
            
            stopWatch.Stop();
            Tools.TRACE("Dauer der Abfrage für Lagerbestand: {0}", stopWatch.Elapsed.ToString());

            return result;
        }

        internal static IList<StorageItemQuantityResult> GetStorageItemQuantityList(int articleId)
        {
            string cmd = string.Empty;

            cmd += "SELECT StorageItem.*, Article.ArticleId, Article.WarnInDays";
            cmd += " FROM StorageItem";
            cmd += " LEFT JOIN Article ON StorageItem.ArticleId = Article.ArticleId";
            cmd += " WHERE StorageItem.ArticleId = ?";
            cmd += " ORDER BY BestBefore ASC";

            return DatabaseService.Instance.ExecuteQuery<StorageItemQuantityResult>(cmd, articleId);
        }

        internal static void DeleteArticle(int articleId)
        {
            //DatabaseService.Instance.BeginTransaction();

            DatabaseService.Instance.ExecuteNonQuery("DELETE FROM ShoppingList WHERE ArticleId = ?", articleId); 

            DatabaseService.Instance.ExecuteNonQuery("DELETE FROM ArticleImage WHERE ArticleId = ?", articleId);

            DatabaseService.Instance.ExecuteNonQuery("DELETE FROM Article WHERE ArticleId = ?",  articleId);

            // TODO: Transaktion umsetzen
            //DatabaseService.Instance.Commit();
        }

        internal static string GetSettingsString(string key)
        {
            string cmd = string.Empty;

            cmd += "SELECT Value";
            cmd += " FROM Settings";
            cmd += " WHERE Key = ?";

            return DatabaseService.Instance.ExecuteScalar<string>(cmd, key);
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

        internal static DateTime? GetSettingsDateTime(string key)
        {
            var dateText = Database.GetSettingsString(key);
            if (dateText == null)
                return null;

            try
            {
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
            string dateText = date.ToString("yyyy.MM.dd");

            Database.SetSettings(key, dateText);
        }

        internal static void SetSettingsDateTime(string key, DateTime date)
        {
            string dateText = date.ToString("yyyy.MM.dd HH:mm:ss");

            Database.SetSettings(key, dateText);
        }

        internal static void SetSettings(string key, string value)
        {
            string oldValue = GetSettingsString(key);

            if (oldValue == null)
            {
                string cmd = "INSERT INTO Settings (Key, Value) VALUES (?, ?)";

                DatabaseService.Instance.ExecuteNonQuery(cmd, key, value);
            }
            else
            {
                string cmd = "UPDATE Settings SET Value = ? WHERE Key = ?";

                DatabaseService.Instance.ExecuteNonQuery(cmd, value, key);
            }
        }

        internal static void ClearSettings(string key)
        {
            string cmd = string.Empty;

            cmd += "DELETE";
            cmd += " FROM Settings";
            cmd += " WHERE Key = ?";

            DatabaseService.Instance.ExecuteNonQuery(cmd, key);
        }
    }
}