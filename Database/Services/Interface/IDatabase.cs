using System;
using System.Collections.Generic;

namespace VorratsUebersicht
{
    internal interface IDatabase
    {
        // Datanbank Abfragen
        List<T> ExecuteQuery<T>(string cmdText, params object[] ps);
        int ExecuteNonQuery(string query, params object[] parametes);
        T ExecuteScalar<T>(string query, params object[] parametes);
        int ExecuteInsert(string query, params object[] parametes);
        int Insert(object obj);
        int Update(object obj);
        int Delete(object obj);

        // Verwaltung
        string GetCurrentDatabaseName();

        string GetDatabaseFileInfo(Android.Content.Context context, string databaseName);
    }
}
