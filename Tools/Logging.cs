using System;
using System.IO;
using System.Text;

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

            string logFilePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
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
            string logFilePath = System.Environment.GetFolderPath (System.Environment.SpecialFolder.Personal);
            return Directory.GetFiles(logFilePath, "Vue_*.log");
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