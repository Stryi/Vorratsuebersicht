using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using Android.App;
using Android.Content.Res;

namespace VorratsUebersicht
{
    using SQLite;
    using static Tools;

    public class Android_Database
    {
        // http://err2solution.com/2016/05/sqlite-with-xamarin-forms-step-by-step-guide/

		public static bool UseTestDatabase = false;
        public static bool? IsDatabaseOnSdCard = null;

        public const string sqliteFilename_Prod = "Vorraete.db3";
        public const string sqliteFilename_New  = "Vorraete_db0.db3";
        public const string sqliteFilename_Demo = "Vorraete_Demo.db3";
        public const string sqliteFilename_Test = "Vorraete_Test.db3";

        public string GetProductiveDatabasePath()
        {
            bool sikop = Android_Database.UseTestDatabase;
            Android_Database.UseTestDatabase = false;

            var path = new Android_Database().GetDatabasePath();
            Android_Database.UseTestDatabase = sikop;

            return path;
        }

		public string GetDatabasePath()
		{
            string databaseName = Android_Database.sqliteFilename_Prod;

			if (Android_Database.UseTestDatabase)
            {
            	databaseName = Android_Database.sqliteFilename_Test;
            }

            //
            // Zuerst prüfen, ob auf der SD Karte die Datenbank ist.
            //
			string sdCardPath = this.GetSdCardPath();
            string databaseFileName = Path.Combine(sdCardPath, databaseName);

            if (File.Exists(databaseFileName))
            {
                Android_Database.IsDatabaseOnSdCard = true;
                return databaseFileName;
            }

            //
            // Jetzt prüfen, ob imr Applikationsverzeichnis die Datenbank da ist.
            //
            string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            databaseFileName = Path.Combine(documentsPath, databaseName);

			if (File.Exists(databaseFileName))
            {
                Android_Database.IsDatabaseOnSdCard = false;
                return databaseFileName;
            }

            Android_Database.IsDatabaseOnSdCard = null;
            return null;
		}

        public string GetDatabaseInfoText(string format)
        {
            string databaseName = new Android_Database().GetDatabasePath();
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

        /*
        public void DeleteDatabase()
        {
            string dbPath = GetDatabasePath();

            File.Delete(dbPath);

            string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            this.CreateDatabaseIfNotExists(documentsPath,  Android_Database.sqliteFilename_New,  Android_Database.sqliteFilename_Prod, false);
        }
        */

        public bool IsCurrentDatabaseExists()
		{
			var path = new Android_Database().GetDatabasePath();

			return File.Exists(path);
		}

		public bool CopyDatabaseToSDCard(bool overrideDatabase)
        {
			string documentsPath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
			string sdCardPath    = this.CreateAndGetSdCardPath();

			string source      = Path.Combine(documentsPath, Android_Database.sqliteFilename_Prod);
            string destination = Path.Combine(sdCardPath,    Android_Database.sqliteFilename_Prod);

            if (File.Exists(destination) && !overrideDatabase)
            {
                return false;
            }


			try
			{
                if (overrideDatabase)
                {
                    File.Delete(destination);
                }

				File.Copy(source, destination);
			}
			catch (Exception e)
			{
				TRACE(e);
				return false;
			}
            
            return true;
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

        
        /*
		public void RestoreDatabase_Prod_Db0(bool overrideDatabase)
        {
            var databaseName = new Android_Database().GetDatabasePath();
            
			string databasePath = Path.GetDirectoryName(databaseName);

            string source      = Path.Combine(databasePath, Android_Database.sqliteFilename_New);
			string destination = Path.Combine(databasePath, Android_Database.sqliteFilename_Prod);

            this.RestoreDatabase(source, destination, overrideDatabase);
        }
        */

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

        private string GetSdCardPath()
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

			this.UpdateDatabase(conn);

            Android_Database.SQLiteConnection = conn;

			// Return the database connection 
			return conn;
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
    }
}