using System;
using System.Globalization;
using System.IO;
using Android.Util;

namespace VorratsUebersicht
{
    public static class Tools
    {
        private static string logFileName;

        public static T Cast<T>(this Java.Lang.Object obj) where T : class
        {
            var propertyInfo = obj.GetType().GetProperty("Instance");
            return propertyInfo == null ? null : propertyInfo.GetValue(obj, null) as T;
        }

        // https://dotnet-snippets.de/snippet/byte-groessenangaben-als-string-formatieren-kb-mb-gb/1304
        public static string ToFuzzyByteString(long bytes)
        {                    
            double s = bytes; 
            string[] format = new string[]
                  {
                      "{0} bytes", "{0} KB",  
                      "{0} MB", "{0} GB", "{0} TB", "{0} PB", "{0} EB"
                  };

            int i = 0;

            while (i < format.Length && s >= 1024)              
            {                                     
                s = (long) (100*s/1024)/100.0;  
                i++;            
            }                     
            return string.Format(format[i], s);  
        }

        public static void TRACE(string text)
        {
            if (text == null)
                return;

            Log.WriteLine(LogPriority.Debug, "stryi", text);
            Tools.LogToFile(text);
        }

        public static void TRACE(string format, params object[] args)
        {
            TRACE(string.Format(format, args));
        }

        public static void TRACE(Exception e)
        {
            Tools.LogToFile(e.ToString());
            Log.WriteLine(LogPriority.Error, "stryi", e.ToString());
        }

        private static void LogToFile(string text)
        {
            try
            {
                if (string.IsNullOrEmpty(Tools.logFileName))
                {
                    Logging.ClearOldLogFiles();

                    Tools.logFileName = Logging.GetCurrentLogFileName();
                }

                string[] lines = text.Split("\n");
                foreach(string line in lines)
                {
                    if (string.IsNullOrEmpty(line))
                        continue;

                    var lineText = DateTime.Now.ToString("HH.mm.ss - ") + line + "\r\n";
                    File.AppendAllText(Tools.logFileName, lineText);
                }

            }
            catch(Exception e)
            {
                Log.WriteLine(LogPriority.Error, "stryi", e.ToString());
            }
        }

        internal static string GetLogFileName()
        {
            return Tools.logFileName;
        }

        public static string GetBackupDatabaseName(string backupFilePath)
        {
            string databaseName = Path.GetFileNameWithoutExtension(backupFilePath);

            if (databaseName.Length < 19)
            {
                return databaseName;
            }

            string dateTimeText = databaseName.Substring(databaseName.Length - 19);

            bool isDateTime = DateTime.TryParseExact(
                dateTimeText,
                "yyyy-MM-dd HH.mm.ss", 
                CultureInfo.InvariantCulture, 
                DateTimeStyles.None,
                out DateTime result);

            if (!isDateTime)
            {
                return databaseName;
            }

            // Das Datum im Namen entfernen
            databaseName = databaseName.Substring(0, databaseName.Length - 19);
            databaseName = databaseName.TrimEnd(new char[] { '_' });

            if (databaseName == "Vue")
            {
                databaseName = "Vorraete";
            }

            return databaseName;
        }
    }
}