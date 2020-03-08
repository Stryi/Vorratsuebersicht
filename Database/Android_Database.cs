using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Android.App;

namespace VorratsUebersicht
{
    using SQLite;
    using static Tools;

    public class Android_Database
    {
        // http://err2solution.com/2016/05/sqlite-with-xamarin-forms-step-by-step-guide/

        public static bool UseTestDatabase = false;
        public static bool UseAppFolderDatabase = false;
        public static bool? IsDatabaseOnSdCard = null;

        public static string SelectedDatabaseName = string.Empty;

        public const string sqliteFilename_Prod = "Vorraete.db3";
        public const string sqliteFilename_New  = "Vorraete_db0.db3";
        public const string sqliteFilename_Demo = "Vorraete_Demo.db3";
        public const string sqliteFilename_Test = "Vorraete_Test.db3";

        private static Android_Database instance = null;
        public static Android_Database Instance
        {
            get
            {
                if (Android_Database.instance == null)
                {
                    Android_Database.instance = new Android_Database();
                }
                return Android_Database.instance;
            }
        }

        private Android_Database()
        {

        }

        public string GetProductiveDatabasePath()
        {
            bool sikop = Android_Database.UseTestDatabase;
            Android_Database.UseTestDatabase = false;

            var path = Android_Database.Instance.GetDatabasePath();
            Android_Database.UseTestDatabase = sikop;

            return path;
        }

        public string GetSdCardDatabasePath()
        {
            string databaseFileName = Path.Combine(this.GetSdCardPath(), 
                                        Android_Database.SelectedDatabaseName);

            if (!File.Exists(databaseFileName))
            {
                return null;
            }

            return databaseFileName;
        }

        public string GetAppFolderDatabasePath()
        {
            string databaseFileName = Path.Combine(
                Environment.GetFolderPath (Environment.SpecialFolder.Personal),
                Android_Database.sqliteFilename_Prod);

            if (!File.Exists(databaseFileName))
            {
                return null;
            }

            return databaseFileName;
        }

		public string GetDatabasePath()
		{
            string databasePath;
            string databaseFileName;

            //
            // Test Datenbank in Optionen ausgewählt?
            //
			if (Android_Database.UseTestDatabase)
            {
                databasePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
                databaseFileName = Path.Combine(databasePath, Android_Database.sqliteFilename_Test);

                Android_Database.IsDatabaseOnSdCard = false;
                return databaseFileName;
            }

            //
            // App Datenbank in Optionen ausgewählt?
            //
            if (Android_Database.UseAppFolderDatabase)
            {
                databasePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
                databaseFileName = Path.Combine(databasePath, Android_Database.sqliteFilename_Prod);

                Android_Database.IsDatabaseOnSdCard = false;
                return databaseFileName;
            }

            //
            // Datenbank beim Starten der Anwendung ausgewählt?
            //
            if (!string.IsNullOrEmpty(Android_Database.SelectedDatabaseName))
            {
                databaseFileName = Android_Database.SelectedDatabaseName;

                Android_Database.IsDatabaseOnSdCard = true;   // Ja, ist nicht so eindeutig...
                return databaseFileName;
            }    

            //
            // Datenbank (bereits) auf der SD Karte?
            //
			string sdCardPath = this.GetSdCardPath();
            databaseFileName = Path.Combine(sdCardPath, Android_Database.sqliteFilename_Prod);

            if (File.Exists(databaseFileName))
            {
                Android_Database.IsDatabaseOnSdCard = true;
                return databaseFileName;
            }

            //
            // Die App Datenbank auswählen.
            //
            databasePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            databaseFileName = Path.Combine(databasePath, Android_Database.sqliteFilename_Prod);

            Android_Database.IsDatabaseOnSdCard = false;
            return databaseFileName;
		}

