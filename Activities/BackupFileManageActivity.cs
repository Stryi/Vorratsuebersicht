using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.Content;
using Android.Views;
using Android.Widget;

using static VorratsUebersicht.Tools;

namespace VorratsUebersicht
{
    [Activity(Label = "Auswahl Datei")]
    public class BackupFileManageActivity : Activity
    {
        List<SimpleListItem2View> items;
        private string path;
        private string searchPattern;

        public static readonly int PickBackupId = 1000;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // ActionBar Hintergrund Farbe setzen
            var backgroundPaint = ContextCompat.GetDrawable(this, Resource.Color.Application_ActionBar_Background);
            backgroundPaint.SetBounds(0, 0, 10, 10);
            ActionBar.SetBackgroundDrawable(backgroundPaint);
            ActionBar.SetDisplayHomeAsUpEnabled(true);
            ActionBar.SetIcon(Resource.Drawable.baseline_folder_white_24);

            this.Title         = Intent.GetStringExtra("Text");
            this.path          = Intent.GetStringExtra("Path");
            this.searchPattern = Intent.GetStringExtra("SearchPattern");

            // Soll noch umgestellt werden
            SetContentView(Resource.Layout.BackupFileManage);

            var pathView = FindViewById<TextView>(Resource.Id.SelectFile_Path);
            pathView.Text = this.path;

            var listView = FindViewById<ListView>(Resource.Id.SelectFile);
            listView.ItemClick += ListView_ItemClick;

            bool isGranted = new SdCardAccess().Grand(this);

            if (isGranted)
            {
                this.ShowFileList();
            }
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            SimpleListItem2View item = Tools.Cast<SimpleListItem2View>(itemObject);
            
            string[] actions = {
                this.Resources.GetString(Resource.String.Settings_Action_BackupRestore),
                this.Resources.GetString(Resource.String.Settings_Action_BackupDelete)
            };

            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle(item.Heading);
            builder.SetItems(actions, (sender2, args) =>
            {

                switch (args.Which)
                {
                    case 0: // Zurückspielen
                        Intent intent = new Intent();
			            intent.PutExtra("FullName",   item.Tag.ToString());

                        this.SetResult(Result.Ok, intent);

                        this.OnBackPressed();

                        break;

                    case 1: // Löschen
                        this.DeleteBackup(item.Tag.ToString());
                        break;
                }

                return;
            });
            builder.Show();

        }

        private void DeleteBackup(string filePath)
        {
            string message = string.Format(this.Resources.GetString(Resource.String.Settings_DeleteBackupFileQuestion),
                Path.GetFileName(filePath));
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetNegativeButton(this.Resources.GetString(Resource.String.App_DoNotDelete), (s, e) => { });
            builder.SetPositiveButton(this.Resources.GetString(Resource.String.App_Delete), (s, e) => 
            { 
                File.Delete(filePath);
                this.ShowFileList();
            });
            builder.Create().Show();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            this.ShowFileList();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);

            if (resultCode != Result.Ok)
            {
                return;
            }
            if ((requestCode == PickBackupId) && (data != null))
            {
            }

        }

        private void ShowFileList()
        {
            try
            {
                var fileListUnsorted = new DirectoryInfo(this.path).GetFiles(this.searchPattern);

                var fileList = fileListUnsorted.OrderBy( e => e.Name);

                this.items = new List<SimpleListItem2View>();
                foreach(FileInfo file in fileList)
                {
                    var item = new SimpleListItem2View();
                    item.Heading = Path.GetFileNameWithoutExtension(file.FullName);
                    string size      = Tools.ToFuzzyByteString(file.Length);
                    string timeStamp = file.CreationTime.ToString(CultureInfo.CurrentUICulture);
                    item.SubHeading = string.Format("{0} - {1}", size, timeStamp);
                    item.Tag = file.FullName;

                    items.Add(item);
                }

                var listView = FindViewById<ListView>(Resource.Id.SelectFile);
                var listAdapter = new SimpleListItem2Adapter(this, items);
                listView.Adapter = listAdapter;
            }
            catch(Exception e)
            {
                TRACE(e);

                var pathView = FindViewById<TextView>(Resource.Id.SelectFile_Message);
                pathView.Visibility = ViewStates.Visible;
                pathView.Text = e.Message;
            }
         }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Android.Resource.Id.Home:
                    this.OnBackPressed();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}