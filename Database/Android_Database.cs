using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

using Android.App;
using Android.Content;

namespace VorratsUebersicht
{
    using Android.Content.Res;
    using SQLite;
    using System.Runtime.CompilerServices;
    using static Tools;
    using static VorratsUebersicht.DatabaseService;

    public class Android_Database
    {
        // http://err2solution.com/2016/05/sqlite-with-xamarin-forms-step-by-step-guide/

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

		public string GetDatabasePath()
		{
            string databasePath;
            string databaseFileName;

            //
            // Datenbank beim Starten der Anwendung ausgewählt?
            //
            if (!string.IsNullOrEmpty(Android_Database.SelectedDatabaseName))
            {
                databaseFileName = Android_Database.SelectedDatabaseName;

                return databaseFileName;
            }    

            //
            // Die App Datenbank auswählen.
            // (sollte dort eigentlich nicht existieren)
            //
            // /data/user/0/de.stryi.Vorratsuebersicht/files/Vorraete.db3
            //
            databasePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            databaseFileName = Path.Combine(databasePath, Android_Database.sqliteFilename_Prod);
            
            if (File.Exists(databaseFileName))
            {
                return databaseFileName;
            }

            // Öffne dann die zuletzt verwendete Datenbank.
            string lastSelectedDatabase = Settings.GetString("LastSelectedDatabase", null);
            if (!string.IsNullOrEmpty(lastSelectedDatabase))
            {
                if (File.Exists(lastSelectedDatabase))
                {
                    return lastSelectedDatabase;
                }
            }
            
            return null;
		}


        public void CompressDatabase()
        {
            DatabaseService.Instance.ExecuteNonQuery("UPDATE Article SET Manufacturer = RTRIM(Manufacturer) WHERE LENGTH(Manufacturer) <> LENGTH(TRIM(Manufacturer))");
            DatabaseService.Instance.ExecuteNonQuery("UPDATE Article SET SubCategory  = RTRIM(SubCategory)  WHERE LENGTH(SubCategory)  <> LENGTH(TRIM(SubCategory))");
            DatabaseService.Instance.ExecuteNonQuery("UPDATE Article SET StorageName  = RTRIM(StorageName)  WHERE LENGTH(StorageName)  <> LENGTH(TRIM(StorageName))");       
            DatabaseService.Instance.ExecuteNonQuery("UPDATE Article SET Supermarket  = RTRIM(Supermarket)  WHERE LENGTH(Supermarket)  <> LENGTH(TRIM(Supermarket))");

            DatabaseService.Instance.ExecuteNonQuery("UPDATE StorageItem SET StorageName = RTRIM(StorageName) WHERE LENGTH(StorageName) <> LENGTH(TRIM(StorageName))");

            DatabaseService.Instance.ExecuteNonQuery("VACUUM");
        }

        public string RepairDatabase()
        {
			string path = GetDatabasePath();
            if (path == null)
                return "Keine Datenbank angegeben.";

			// This is where we copy in the prepopulated database
			TRACE("Database Path: {0}", path);

			var conn = new SQLite.SQLiteConnection(path, false);

            var checkResult = conn.Query<IntegrityCheck>("PRAGMA integrity_check");

            if (checkResult.Count == 0)
                return "Kein Ergebnis beim PRAGMA integrity_check geliefert.";

            return checkResult[0].integrity_check;
        }

