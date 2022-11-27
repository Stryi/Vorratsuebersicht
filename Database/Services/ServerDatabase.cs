using System;
using System.IO;
using System.Net;
using System.Text;
using System.Collections.Generic;

using Android.Content;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace VorratsUebersicht
{
    using SQLite;
    using static Tools;
    using static VorratsUebersicht.DatabaseService;

    internal class ServerDatabase : IDatabase
    {
        public bool GetLastInsertRowId {get;set;}
        public int LastInsertRowId {get;set;}

        internal static string serverAddresses;  // Liste der Adressen mit "Neue Zeile" getrennt.
        internal DatabaseService.Database database;

        public static void Initialize(string serverAddresses, string databaseName)
        {
            ServerDatabase.serverAddresses = serverAddresses;
        }
        
        public List<T> ExecuteQuery<T>(string query, params object[] parameters)
        {
            var sqlRequest = new RestAPIRequest()
            {
                Database = this.database.DatabaseName,
                SqlCommand = query,
                Parameters = parameters
            };

            string json = JsonConvert.SerializeObject(sqlRequest);
            
            var response = this.ExecuteRequest("/ExecuteQuery", "POST", json);

            JsonSerializerSettings settings = new JsonSerializerSettings();
            settings.Converters.Add(new ByteArrayJSonConverter());

            var responseObject = JsonConvert.DeserializeObject<List<T>>(response, settings);

            return responseObject;
        }

        internal class ByteArrayJSonConverter : JsonConverter<Byte[]>
        {
            public override byte[] ReadJson(JsonReader reader, Type objectType, byte[] existingValue, bool hasExistingValue, JsonSerializer serializer)
            {
                var jObject = JObject.Load(reader);
                JToken data = jObject.SelectToken("data");
                var result = data.ToObject<byte[]>();
                
                return result;
            }

            public override void WriteJson(JsonWriter writer, byte[] value, JsonSerializer serializer)
            {
            }
        }

        public int ExecuteNonQuery(string query, params object[] parameters)
        {
            var sqlRequest = new RestAPIRequest()
            {
                Database = this.database.DatabaseName,
                SqlCommand = query,
                Parameters = parameters
            };

            string json = JsonConvert.SerializeObject(sqlRequest);
            
            var changes = this.ExecuteRequest("/ExecuteNonQuery", "POST", json);
        
            return Int32.Parse(changes);
        }

        public T ExecuteScalar<T>(string query, params object[] parameters)
        {
            var sqlRequest = new RestAPIRequest()
            {
                Database = this.database.DatabaseName,
                SqlCommand = query,
                Parameters = parameters
            };

            string json = JsonConvert.SerializeObject(sqlRequest);
            
            var response = this.ExecuteRequest("/ExecuteScalar", "POST", json);

            var responseObject = JsonConvert.DeserializeObject<T>(response);

            return responseObject;
        }

        public int ExecuteInsert(string query, params object[] parameters)
        {
            var sqlRequest = new RestAPIRequest()
            {
                Database = this.database.DatabaseName,
                SqlCommand = query,
                Parameters = parameters
            };

            string json = JsonConvert.SerializeObject(sqlRequest);
            
            string newId = this.ExecuteRequest("/ExecuteInsert", "POST", json);

            return Int32.Parse(newId);
        }

        public int Insert(object obj)
        {
            var conn = new SQLiteConnection(String.Empty, SQLiteOpenFlags.ReadOnly);

            var map = conn.GetMapping(obj.GetType());

            string columnList = string.Empty;
            string valueList  = string.Empty;
            var par = new List<object>();

            foreach(var col in map.InsertColumns)
            {
                if (!string.IsNullOrEmpty(columnList)) columnList += ", ";
                if (!string.IsNullOrEmpty(valueList))  valueList  += ", ";
                
                columnList += "\"" + col.Name + "\"";
                valueList  += "?";

                par.Add(col.GetValue(obj));
            }

			var cmd = string.Format ("INSERT INTO \"{0}\" ({1}) VALUES ({2})",
                map.TableName,
				columnList,
				valueList);

            return this.ExecuteInsert(cmd, par.ToArray());
        }

        public int Update(object obj)
        {
            var conn = new SQLiteConnection(String.Empty, SQLiteOpenFlags.ReadOnly);

            var map = conn.GetMapping(obj.GetType());

            string valueList  = string.Empty;
            var par = new List<object>();

            foreach(var col in map.InsertOrReplaceColumns)
            {
                if (col.IsPK)
                    continue;

                if (!string.IsNullOrEmpty(valueList))  valueList  += ", ";
                
                valueList += $"\"{col.Name}\" = ?";

                par.Add(col.GetValue(obj));
            }

            var whereExpr = $"{map.PK.Name} = ?";
            par.Add(map.PK.GetValue(obj));

			var cmd = $"UPDATE \"{map.TableName}\" SET {valueList} WHERE {whereExpr}";

            return this.ExecuteNonQuery(cmd, par.ToArray());
        }

        public int Delete(object obj)
        {
            var conn = new SQLiteConnection(String.Empty, SQLiteOpenFlags.ReadOnly);

            var map = conn.GetMapping(obj.GetType());

            var par = new List<object>();

            var whereExpr = $"{map.PK.Name} = ?";
            par.Add(map.PK.GetValue(obj));

			var cmd = $"DELETE FROM {map.TableName} WHERE {whereExpr}";

            return this.ExecuteNonQuery(cmd, par.ToArray());
        }

        public static List<DatabaseService.Database> GetDatabases(ref Exception exception)
        {
            var databaseList = new List<DatabaseService.Database>();

            if (string.IsNullOrEmpty(ServerDatabase.serverAddresses))
                return databaseList;

            var serverDatabase = new ServerDatabase();

            foreach(string serverInfo in serverAddresses.Split(Environment.NewLine))
            {
                if (String.IsNullOrEmpty(serverInfo))
                    continue;

                var server = serverInfo.Split(";");
                var serverAddress = server[0];
                var serverName    = "Server";
                if (server.Length > 1)
                {
                    serverName    = server[0];
                    serverAddress = server[1];
                }

                try
                {
                    serverDatabase.database = new DatabaseService.Database();
                    serverDatabase.database.Location = serverAddress;

                    var response = serverDatabase.ExecuteRequest("/GetDatabases", "GET");

                    var serverDatabases = JsonConvert.DeserializeObject<List<string>>(response);
                    foreach(var database in serverDatabases)
                    {
                        var dbInfo = new DatabaseService.Database()
                        {
                            Name = Path.GetFileNameWithoutExtension(database) + " - " + serverName,
                            Location = serverAddress,
                            DatabaseName = database,
                            Type = DatabaseService.DatabaseType.Server
                        };

                        databaseList.Add(dbInfo);
                    }
                }
                catch(Exception ex)
                {
                    TRACE("ServerDatabases.GetDatabases(...):");
                    TRACE(ex);
                    exception = ex;
                }
            }

            return databaseList;
        }

        internal static string TryOpenDatabase(DatabaseService.Database database)
        {
            try
            {
                // Irgend ein SELECT...
                var sqlRequest = new RestAPIRequest()
                {
                    Database   = database.Location,
                    SqlCommand = "SELECT * FROM Article WHERE ArticleId = 999" // PRAGMA integrity_check
                };
                
                string json = JsonConvert.SerializeObject(sqlRequest);
                
                // TODO: Bessere Abfrage....
                //var response = new ServerDatabase().ExecuteRequest("/ExecuteQuery", "POST", json);
            }
            catch(Exception ex)
            {
                return ex.Message;
            }

            return null;
        }

        public string GetCurrentDatabaseName()
        {
            return this.database.Name;
        }


        public string GetDatabaseFileInfo(Context context, string databaseFileName)
        {
            var request = new
            {
                Database   = databaseFileName
            };
                
            string json = JsonConvert.SerializeObject(request);
                
            var response = this.ExecuteRequest("/GetDatabaseFileInfo", "POST", json);

            var fileInfo = JsonConvert.DeserializeObject<DatabaseFileInfo>(response);

            string info = "Server: " + this.database.Location + Environment.NewLine + Environment.NewLine;

            string format = context.Resources.GetString(Resource.String.Settings_Datenbank);
            info += string.Format(format, this.database.DatabaseName, Tools.ToFuzzyByteString(fileInfo.Length), fileInfo.Length);

            return info;
        }

        public static Exception DeleteDatabase(string databaseName)
        {
            throw new NotImplementedException();
        }

        internal string ExecuteRequest(string requestUrl, string method, string body = null)
        {
            WebRequest request= WebRequest.Create(this.database.Location + requestUrl);
            request.Timeout = 10000;
            request.Method = method;
            request.ContentType = "application/json";

            if (!string.IsNullOrEmpty(body))
            {
                // turn our request string into a byte stream
                byte[] postBytes = Encoding.UTF8.GetBytes(body);

                request.ContentLength = postBytes.Length;
                Stream requestStream = request.GetRequestStream();

                requestStream.Write(postBytes, 0, postBytes.Length);
                requestStream.Close();
            }

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            //System.Threading.Thread.Sleep(1000);
            string webResponse = string.Empty;

            using (Stream dataStream = response.GetResponseStream())
            {
                StreamReader reader = new StreamReader(dataStream);
                webResponse = reader.ReadToEnd();
            }
            
            response.Close();

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                var error = JsonConvert.DeserializeObject<RestAPIErrorResponse>(webResponse);

                throw new Exception(error.Message);

            }

            return webResponse;
        }
    }

    public class RestAPIRequest
    {
        public string Database {get;set;}
        public string SqlCommand {get;set;}
        public object[] Parameters {get;set;}
    }

    public class RestAPIErrorResponse
    {
        public string Status {get;set;}
        public string Message {get;set;}
    }

}
