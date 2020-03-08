using System;
using System.IO;
using System.Text;

using Android.App;
using Android.Content;
using Android.Support.V4.Content;

namespace VorratsUebersicht
{
    internal class CsvExport
    {
        public static void Share(Context context)
        {
            //generate data
            StringBuilder data = new StringBuilder();
            data.Append("Time;Distance");
            for(int i = 0; i<5; i++)
            {
                data.Append("\n"+i.ToString()+";"+(i*i).ToString());
            }
            try
            {
			    //string tempPath = System.IO.Path.GetTempPath();
                var tempPath = Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
                string destination = Path.Combine(tempPath, "Article.csv");

                //saving the file into device
                using(var writer = new System.IO.StreamWriter(destination, false))
                {
                    writer.Write(data.ToString());
                }
                
                //exporting
                Java.IO.File filelocation = new Java.IO.File(destination);
                var path = FileProvider.GetUriForFile(context, "de.stryi.exportcsv.fileprovider", filelocation);
                Intent fileIntent = new Intent(Intent.ActionSend);
                fileIntent.SetType("text/csv");
                fileIntent.PutExtra(Intent.ExtraSubject, "Articles");
                // fileIntent.AddFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                fileIntent.SetFlags(ActivityFlags.NewTask);
                fileIntent.SetFlags(ActivityFlags.GrantReadUriPermission);

                fileIntent.PutExtra(Intent.ExtraStream, path);
                context.StartActivity(Intent.CreateChooser(fileIntent, "CSV Datei senden"));
            }
            catch(Exception e){
                throw e;
            }
        }
    }
}