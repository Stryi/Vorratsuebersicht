using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace VorratsUebersicht
{
    internal class DatabaseService
    {
        public event EventHandler<EventArgs> Progress;

        private static IDatabase instance = null;

        private DatabaseService() { }

        internal static string databasePath;
        internal static DatabaseService.Database database;

        private static DatabaseType _databaseType;
        internal static DatabaseType databaseType
        {
            set { DatabaseService.instance = null; DatabaseService._databaseType = value;}
            get { return DatabaseService._databaseType; }
        }

        public enum DatabaseType
        {
            None,
            Local,
            Server
        }

        public static IDatabase Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new DatabaseService().GetDatabase();

                    if (instance == null)
                        return null;

                    // Ggf. Datenbank auf den neuesten Stand bringen.
                    DatabaseService.UpdateDatabase(instance);
                }
                return instance;
            }
        }

        private IDatabase GetDatabase()
        {
            switch(DatabaseService.databaseType)
            {
                case DatabaseType.Local:
                    LocalDatabase.databaseFileName = DatabaseService.databasePath;
                    return new LocalDatabase();

                case DatabaseType.Server:
                    var database = new ServerDatabase();
                    database.database = DatabaseService.database;
                    return database;

                default:
                    return null;
            }
        }

        internal static List<DatabaseService.Database> GetDatabases(Android.Content.Context context, ref Exception exception)
        {
            var databaseList = LocalDatabase.GetDatabases(context, ref exception);

            var serverDatabaseList = ServerDatabase.GetDatabases(ref exception);

            databaseList.AddRange(serverDatabaseList);

            return databaseList;
        }

        internal static string TryOpenDatabase(Database database)
        {
            string error = null;

            if (database.Type == DatabaseType.Local)
            {
                error = LocalDatabase.TryOpenDatabase(database);

                if (string.IsNullOrEmpty(error))
                {
                    // Es hat geklappt. Die Datenbank merken...
                    DatabaseService.databasePath = database.Location;
                    DatabaseService.databaseType = DatabaseService.DatabaseType.Local;

                    Settings.PutString("LastSelectedDatabase",     DatabaseService.databasePath);
                    Settings.PutInt   ("LastSelectedDatabaseType", (int)DatabaseService.databaseType);
                }
            }

            if (database.Type == DatabaseType.Server)
            {
                error = ServerDatabase.TryOpenDatabase(database);
                if (string.IsNullOrEmpty(error))
                {
                    // Es hat geklappt. Die Datenbank merken...
                    DatabaseService.database     = database;
                    DatabaseService.databaseType = DatabaseService.DatabaseType.Server;

                    Settings.PutString("LastSelectedDatabase",     DatabaseService.database.Location);
                    Settings.PutString("LastSelectedDatabaseName", DatabaseService.database.Name);
                    Settings.PutInt   ("LastSelectedDatabaseType", (int)DatabaseService.databaseType);
                }
            }

            return error;
        }

        internal static Exception DeleteDatabase(Database database)
        {
            Exception error = null;

            if (database.Type == DatabaseType.Local)
            {
                error = LocalDatabase.DeleteDatabase(database.Location);
            }

            if (database.Type == DatabaseType.Server)
            {
                error = ServerDatabase.DeleteDatabase(database.Location);
            }

            return error;
        }


		private static void UpdateDatabase(IDatabase conn)
		{
			// Update 1.21: Kalorien
			if (!DatabaseService.IsFieldInTheTable(conn, "Article", "Calorie"))
			{
				conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN Calorie INTEGER");
			}

            // Update 2.00: Einkaufswagen
            if (!DatabaseService.IsTableInDatabase(conn, "ShoppingList"))
            {
                string cmd = string.Empty;

                cmd += "CREATE TABLE [ShoppingList] (";
                cmd += " [ShoppingListId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,";
                cmd += " [ArticleId] INTEGER CONSTRAINT[FK_Article] REFERENCES[Article], ";
                cmd += " [Quantity] NUMERIC);";

                conn.ExecuteNonQuery(cmd);
            }

            // Update 2.10: Lagerort, Mindestbestand, Einkaufsmarkt
            if (!DatabaseService.IsFieldInTheTable(conn, "Article", "StorageName"))
            {
                conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN [MinQuantity] INTEGER");
                conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN [PrefQuantity] INTEGER");
                conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN [StorageName] TEXT");
                conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN [Supermarket] TEXT");

                //conn.Execute("DROP   INDEX [Article_StorageName]");
                conn.ExecuteNonQuery("CREATE INDEX [Article_StorageName] ON [Article] ([StorageName] COLLATE NOCASE ASC);");
                //conn.Execute("DROP   INDEX [Article_Calorie]");
                conn.ExecuteNonQuery("CREATE INDEX [Article_Calorie]     ON [Article] ([Calorie]     COLLATE NOCASE ASC);");
            }

            // Update 2.22: Preis
            if (!DatabaseService.IsFieldInTheTable(conn, "Article", "Price"))
            {
                conn.ExecuteNonQuery("ALTER TABLE Article ADD COLUMN [Price] MONEY");
            }

            // Update 2.34: Einstellungen (z.B. für zusätzliche eigene Kategorien)
            if (!DatabaseService.IsTableInDatabase(conn, "Settings"))
            {
                string cmd = string.Empty;

                cmd += "CREATE TABLE [Settings] (";
                cmd += " [SettingsId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,";
                cmd += " [Key] TEXT, ";
                cmd += " [Value] TEXT);";

                conn.ExecuteNonQuery(cmd);
            }

            // Update 4.00: Extra Tabelle für Bilder
            if (!DatabaseService.IsTableInDatabase(conn, "ArticleImage"))
            {
                string cmd = 
                    "CREATE TABLE [ArticleImage] (" +
                    " [ImageId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " [ArticleId] INTEGER NOT NULL," +
                    " [Type] INTEGER NOT NULL," +     // 0 - Artikelbild(-er), 1 - z.B. Rüchsicht, 3 - Zutaten
                    " [CreatedAt] DATETIME NOT NULL," +   // Zum Sortieren gedacht
                    " [ImageSmall] IMAGE," +
                    " [ImageLarge] IMAGE);";

                conn.ExecuteNonQuery(cmd);
            }

            // Update 4.10: Gekauft in Einkaufsliste
            if (!DatabaseService.IsFieldInTheTable(conn, "ShoppingList", "Bought"))
            {
                conn.ExecuteNonQuery("ALTER TABLE ShoppingList ADD COLUMN [Bought] BOOLEAN");
            }

            // Update 4.30
            if (!DatabaseService.IsFieldInTheTable(conn, "StorageItem", "StorageName"))
            {
                conn.ExecuteNonQuery("ALTER TABLE StorageItem ADD COLUMN [StorageName] TEXT");
            }
        }

        private static bool IsFieldInTheTable(IDatabase conn, string tableName, string fieldName)
		{
			string cmd = string.Format("PRAGMA table_info({0})", tableName);
			IList<table_info> tableInfo = conn.ExecuteQuery<table_info>(cmd);
			var field = tableInfo.FirstOrDefault(e => e.name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
			return (field != null);
		}

        private static bool IsTableInDatabase(IDatabase conn, string tableName)
        {
            string cmd = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{0}'", tableName);
            IList<table_info> tableInfo = conn.ExecuteQuery<table_info>(cmd);
            return (tableInfo.Count > 0);
        }


        [DebuggerDisplay("{Name} - {Path}")]
        public class Database
        {
            public string Name;
            public string Location;
            public string DatabaseName;
            public DatabaseType Type;

            internal static void RemoveDatabaseFromList(ref List<DatabaseService.Database> fileList, string databaseToRemove)
            {
                if (string.IsNullOrEmpty(databaseToRemove))
                    return;

                var testEntryToRemove = fileList.FirstOrDefault(e => e.Location == databaseToRemove);
                if (testEntryToRemove != null)
                {
                    if (fileList.Contains(testEntryToRemove))
                        fileList.Remove(testEntryToRemove);
                }
            }

        }
    }
}