		public Exception CreateDatabaseOnAppStorage(Context context, string databaseName, bool setAsCurrentDatabase = false)
        {
            // Beispiel: "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
			var externalFileDir = context.GetExternalFilesDir(null);
            if (externalFileDir == null)
                return null;

            string destination = Path.Combine(externalFileDir.AbsolutePath, databaseName + ".db3");
            if (File.Exists(destination))
            {
                return new Exception($"Datenbank '{databaseName}' ist bereits vorhanden.");
            }

            try
            {
                using (var br = new BinaryReader(Application.Context.Assets.Open(Android_Database.sqliteFilename_New)))
                {
                    using (var bw = new BinaryWriter(new FileStream(destination, FileMode.Create)))
                    {
                        byte[] buffer = new byte[2048];
                        int length = 0;
                        while ((length = br.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            bw.Write(buffer, 0, length);
                        }
                    }
                }

                if (setAsCurrentDatabase)
                {
                    DatabaseService.databasePath = destination;
                    DatabaseService.databaseType = DatabaseService.DatabaseType.Local;

                    Settings.PutString("LastSelectedDatabase",     DatabaseService.databasePath);
                    Settings.PutInt   ("LastSelectedDatabaseType", (int)DatabaseService.databaseType);
                }

			}
			catch (Exception e)
			{
				TRACE(e);

				return e;
			}

            return null;
        }

        /// <summary>
        /// Create test database with samples.
        /// </summary>
		public void RestoreDatabase_Test_Sample(Context context)
        {
            this.CreateLocalizedDatabaseFromAsset(context, Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Test, true, true);
        }

        /// <summary>
        /// Create empty test database.
        /// </summary>
        public void RestoreDatabase_Test_Db0(Context context)
        {
            this.CreateLocalizedDatabaseFromAsset(context, Android_Database.sqliteFilename_New, Android_Database.sqliteFilename_Test, false, true);
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

        /// <summary>
        /// Datenbanken aus den Resourcen erstellen.
        /// </summary>
		public void RestoreDatabasesFromResourcesOnStartup(Context context)
		{
            // Localized demo database with sample data.
            this.CreateLocalizedDatabaseFromAsset(context, Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Test, true,  false);

            // Datenbank im Applikationsverzeichnis erstellen.
            string databaseName = Path.GetFileNameWithoutExtension(Android_Database.sqliteFilename_Prod);
            this.CreateDatabaseOnAppStorage(context, databaseName, true);
        }

        public static SQLite.SQLiteConnection SQLiteConnection = null;

        internal static string GetTestDatabaseFileName(Context context)
        {
            // Beispiel: "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
			var externalFileDir = context.GetExternalFilesDir(null);

            return Path.Combine(externalFileDir.Path, Android_Database.sqliteFilename_Test);
        }

        internal static bool IsTestDatabaseActive(Context context)
        {
            string testDatabaseFileName = Android_Database.GetTestDatabaseFileName(context);
            
            return DatabaseService.databasePath == testDatabaseFileName;
        }

        public SQLite.SQLiteConnection GetConnection()
		{
            if (Android_Database.SQLiteConnection != null)
                return Android_Database.SQLiteConnection;

			string path = GetDatabasePath();
            if (path == null)
            {
                TRACE("Keine Datenbank ist ausgewählt.");
                throw new Exception("Keine Datenbank ist ausgewählt.");
            }

            FileInfo fileInfo = new FileInfo(path);
            if (fileInfo.IsReadOnly)
            {
                TRACE($"Database '{path}' is read only!");
                throw new Exception($"Database '{path}' is read only!");
            }

			// This is where we copy in the prepopulated database
			if (!File.Exists(path))
            {
                TRACE($"Database '{path}' not exists.");
                throw new Exception($"Database '{path}' not exists.");
            }

            TRACE("Database Path: {0}", path);

            TRACE("Database Size: {0} ({1:n0} Bytes)", Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

			var conn = new SQLite.SQLiteConnection(path, false);
            //conn.Trace = true;

			string cmd = "PRAGMA journal_mode=MEMORY";
			IList<JournalMode> tableInfo = conn.Query<JournalMode>(cmd);
            if (tableInfo.Count > 0)
            {
                //TRACE("PRAGMA journal_mode={0}", tableInfo[0].journal_mode);
            }

			this.UpdateDatabase(conn);

            Android_Database.SQLiteConnection = conn;

            Settings.PutString("LastSelectedDatabase", path);
        
			// Return the database connection 
			return conn;
		}

        public static Exception LoadDatabaseFileListSafe(Context context, out List<string> fileList)
        {
            Exception exception = null;
            fileList = new List<string>();

            try
            {
                string addPath = Settings.GetString("AdditionslDatabasePath", string.Empty);
            
                if (!string.IsNullOrEmpty(addPath))
                {
                    fileList.AddRange(Directory.GetFiles(addPath, "*.db3"));
                }
            }
            catch (Exception ex) { TRACE("AdditionslDatabasePath..."); exception = ex; }
            
            try
            {
                // "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht"
                // "/storage/0E0E-2316/Android/data/de.stryi.Vorratsuebersicht"
                var externalFilesDirs = context.GetExternalFilesDirs(null);
                if (externalFilesDirs != null)
                {
                    foreach(var extFilesDir in externalFilesDirs)
                    {
                        // Ist das Storage nicht gemounted?
                        if (extFilesDir == null)
                            continue;

                        if (!extFilesDir.CanWrite())
                        {
                            TRACE("GetDatabaseFileListSafe(): Can not write external storage dir '{0}'.", extFilesDir.AbsolutePath);
                            continue;
                        }

                        fileList.AddRange(Directory.GetFiles(extFilesDir.AbsolutePath, "*.db3"));
                    }
                }
            }
            catch (Exception ex) { TRACE("GetExternalFilesDirs..."); exception = ex; }

            try
            {
                var sorted = fileList.OrderBy(e => Path.GetFileNameWithoutExtension(e));
                fileList = sorted.ToList();
            }
            catch (Exception ex) { TRACE("OrderBy..."); exception = ex; }
            
            if (exception != null)
            {
                TRACE(exception);
            }

            return exception;
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
        private string CreateLocalizedDatabaseFromAsset(Context context, string fileName, string destinationFileName, bool prepareTestData, bool overrideIfExists)
        {
            // Beispiel: "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
			var applicationFileDir = context.GetExternalFilesDir(null);
            
            string dbPath = Path.Combine(applicationFileDir.AbsolutePath, destinationFileName);

            if (File.Exists(dbPath) && overrideIfExists)
                File.Delete(dbPath);

            if (File.Exists (dbPath))
                return null;

            string localizedFileName = this.GetLocalizedFileName(fileName);

            var liste = Application.Context.Assets.List(String.Empty);
            if (liste.Contains(localizedFileName))
            {
                fileName = localizedFileName;
            }

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

            return dbPath;
        }

        private string GetLocalizedFileName(string fileName)
        {
            // Gibt es eine länderspezifisches Assets (Datei)?
            string landKz = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

            string name      = Path.GetFileNameWithoutExtension(fileName);
            string extension = Path.GetExtension(fileName);

            return name + "_" + landKz + extension;
        }

        public Exception RenameDatabase(Context context, string databaseName, string newName)
        {
            string destinationName = Path.GetDirectoryName(databaseName);

            destinationName = Path.Combine(destinationName, newName + ".db3");

			try
			{
                File.Move(databaseName, destinationName);
			}
			catch (Exception e)
			{
				TRACE(e);

				return e;
			}

            return null;
        }

        internal Exception ImportDatabase(Context context, string sourceFilePath, string datebaseName)
        {
            try
            {

                // Beispiel: "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
			    var applicationFileDir = context.GetExternalFilesDir(null);
                if (applicationFileDir == null)
                    return null;

                string destinationFilePath = this.GetUniqueDestinationDatabaseName(applicationFileDir.Path, datebaseName);
                
                File.Copy(sourceFilePath, destinationFilePath);

                string msg = string.Format("Die Datenbank wurde als '{0}' importiert.", Path.GetFileNameWithoutExtension(destinationFilePath));
                var message = new AlertDialog.Builder(context);
                message.SetMessage(msg);
                message.SetPositiveButton(context.Resources.GetString(Resource.String.App_Ok), (s, e) => { });
                message.Create().Show();

            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        /// <summary>
        /// Dateiname liefern, was noch nicht da ist.
        /// </summary>
        /// <param name="path"></param>
        /// <param name="datebaseName"></param>
        /// <returns></returns>
        private string GetUniqueDestinationDatabaseName(string path, string datebaseName)
        {
            int counter = 0;
            string fileName = Path.GetFileNameWithoutExtension(datebaseName);
            string destinationFilePath;

            do
            {
                destinationFilePath = Path.Combine(path, fileName);

                // Erweiterung .db3 setzen.
                destinationFilePath = String.Format("{0}.{1}", destinationFilePath, "db3");

                counter++;
                fileName = string.Format("{0}_{1}", Path.GetFileNameWithoutExtension(datebaseName), counter);

            } while(File.Exists(destinationFilePath));

            return destinationFilePath;
        }
    }
}