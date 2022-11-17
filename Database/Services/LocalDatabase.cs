using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using Android.Content;

namespace VorratsUebersicht
{
    using SQLite;
    using static Tools;

    internal class LocalDatabase : IDatabase
    {
        SQLiteConnection connection = null;

        internal static string databaseFileName = 
            Environment.ExpandEnvironmentVariables(
            @"/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/files/Vorraete.db3");

        public List<T> ExecuteQuery<T>(string query, params object[] parameters)
        {
            this.connection = this.GetConnection();

            var command = this.connection.CreateCommand(query, parameters);

            return command.ExecuteQuery<T>();
        }

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            this.connection = this.GetConnection();

            var command = this.connection.CreateCommand(query, parameters);

            int result = command.ExecuteNonQuery();

            return result;
        }

        public T ExecuteScalar<T>(string query, params object[] parameters)
        {
            this.connection = this.GetConnection();

            return this.connection.ExecuteScalar<T>(query, parameters);
        }

        public int ExecuteInsert(string query, params object[] parameters)
        {
            this.connection = this.GetConnection();

            var command = this.connection.CreateCommand(query, parameters);

            command.ExecuteNonQuery();

            return this.ExecuteScalar<int>("SELECT last_insert_rowid()");
        }

        public int Insert(object obj)
        {
            this.connection = this.GetConnection();

            return this.connection.Insert(obj);
        }

        public int Update(object obj)
        {
            this.connection = this.GetConnection();

            return this.connection.Update(obj);
        }

        public int Delete(object obj)
        {
            this.connection = this.GetConnection();

            return this.connection.Delete(obj);
        }

        internal static List<DatabaseService.Database> GetDatabases(Android.Content.Context context, ref Exception exception)
        {
            var fileList = new List<string>();

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
                    foreach (var extFilesDir in externalFilesDirs)
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

            var databaseList = new List<DatabaseService.Database>();

            foreach(var file in fileList)
            {
                var dbInfo = new DatabaseService.Database()
                {
                    Name = Path.GetFileNameWithoutExtension(file),
                    Path = file,
                    Type = DatabaseService.DatabaseType.Local,
                };

                databaseList.Add(dbInfo);
            }

            if (exception != null)
            {
                TRACE("LocalDatabase.GetDatabases(...):");
                TRACE(exception);
            }

            return databaseList;
        }

        internal static string TryOpenDatabase(DatabaseService.Database database)
        {
            try
            {
                FileInfo fileInfo = new FileInfo(database.Path);
                if (!fileInfo.Exists)
                {
                    TRACE($"Database '{database.Path}' does not exists.");
                    throw new Exception($"Database '{database.Path}' does not exists.");
                }

                if (fileInfo.IsReadOnly)
                {
                    TRACE($"Database '{database.Path}' is read only!");
                    throw new Exception($"Database '{database.Path}' is read only.");
                }

                TRACE("Database Path: {0}", database.Path);

                TRACE("Database Size: {0} ({1:n0} Bytes)", Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

			    var conn = new SQLite.SQLiteConnection(database.Path, SQLite.SQLiteOpenFlags.ReadOnly, false);

                // Irgend ein SELECT...
                string cmd = string.Format("SELECT name FROM sqlite_master WHERE type = 'Article' AND name = 'ArticleId'");
                IList<table_info> tableInfo = conn.Query<table_info>(cmd);

                conn.Close();
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

		public SQLiteConnection GetConnection()
		{
            if (this.connection != null)
                return this.connection;
            
            var path = LocalDatabase.databaseFileName;

            if (path == null)
            {
                TRACE("Keine Datenbank ist ausgewählt.");
                throw new Exception("Keine Datenbank ist ausgewählt.");
            }

            FileInfo fileInfo = new FileInfo(path);
            if (!fileInfo.Exists)
            {
                TRACE($"Database '{path}' does not exists.");
                throw new Exception($"Database '{path}' does not exists.");
            }

            if (fileInfo.IsReadOnly)
            {
                TRACE($"Database '{path}' is read only!");
                throw new Exception($"Database '{path}' is read only!");
            }

            TRACE("Database Path: {0}", path);

            TRACE("Database Size: {0} ({1:n0} Bytes)", Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

			var conn = new SQLiteConnection(path, false);
            //conn.Trace = true;

			string cmd = "PRAGMA journal_mode=MEMORY";
			IList<JournalMode> tableInfo = conn.Query<JournalMode>(cmd);
            if (tableInfo.Count > 0)
            {
                //TRACE("PRAGMA journal_mode={0}", tableInfo[0].journal_mode);
            }

			//this.UpdateDatabase(conn);

            this.connection = conn;

            //Settings.PutString("LastSelectedDatabase", path);
            //Settings.PutInt   ("LastSelectedDatabaseType", (int)DatabaseService.DatabaseType.Local);
        
			// Return the database connection 
			return conn;
		}

        public string GetDatabaseFileInfo(Context context, string databaseFileName)
        {
            string format = context.Resources.GetString(Resource.String.Settings_Datenbank);

            if (string.IsNullOrEmpty(databaseFileName))
                return string.Empty;

            FileInfo fileInfo = new FileInfo(databaseFileName);
            if (!fileInfo.Exists)
            {
                return $"Database '{databaseFileName}' does not exists.";
            }

            string info = string.Format(format, databaseFileName, Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

            if (fileInfo.IsReadOnly)
            {
                info += Environment.NewLine;
                info += Environment.NewLine;
                info += "File is read only!";
            }

            return info;
        }

        public static Exception DeleteDatabase(string databaseName)
        {
            try
            {
                File.Delete(databaseName);
            }
            catch (Exception ex)
            {
                return ex;
            }
            return null;
        }
    }
}
