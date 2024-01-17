using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using System.Globalization;

using Android.App;
using Android.Content;
using Xamarin.Essentials;

namespace VorratsUebersicht
{
    using SQLite;
    using static Tools;

    public class Android_Database
    {
        // http://err2solution.com/2016/05/sqlite-with-xamarin-forms-step-by-step-guide/

        public static bool UseTestDatabase = false;

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

		public string GetDatabasePath()
		{
            string databasePath;
            string databaseFileName;

            //
            // Test Datenbank in Optionen ausgew�hlt?
            //
			if (Android_Database.UseTestDatabase)
            {
                databasePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
                databaseFileName = Path.Combine(databasePath, Android_Database.sqliteFilename_Test);

                return databaseFileName;
            }

            //
            // Datenbank beim Starten der Anwendung ausgew�hlt?
            //
            if (!string.IsNullOrEmpty(Android_Database.SelectedDatabaseName))
            {
                databaseFileName = Android_Database.SelectedDatabaseName;

                return databaseFileName;
            }    

            //
            // Die App Datenbank ausw�hlen.
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

            // �ffne dann die zuletzt verwendete Datenbank.
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

        public string GetDatabaseInfoText(string format)
        {
            string databaseName = Android_Database.Instance.GetDatabasePath();
            if (databaseName == null)
                return string.Empty;

            try
            {
                FileInfo fileInfo = new FileInfo(databaseName);

                string info = string.Format(format, databaseName, Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

                return info;
            }
            catch(Exception ex)
            {
                return ex.Message;
            }
        }

        public void CompressDatabase()
        {
            SQLite.SQLiteConnection databaseConnection = this.GetConnection();
            if (databaseConnection == null)
                return;

            databaseConnection.Execute("UPDATE Article SET Manufacturer = RTRIM(Manufacturer) WHERE LENGTH(Manufacturer) <> LENGTH(TRIM(Manufacturer))");
            databaseConnection.Execute("UPDATE Article SET SubCategory  = RTRIM(SubCategory)  WHERE LENGTH(SubCategory)  <> LENGTH(TRIM(SubCategory))");
            databaseConnection.Execute("UPDATE Article SET StorageName  = RTRIM(StorageName)  WHERE LENGTH(StorageName)  <> LENGTH(TRIM(StorageName))");       
            databaseConnection.Execute("UPDATE Article SET Supermarket  = RTRIM(Supermarket)  WHERE LENGTH(Supermarket)  <> LENGTH(TRIM(Supermarket))");

            databaseConnection.Execute("UPDATE StorageItem SET StorageName = RTRIM(StorageName) WHERE LENGTH(StorageName) <> LENGTH(TRIM(StorageName))");

            databaseConnection.Execute("VACUUM");
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

        public bool IsCurrentDatabaseExists()
		{
			var path = Android_Database.Instance.GetDatabasePath();

			return File.Exists(path);
		}

		public Exception CreateDatabaseOnAppStorage(Context context, string databaseName)
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

            // Beispiel: "/data/user/0/de.stryi.Vorratsuebersicht/files"
			string documentsPath = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
			string source      = Path.Combine(documentsPath, Android_Database.sqliteFilename_New);

            try
            {
                File.Copy(source, destination);
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
		public void RestoreDatabase_Test_Sample(bool overrideDatabase)
        {
            this.CreateLocalizedDatabaseFromAsset(Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Test, true, true);
        }

        /// <summary>
        /// Create empty test database.
        /// </summary>
        public void RestoreDatabase_Test_Db0(bool overrideDatabase)
        {
            this.CreateLocalizedDatabaseFromAsset(Android_Database.sqliteFilename_New, Android_Database.sqliteFilename_Test, false, true);
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
            this.CreateLocalizedDatabaseFromAsset(Android_Database.sqliteFilename_Demo, Android_Database.sqliteFilename_Test, true,  false);

            // Empty database to create a new database.
            this.CreateLocalizedDatabaseFromAsset(Android_Database.sqliteFilename_New,  Android_Database.sqliteFilename_New,  false, false);

            List<string> databases;
            Android_Database.LoadDatabaseFileListSafe(context, out databases);
            if (databases.Count == 0)
            {
                // Datenbank im Applikationsverzeichnis erstellen.
                string databaseName = Path.GetFileNameWithoutExtension(Android_Database.sqliteFilename_Prod);
                this.CreateDatabaseOnAppStorage(context, databaseName);
            }
        }

        public static SQLite.SQLiteConnection SQLiteConnection = null;

		public SQLite.SQLiteConnection GetConnection()
		{
            if (Android_Database.SQLiteConnection != null)
                return Android_Database.SQLiteConnection;

			string path = GetDatabasePath();
            if (path == null)
            {
                TRACE("Keine Database ist ausgew�hlt.");
                throw new Exception("Keine Database ist ausgew�hlt.");
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

        public static List<string> GetStoragesPaths(Context context, string additionsPath = null)
        {
            List<String> pathList = new List<String>();

            var externalStoragePaths = context.GetExternalFilesDirs(Android.OS.Environment.DirectoryDownloads);
            foreach(Java.IO.File pathFile in externalStoragePaths)
            {
                string pathName = pathFile.AbsolutePath;
                if (!pathFile.CanRead())
                {
                    continue;
                }

                int index = pathName.IndexOf(Path.Combine("/Android/data", AppInfo.PackageName));
                if (index < 0)
                {
                    continue;
                }
                pathName = pathName.Substring(0, index);

                if (!string.IsNullOrEmpty(additionsPath))
                {
                    pathName = Path.Combine(pathName, additionsPath);
                }

                if (Directory.Exists(pathName))
                {
                    pathList.Add(pathName);
                    try
                    {   
                        Android_Database.GetDirectoryReqursive(pathList, pathName);
                    }
                    catch
                    { 
                        // Wenn nich keine Berechtigung vergeben ist...
                    }

                }
            }

            return pathList;
        }

        private static void GetDirectoryReqursive(List<string> pathList, string pathName)
        {
            // Und noch alle darunterliegenden Verzeichnisse
            foreach(string subDir in Directory.GetDirectories(pathName))
            {
                var dirInfo = new DirectoryInfo(subDir);
                if ((dirInfo.Attributes & FileAttributes.Hidden) == (FileAttributes.Hidden))
                {
                    continue;
                }

                pathList.Add(subDir);

                Android_Database.GetDirectoryReqursive(pathList, subDir);
            }
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

        public static string TryOpenDatabase(string databaseName)
        {
            try
            {
                Android_Database.Instance.CloseConnection();

                Android_Database.SelectedDatabaseName = databaseName;

                Android_Database.SQLiteConnection = Android_Database.Instance.GetConnection();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return null;
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

            // Update 2.34: Einstellungen (z.B. f�r zus�tzliche eigene Kategorien)
            if (!this.IsTableInDatabase(conn, "Settings"))
            {
                string cmd = string.Empty;

                cmd += "CREATE TABLE [Settings] (";
                cmd += " [SettingsId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT,";
                cmd += " [Key] TEXT, ";
                cmd += " [Value] TEXT);";

                conn.Execute(cmd);
            }

            // Update 4.00: Extra Tabelle f�r Bilder
            if (!this.IsTableInDatabase(conn, "ArticleImage"))
            {
                string cmd = 
                    "CREATE TABLE [ArticleImage] (" +
                    " [ImageId] INTEGER NOT NULL PRIMARY KEY AUTOINCREMENT," +
                    " [ArticleId] INTEGER NOT NULL," +
                    " [Type] INTEGER NOT NULL," +     // 0 - Artikelbild(-er), 1 - z.B. R�chsicht, 3 - Zutaten
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
        private string CreateLocalizedDatabaseFromAsset(string fileName, string destinationFileName, bool prepareTestData, bool overrideIfExists)
        {
            // Beispiel: "/data/user/0/de.stryi.Vorratsuebersicht/files"
			string path = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            

            string dbPath = Path.Combine(path, destinationFileName);

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
            // Gibt es eine l�nderspezifisches Assets (Datei)?
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

        internal Exception DeleteDatabase(string selectedDatabasePath)
        {
            try
            {
                File.Delete(selectedDatabasePath);
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }

        internal Exception ImportDatabase(Context context, string sourceFilePath, string datebaseName)
        {
            // Beispiel: "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files"
			var applicationFileDir = context.GetExternalFilesDir(null);
            if (applicationFileDir == null)
                return null;

            string destinationFilePath = Path.Combine(applicationFileDir.Path, datebaseName);
            destinationFilePath = String.Format("{0}.{1}", destinationFilePath, "db3");

            var fileExists = File.Exists(destinationFilePath); 
            if (fileExists)
            {
                return new Exception($"Die Datenbank '{datebaseName}' existiert bereits.");
            }
                
            try
            {
                File.Copy(sourceFilePath, destinationFilePath);
            }
            catch (Exception ex)
            {
                var text = string.Format("{0}\n\n{1}",
                    ex.Message, 
                    context.Resources.GetString(Resource.String.App_CheckPermissions));

                var message = new AlertDialog.Builder(context);
                message.SetMessage(text);
                message.SetPositiveButton(context.Resources.GetString(Resource.String.App_Ok), (s, evt) => { });
                message.SetNegativeButton("App Info", (s, evt) => { AppInfo.ShowSettingsUI();});
                message.Create().Show();

                return null;
            }
            return null;
        }
    }
}