        public string GetDatabaseInfoText(string format)
        {
            string databaseName = Android_Database.Instance.GetDatabasePath();
            if (databaseName == null)
                return string.Empty;

            FileInfo fileInfo = new FileInfo(databaseName);

            string info = string.Format(format, databaseName, Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

            return info;
        }

        public void CompressDatabase()
        {
            SQLite.SQLiteConnection databaseConnection = this.GetConnection();
            if (databaseConnection == null)
                return;

            databaseConnection.Execute("VACUUM");
        }

        public string RepairDatabase()
        {
			string path = GetDatabasePath();
            if (path == null)
                return "Keine Datenbank angegeben.";

			// This is where we copy in the prepopulated database
			TRACE("Database Path: {0}", path);
			if (!File.Exists(path))
                return "Datenbank Datei existiert nicht.";

			var conn = new SQLite.SQLiteConnection(path, false);

            var checkResult = conn.Query<IntegrityCheck>("PRAGMA integrity_check");

            if (checkResult.Count == 0)
                return "Kein Ergebnis beim PRAGMA integrity_check geliefert.";

            return checkResult[0].integrity_check;
        }

        public bool IsCurrentDatabaseExists()
		{
			var path = Android_Database.Instance.GetDatabasePath();

			return File.Exists(path);
		}

		public Exception CopyDatabaseToSDCard(bool overrideDatabase)
        {
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			string sdCardPath    = this.CreateAndGetSdCardPath();

			string source      = Path.Combine(documentsPath, Android_Database.sqliteFilename_Prod);
            string destination = Path.Combine(sdCardPath,    Android_Database.sqliteFilename_Prod);

            if (File.Exists(destination) && !overrideDatabase)
            {
                return null;
            }


			try
			{
                // Test eines Absturzes
                //Activity test = null; test.GetType();

                if (overrideDatabase)
                {
                    File.Delete(destination);
                }

                File.Copy(source, destination);

                // Datenbankverbindung neu öffnen
                Android_Database.SQLiteConnection = null;
			}
			catch (Exception e)
			{
				TRACE(e);
				return e;
			}
            
            return null;
        }

        /// <summary>
        /// Datenbank von der SD Karte restaurieren
        /// </summary>
		public void RestoreDatabase_Test_Sample(bool overrideDatabase)
        {
            //  /data/data/de.stryi.Vorratsuebersicht/files/Vorraete_Demo.db3
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            string source      = Path.Combine(documentsPath, Android_Database.sqliteFilename_Demo);

            //  /data/data/de.stryi.Vorratsuebersicht/files/Vorraete_Test.db3
			string destination = Path.Combine(documentsPath,  Android_Database.sqliteFilename_Test);

            this.RestoreDatabase(source, destination, overrideDatabase);

            this.PrepareTestDatabase(destination);
        }

        public void RestoreDatabase_Test_Db0(bool overrideDatabase)
        {
            //  /data/data/de.stryi.Vorratsuebersicht/files/Vorraete_Db0.db3
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            string source      = Path.Combine(documentsPath, Android_Database.sqliteFilename_New);

            //  /data/data/de.stryi.Vorratsuebersicht/files/Vorraete_Test.db3
			string destination = Path.Combine(documentsPath, Android_Database.sqliteFilename_Test);

            this.RestoreDatabase(source, destination, overrideDatabase);
        }

        /// <summary>
        /// Haltbarkeitsdatum der Testdaten aktualisieren
        /// </summary>
        /// <param name="destination"></param>
        private void PrepareTestDatabase(string destination)
        {
			var conn = new SQLite.SQLiteConnection(destination, false);

            DateTime bestBefore = new DateTime(2000, 1, 1);
            TimeSpan span =  DateTime.Today - bestBefore;
            int daysAdd = (int)span.TotalDays;

            string cmd = string.Format("UPDATE StorageItem SET BestBefore = date(BestBefore, '+{0} day')", daysAdd);
            conn.Execute(cmd);
        }

        
        private bool RestoreDatabase(string source, string destination, bool overrideDestination)
        {
            if (File.Exists(source))
            {
                if (File.Exists(destination) && overrideDestination)
                    File.Delete(destination);

                if (!File.Exists(destination))
                {
                    try
                    {
                        File.Copy(source, destination);
                    }
                    catch (Exception e)
                    {
                        TRACE(e);
                        return false;
                    }
                    return true;
                }
            }
            
            return false;
        }
                
        /// <summary>
        /// Datenbanken aus den Resourcen erstellen.
        /// </summary>
		public void RestoreSampleDatabaseFromResources()
		{
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);

            this.CreateDatabaseIfNotExists(documentsPath,  Android_Database.sqliteFilename_New,  Android_Database.sqliteFilename_Prod, false);
            this.CreateDatabaseIfNotExists(documentsPath,  Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Test, true);
            this.CreateDatabaseIfNotExists(documentsPath,  Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Demo, false);
            this.CreateDatabaseIfNotExists(documentsPath,  Android_Database.sqliteFilename_New,  Android_Database.sqliteFilename_New,  false);

			string sdCardPath = this.CreateAndGetSdCardPath();
            if (!string.IsNullOrEmpty(sdCardPath))
            {
                this.CopyDatabaseToSDCard(false);
            }
        }

        public string GetSdCardPath()
        {
            string sdCardPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            sdCardPath = Path.Combine(sdCardPath, "Vorratsuebersicht");

            return sdCardPath;
        }

