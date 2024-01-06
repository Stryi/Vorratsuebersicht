using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Android.App;
using static Android.Media.Audiofx.DynamicsProcessing;

namespace VorratsUebersicht
{
    internal class Logging
    {
        internal static string GetCurrentLogFileName(DateTime? day = null)
        {
            if (day == null)
            {
                day = DateTime.Today;
            }

            // "/data/user/0/de.stryi.Vorratsuebersicht/files"
            string logFilePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);

            // 1. "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/cache"
            // 2. "/storage/1820-3B0F/Android/data/de.stryi.Vorratsuebersicht/cache"
            var cacheDirs = Application.Context.GetExternalCacheDirs();
            if (cacheDirs.Length > 1)
            {
                logFilePath = cacheDirs[0].AbsolutePath;
            }

            string fileName = string.Format("Vue_{0}.log", day.Value.ToString("yyyy.MM.dd"));
            return Path.Combine(logFilePath, fileName);
        }

        internal static void CreateTestLogFiles()
        {
            for (int index = -1; index >= -11; index--)
            {
                var day = DateTime.Today.AddDays(index);
                string fileName = Logging.GetCurrentLogFileName(day);

                if (!File.Exists(fileName))
                {
                    File.AppendAllText(fileName, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss - ") + "Protokolleintrag 1\n");
                    File.AppendAllText(fileName, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss - ") + "Protokolleintrag 2\n");
                    File.AppendAllText(fileName, DateTime.Now.ToString("yyyy-MM-dd HH.mm.ss - ") + "Protokolleintrag 3\n");
                }
            }
        }

        /// <summary>
        /// Nur die letzten 5 Log Dateien behalten (von den letzten 5 Tagen)
        /// </summary>
        internal static void ClearOldLogFiles()
        {
            //Logging.CreateTestLogFiles();

            var fileList = Logging.GetLogFileList();
            if (fileList.Length <= 5)
                return;

            for (int i = 0; i < fileList.Length - 5; i++)
            {
                File.Delete(fileList[i]);
            }
        }

        internal static string[] GetLogFileList()
        {
            // "/data/user/0/de.stryi.Vorratsuebersicht/files"
            string logFilePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            
            var unsortedFileList = Directory.GetFiles(logFilePath, "Vue_*.log");

            List<string> sortedList = new List<string>(unsortedFileList);

            // 1. "/storage/emulated/0/Android/data/de.stryi.Vorratsuebersicht/cache"
            // 2. "/storage/1820-3B0F/Android/data/de.stryi.Vorratsuebersicht/cache"
            var cacheDirs = Application.Context.GetExternalCacheDirs();
            if (cacheDirs.Length > 1)
            {
                logFilePath = cacheDirs[0].AbsolutePath;

                unsortedFileList = Directory.GetFiles(logFilePath, "Vue_*.log");
                sortedList.AddRange(unsortedFileList);
            }

            return sortedList.OrderBy(e => e).ToArray();
        }

        internal static string GetLogFileText()
        {
            StringBuilder text = new StringBuilder();

            var fileList = Logging.GetLogFileList();

            foreach(string logFileName in fileList)
            {
                text.AppendLine("LogFile: " + logFileName);
                text.AppendLine(File.ReadAllText(logFileName));
            }

            return text.ToString();
        }

    }
}