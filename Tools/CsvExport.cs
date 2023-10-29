using System;
using System.Globalization;
using System.IO;
using System.Text;

using Android.App;
using Android.Content;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{

    //
    // Anhand vom https://github.com/techtribeyt/androidcsv/tree/master
    //
    internal class CsvExport
    {
        private SQLite.SQLiteConnection databaseConnection;
        private Activity context;
        private string trennzeichen = ",";

        public static string ExportArticles(Activity context, int requestCode)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            CsvExport export = new CsvExport();
            export.context = context;
            export.databaseConnection = databaseConnection;

            var result = export.GetArticlesAsCsvString();
            return export.WriteToFile("Vue-Artikel.csv", result, requestCode);
        }

        public static string ExportStorageItems(Activity context, int requestCode)
        {
            SQLite.SQLiteConnection databaseConnection = Android_Database.Instance.GetConnection();
            if (databaseConnection == null)
                return null;

            CsvExport export = new CsvExport();
            export.context = context;
            export.databaseConnection = databaseConnection;

            var result = export.GetStorageItemsAsCsvString();
            return export.WriteToFile("Vue-Lagerbestand.csv", result, requestCode);
        }

        private string WriteToFile(string fileName, StringBuilder result, int requestCode)
        {
            string destination = Environment.GetFolderPath (Environment.SpecialFolder.Personal);
            destination = Path.Combine(destination, fileName);

            //saving the file into device
            using(var writer = new StreamWriter(destination, false))
            {
                writer.Write(result.ToString());
            }
                
            Java.IO.File filelocation = new Java.IO.File(destination);
            var path = FileProvider.GetUriForFile(context, "de.stryi.exportcsv.fileprovider", filelocation);
            Intent fileIntent = new Intent(Intent.ActionSend);
            fileIntent.SetType("text/csv");
            fileIntent.PutExtra(Intent.ExtraSubject, fileName);
            fileIntent.SetFlags(ActivityFlags.NewTask);
            fileIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

            fileIntent.PutExtra(Intent.ExtraStream, path);
            context.StartActivityForResult(Intent.CreateChooser(fileIntent, "CSV Datei senden"), requestCode);

            return destination;
        }

        private StringBuilder GetArticlesAsCsvString()
        {
            StringBuilder header = new StringBuilder();
            header.Append("ArticleId|EANCode|Name|Manufacturer|Category|SubCategory|DurableInfinity|WarnInDays|Size|Unit|Notes|MinQuantity|PrefQuantity|StorageName|Supermarket|Calorie|Price");
            header.AppendLine();
            header.Replace("|", this.trennzeichen);

            StringBuilder data = new StringBuilder();
            var articleList = databaseConnection.Query<Article>("SELECT * FROM Article ORDER BY ArticleId");
            foreach(Article article in articleList)
            {
                StringBuilder row = new StringBuilder();
                this.AddField(row, article.ArticleId);
                this.AddField(row, article.EANCode);
                this.AddField(row, article.Name);
                this.AddField(row, article.Manufacturer);
                this.AddField(row, article.Category);
                this.AddField(row, article.SubCategory);
                this.AddField(row, article.DurableInfinity);
                this.AddField(row, article.WarnInDays);
                this.AddField(row, article.Size);
                this.AddField(row, article.Unit);
                this.AddField(row, article.Notes);
                this.AddField(row, article.MinQuantity);
                this.AddField(row, article.PrefQuantity);
                this.AddField(row, article.StorageName);
                this.AddField(row, article.Supermarket);
                this.AddField(row, article.Calorie);
                this.AddField(row, article.Price);

                data.Append(row);
                data.AppendLine();
            }

            header.Append(data);

            return header;
        }

        private StringBuilder GetStorageItemsAsCsvString()
        {
            StringBuilder header = new StringBuilder();
            header.Append("StorageItemId|ArticleId|Name|Manufacturer|ArticleStorageName|DurableInfinity|WarnInDays|Quantity|BestBefore|StorageName");
            header.AppendLine();
            header.Replace("|", this.trennzeichen);

            StringBuilder data = new StringBuilder();
            var articleList = databaseConnection.Query<StorageItemQuantityResult>(
                "SELECT StorageItemId, StorageItem.ArticleId, Name, Manufacturer, Article.StorageName AS ArticleStorageName, DurableInfinity, WarnInDays, Quantity, BestBefore, StorageItem.StorageName" +
                " FROM StorageItem" +
                " JOIN Article ON StorageItem.ArticleId = Article.ArticleId" +
                " ORDER BY StorageItem.ArticleId, BestBefore DESC");

            foreach(StorageItemQuantityResult item in articleList)
            {
                StringBuilder row = new StringBuilder();
                this.AddField(row, item.StorageItemId);
                this.AddField(row, item.ArticleId);
                this.AddField(row, item.Name);
                this.AddField(row, item.Manufacturer);
                this.AddField(row, item.ArticleStorageName);
                this.AddField(row, item.DurableInfinity);
                this.AddField(row, item.WarnInDays);
                this.AddField(row, item.Quantity);
                this.AddField(row, item.BestBefore);
                this.AddField(row, item.StorageName);

                data.Append(row);
                data.AppendLine();
            }

            header.Append(data);

            return header;
        }

        #region private

        private void AddField(StringBuilder data, bool value)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            data.Append(value);
        }

        private void AddField(StringBuilder data, int value)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            data.Append(value);
        }
        private void AddField(StringBuilder data, int? value)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            if (value == null)
                return;

            data.Append(value.Value);
        }

        private void AddField(StringBuilder data, decimal? value)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            if (value == null)
                return;

            var text = value.Value.ToString(CultureInfo.CurrentCulture);
            if (text.Contains(this.trennzeichen))
            {
                text = string.Format("\"{0}\"", text);
            }
            data.Append(text);
        }

        private void AddField(StringBuilder data, DateTime? value)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            if (value == null)
                return;

            data.Append(value.Value.ToShortDateString());
        }

        private void AddField(StringBuilder data, string text)
        {
            if (data.Length > 0)
                data.Append(this.trennzeichen);

            if (text == null)
                return;


            if (text.Contains(','))
            {
                if (text.Contains("\""))
                    text = text.Replace("\"", "\\\"");

                data.Append("\"" + text + "\"");
            }
            else
            {
                data.Append(text);
            }
        }

        #endregion
    }
}