        private string CreateAndGetSdCardPath()
        {
            if (!Android.OS.Environment.ExternalStorageDirectory.CanWrite())
                return null;

            string sdCardPath = this.GetSdCardPath();

            if (!Directory.Exists(sdCardPath))
                Directory.CreateDirectory(sdCardPath);

            return sdCardPath;
        }

        public static SQLite.SQLiteConnection SQLiteConnection = null;

		public SQLite.SQLiteConnection GetConnection()
		{
            if (Android_Database.SQLiteConnection != null)
                return Android_Database.SQLiteConnection;

			string path = GetDatabasePath();
            if (path == null)
                return null;

			// This is where we copy in the prepopulated database
			TRACE("Database Path: {0}", path);
			if (!File.Exists(path))
				return null;

			var conn = new SQLite.SQLiteConnection(path, false);
            //conn.Trace = true;

			string cmd = "PRAGMA journal_mode=MEMORY";
			IList<JournalMode> tableInfo = conn.Query<JournalMode>(cmd);
            if (tableInfo.Count > 0)
            {
                TRACE("PRAGMA journal_mode={0}", tableInfo[0].journal_mode);
            }

			this.UpdateDatabase(conn);

            Android_Database.SQLiteConnection = conn;

			// Return the database connection 
			return conn;
		}

        public void CloseConnection()
        {
            if (Android_Database.SQLiteConnection != null)
            {
                Android_Database.SQLiteConnection.Close();
                Android_Database.SQLiteConnection = null;
            }
        }

		private void UpdateDatabase(SQLiteConnection conn)
		{
			// Update 1.21: Kalorien
			if (!this.IsFieldInTheTable(conn, "Article", "Calorie"))
			{
				conn.Execute("ALTER TABLE Article ADD COLUMN Calorie INTEGER");
			}

            // Update 2.00: Einkaufswagen
            if (!this.IsTableInDatabase(conn, "ShoppingList"))
            {
                string cmd = string.Empty;

                cmd += "CREATE TABLE [ShoppingList] (";
                cmd += " [ShoppingListId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,";
                cmd += " [ArticleId] INTEGER CONSTRAINT[FK_Article] REFERENCES[Article], ";
                cmd += " [Quantity] NUMERIC);";

                conn.Execute(cmd);
            }

            // Update 2.10: Lagerort, Mindestbestand, Einkaufsmarkt
            if (!this.IsFieldInTheTable(conn, "Article", "StorageName"))
            {
                conn.Execute("ALTER TABLE Article ADD COLUMN [MinQuantity] INTEGER");
                conn.Execute("ALTER TABLE Article ADD COLUMN [PrefQuantity] INTEGER");
                conn.Execute("ALTER TABLE Article ADD COLUMN [StorageName] TEXT");
                conn.Execute("ALTER TABLE Article ADD COLUMN [Supermarket] TEXT");

                //conn.Execute("DROP   INDEX [Article_StorageName]");
                conn.Execute("CREATE INDEX [Article_StorageName] ON [Article] ([StorageName] COLLATE NOCASE ASC);");
                //conn.Execute("DROP   INDEX [Article_Calorie]");
                conn.Execute("CREATE INDEX [Article_Calorie]     ON [Article] ([Calorie]     COLLATE NOCASE ASC);");
            }

            // Update 2.22: Preis
            if (!this.IsFieldInTheTable(conn, "Article", "Price"))
            {
                conn.Execute("ALTER TABLE Article ADD COLUMN [Price] MONEY");
            }

            // Update 2.34: Einstellungen (z.B. für zusätzliche eigene Kategorien)
            if (!this.IsTableInDatabase(conn, "Settings"))
            {
                string cmd = string.Empty;

                cmd += "CREATE TABLE [Settings] (";
                cmd += " [SettingsId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,";
                cmd += " [Key] TEXT, ";
                cmd += " [Value] TEXT);";

                conn.Execute(cmd);
            }

            // Update 4.00: Extra Tabelle für Bilder
            if (!this.IsTableInDatabase(conn, "ArticleImage"))
            {
                string cmd = 
                    "CREATE TABLE [ArticleImage] (" +
                    " [ImageId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " [ArticleId] INTEGER NOT NULL," +
                    " [Type] INTEGER NOT NULL," +     // 0 - Artikelbild(-er), 1 - z.B. Rüchsicht, 3 - Zutaten
                    " [CreatedAt] DATETIME NOT NULL," +   // Zum Sortieren gedacht
                    " [ImageSmall] IMAGE," +
                    " [ImageLarge] IMAGE);";

                conn.Execute(cmd);
            }

            // Update 4.10: Gekauft in Einkaufsliste
            if (!this.IsFieldInTheTable(conn, "ShoppingList", "Bought"))
            {
                conn.Execute("ALTER TABLE ShoppingList ADD COLUMN [Bought] BOOLEAN");
            }

            // Update 4.30
            if (!this.IsFieldInTheTable(conn, "StorageItem", "StorageName"))
            {
                conn.Execute("ALTER TABLE StorageItem ADD COLUMN [StorageName] TEXT");
            }
        }

