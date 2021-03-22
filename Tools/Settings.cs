using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    internal class Settings
    {
        internal static string GetString(string key, string defValue)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            return prefs.GetString(key, defValue);
        }

        internal static int GetInt(string key, int defValue)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            return prefs.GetInt(key, defValue);
        }
        
        internal static bool GetBoolean(string key, bool defValue)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            return prefs.GetBoolean(key, defValue);
        }

        internal static DateTime? GetDate(string key)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            string dateText = prefs.GetString(key, null);

            if (dateText == null)
                return null;

            var date = DateTime.ParseExact(dateText, "yyyy.MM.dd", CultureInfo.InvariantCulture);

            return date.Date;
        }
        
        internal static void PutString(string key, string value)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString(key, value);
            prefEditor.Commit();
        }

        internal static void PutInt(string key, int value)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutInt(key, value);
            prefEditor.Commit();
        }

        internal static void PutBoolean(string key, bool value)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutBoolean(key, value);
            prefEditor.Commit();
        }

        internal static void PutDate(string key, DateTime value)
        {
            string dateText = value.ToString("yyyy.MM.dd");

            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.PutString(key, dateText);
            prefEditor.Commit();
        }

        internal static void Clear(string key)
        {
            var prefs = Application.Context.GetSharedPreferences("Vorratsübersicht", FileCreationMode.Private);
            var prefEditor = prefs.Edit();
            prefEditor.Remove(key);
            prefEditor.Commit();
        }
    }
}