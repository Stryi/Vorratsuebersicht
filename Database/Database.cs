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
using SQLite;

namespace VorratsUebersicht
{
    public static class Database
    {
		internal static void UpdateStorageItemQuantity(StorageItemQuantityResult storageItem)
		{
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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
					cmd += "UPDATE StorageItem SET Quantity = ? WHERE StorageItemId = ?";
					command = databaseConnection.CreateCommand(cmd, new object[] { storageItem.Quantity, storageItem.StorageItemId});
				}
            }

            int result = command.ExecuteNonQuery();

		}

        internal static IList<ShoppingItemListResult> GetShoppingItemList()
        {
            List<ShoppingItemListResult> result = new List<ShoppingItemListResult>();

            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            SQLiteCommand command;

            cmd += "SELECT ShoppingListId, Article.ArticleId, Name, Manufacturer, Size, Unit, Calorie, Quantity";
            cmd += " FROM ShoppingList";
            cmd += " LEFT JOIN Article ON ShoppingList.ArticleId = Article.ArticleId";
            cmd += " ORDER BY Name";

            command = databaseConnection.CreateCommand(cmd, new object[] { });

            return command.ExecuteQuery<ShoppingItemListResult>();
        }

        internal static double AddToShoppingList(int articleId, double addQuantity)
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return 0;

            SQLiteCommand command;
            string cmd = string.Empty;


            cmd = "SELECT Quantity FROM ShoppingList WHERE ArticleId = ?";
            command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            double isQuantity = command.ExecuteScalar<double>();

            double newQuantity = isQuantity + addQuantity;

            bool isInList = Database.IsArticleInStoppingList(articleId);
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

        internal static void RemoveFromShoppingList(int shoppingListId)
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return;

            SQLiteCommand command;
            string cmd = string.Empty;

            cmd += "DELETE FROM ShoppingList WHERE ShoppingListId = ?";
            command = databaseConnection.CreateCommand(cmd, new object[] { shoppingListId });
            command.ExecuteNonQuery();
        }

        internal static bool IsArticleInStoppingList(int articleId)
        {
            SQLiteConnection databaseConnection = new Android_Database().GetConnection();

            // Artikel suchen, die schon abgelaufen sind.
            string cmd = string.Empty;
            cmd += "SELECT COUNT(*)";
            cmd += " FROM ShoppingList";
            cmd += " WHERE ArticleId = ?";

            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });
            int count = command.ExecuteScalar<int>();

            return count > 0;
        }

        internal static IList<StorageItemQuantityResult> GetBestBeforeItemQuantity(StorageItemQuantityResult storegeItem)
		{
            IList<StorageItemQuantityResult> result = new List<StorageItemQuantityResult>();

            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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

            command = databaseConnection.CreateCommand(cmd, new object[] { storegeItem.ArticleId });

            return command.ExecuteQuery<StorageItemQuantityResult>();
		}

        internal static Article GetArticleImage(int articleId, bool showLarge)
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return null;

            string cmd = string.Empty;

            if (showLarge)
                cmd += "SELECT ImageLarge AS Image";
            else
                cmd += "SELECT Image";

			cmd += " FROM Article";
			cmd += " WHERE ArticleId = ?";


            var command = databaseConnection.CreateCommand(cmd, new object[] { articleId });

            return command.ExecuteQuery<Article>().First();
        }

        internal static decimal GetArticleQuantityInStorage(int articleId)
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();

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

        internal static string[] GetCategories()
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();

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
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();

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

        internal static IList<Article> GetArticleListNoImages(string category, string subCategory)
        {
            IList<Article> result = new Article[0];

            var databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return result;

            IList<object> parameter = new List<object>();

            string cmd = string.Empty;
            cmd += "SELECT ArticleId, Name, Manufacturer, Category, SubCategory, DurableInfinity, WarnInDays, Size, Unit, Calorie, Notes, EANCode";
			cmd += " FROM Article";

            if (!string.IsNullOrEmpty(category))
            {
    			cmd += " WHERE Article.Category = ?";
                parameter.Add(category);
            }

            if (!string.IsNullOrEmpty(subCategory))
            {
                if (parameter.Count > 0)
                    cmd += " AND ";
                else
                    cmd += " WHERE ";

    			cmd += " Article.SubCategory = ?";
                parameter.Add(subCategory);
            }

			cmd += " ORDER BY Name COLLATE NOCASE";

            var command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());
            result = command.ExecuteQuery<Article>();
            
            return result;
        }

        internal static IList<Article> GetArticlesByEanCode(string eanCode)
        {
            IList<Article> result = new List<Article>();

            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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
        /// Artikel suchen, für die eine Warnung ausgegeben werden soll.
        /// </summary>
        /// <returns></returns>
        internal static decimal GetArticleCount_BaldZuVerbrauchen()
        {
            SQLite.SQLiteConnection databaseConnection = new Android_Database().GetConnection();
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

        internal static IList<StorageItemQuantityResult> GetStorageItemQuantityListNoImage(string category, string subCategory, string eanCode, bool showNotInStorageArticles)
        {
            var result = new List<StorageItemQuantityResult>();

            var databaseConnection = new Android_Database().GetConnection();
            if (databaseConnection == null)
                return result;

            string cmd = string.Empty;
            cmd += "SELECT ArticleId, Name, WarnInDays, Size, Unit, DurableInfinity,";
			cmd += " (SELECT SUM(Quantity) FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId) AS Quantity,";
			cmd += " (SELECT BestBefore FROM StorageItem WHERE StorageItem.ArticleId = Article.ArticleId ORDER BY BestBefore ASC LIMIT 1) AS BestBefore";
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

            cmd += filter;
            cmd += " ORDER BY Article.Name COLLATE NOCASE";

            var command = databaseConnection.CreateCommand(cmd, parameter.ToArray<object>());
            return command.ExecuteQuery<StorageItemQuantityResult>();
        }

        internal static IList<StorageItemQuantityResult> GetStorageItemQuantityList(int articleId)
        {
            var result = new List<StorageItemQuantityResult>();

            var databaseConnection = new Android_Database().GetConnection();
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
    }
}