        public IList<ArticleData> GetArticlesToCopyImages(SQLite.SQLiteConnection databaseConnection)
        {
            // Noch keine Datenbank angelegt?
            if (databaseConnection == null)
                return new List<ArticleData>();

            // Artikelbilder ermitteln, die noch nicht übertragen wurden.
            var articleImagesToCopy = databaseConnection.Query<ArticleData>(
                "SELECT ArticleId, Name" +
                " FROM Article" +
                " WHERE Image IS NOT NULL" +
                " AND ArticleId NOT IN (SELECT ArticleId FROM ArticleImage)" +
                " ORDER BY Name");

            return articleImagesToCopy;
        }

        private bool IsFieldInTheTable(SQLiteConnection conn, string tableName, string fieldName)
		{
			string cmd = string.Format("PRAGMA table_info({0})", tableName);
			IList<table_info> tableInfo = conn.Query<table_info>(cmd);
			var field = tableInfo.FirstOrDefault(e => e.name.Equals(fieldName, StringComparison.InvariantCultureIgnoreCase));
			return (field != null);
		}

        private bool IsTableInDatabase(SQLiteConnection conn, string tableName)
        {
            string cmd = string.Format("SELECT name FROM sqlite_master WHERE type = 'table' AND name = '{0}'", tableName);
            IList<table_info> tableInfo = conn.Query<table_info>(cmd);
            return (tableInfo.Count > 0);
        }

        /// <summary>
        /// Erstellt Datenbank aus den Resourcen.
        /// </summary>
        /// <seealso>http://arteksoftware.com/deploying-a-database-file-with-a-xamarin-forms-app<seealso>
        /// <param name="path"></param>
        /// <param name="fileName"></param>
        private void CreateDatabaseIfNotExists(string path, string fileName, string destinationFileName, bool prepareTestData)
        {
			string dbPath = Path.Combine(path, destinationFileName);

            if (File.Exists (dbPath))
                return;

            using (var br = new BinaryReader(Application.Context.Assets.Open(fileName)))
            {
                using (var bw = new BinaryWriter(new FileStream(dbPath, FileMode.Create)))
                {
                    byte[] buffer = new byte[2048];
                    int length = 0;
                    while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                    {
                        bw.Write(buffer, 0, length);
                    }
                }
            }

            if (prepareTestData)
                this.PrepareTestDatabase(dbPath);

            return;
        }

        public bool CheckWrongDatabase() 
        {

            try
            {
                int? sdArticleCount  = null;
                int? appArticleCount = null;

                // SD Karten Datenbank Pfad (falls vorhanden)
                string dbFileName = Android_Database.Instance.GetSdCardDatabasePath();
                if ((dbFileName == null) || !Android.OS.Environment.ExternalStorageDirectory.CanWrite())
                {
                    // Keine Datenbank oder kein Zugriff auf die SD Karte.
                    return false;
                }
                
                // Anzahl Artikel auf der SD Karte.
                var conn = new SQLite.SQLiteConnection(dbFileName, false);
                sdArticleCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Article");

                if (sdArticleCount > 0)
                {
                    // Auf der SD Karte sind schon Daten erfasst. => Alles OK.
                    return false;
                }

                // DB im Applikationsverzeichnis?
                dbFileName = Android_Database.Instance.GetAppFolderDatabasePath();
                if (dbFileName == null)
                {
                    // Noch keine Datenbank angelegt (Erstaufruf?).
                    return false;
                }
                
                // Anzahl Artikel im App-Verzeichnis.
                conn = new SQLite.SQLiteConnection(dbFileName, false);
                appArticleCount = conn.ExecuteScalar<int>("SELECT COUNT(*) FROM Article");

                if (appArticleCount == 0)
                {
                    // Keine Artikel für die Übernahme
                    return false;
                }

                // Fehlerfall entdeckt
                return true;
            }
            catch(Exception e)
            {
                TRACE(e);
            }

            return false;
        }

    }
}