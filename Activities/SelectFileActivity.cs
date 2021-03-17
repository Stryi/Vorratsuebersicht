using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

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
    public class SelectFileActivity : Activity
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
            SetContentView(Resource.Layout.SelectFile);

            var pathView = FindViewById<TextView>(Resource.Id.SelectFile_Path);
            pathView.Text = this.path;

            var selectFileButton = FindViewById<Button>(Resource.Id.SelectFile_Button);
            selectFileButton.Click += SelectFileButton_Click;

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

            string[] actions = { "Zurückspielen", "Löschen" };

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
            string message = string.Format("Backup Datei löschen?\n\n{0}",
                Path.GetFileName(filePath));
            var builder = new AlertDialog.Builder(this);
            builder.SetMessage(message);
            builder.SetNegativeButton("Nicht löschen", (s, e) => { });
            builder.SetPositiveButton("Löschen", (s, e) => 
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
                var fileList = new DirectoryInfo(this.path).GetFiles(this.searchPattern);

                this.items = new List<SimpleListItem2View>();
                foreach(FileInfo file in fileList)
                {
                    var item = new SimpleListItem2View();
                    item.Heading = Path.GetFileNameWithoutExtension(file.FullName);
                    item.SubHeading = Tools.ToFuzzyByteString(file.Length);
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

        private void SelectFileButton_Click(object sender, EventArgs e)
        {
            Intent = new Intent();
            Intent.SetType("*/*.VueBak"); // *.VueBak
            Intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(Intent, "Select Backup"), PickBackupId);
        }

    }
}