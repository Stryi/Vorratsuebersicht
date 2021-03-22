using System;
using System.IO;
using System.Collections.Generic;

using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;

namespace VorratsUebersicht
{
    public class DatabaseEventArg : EventArgs
    {
        public string DatabasePath;

    }

    // https://guides.codepath.com/android/using-dialogfragment

    public class DatabaseSelectFragment : Android.Support.V4.App.DialogFragment
    {
        public event EventHandler<DatabaseEventArg> OpenDatabase;
        public event EventHandler<DatabaseEventArg> CancelSelect;

        List<string> databaseFileInfoList;

		public static DatabaseSelectFragment newInstance()
		{
			DatabaseSelectFragment frag = new DatabaseSelectFragment();
			return frag;
		}

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            return inflater.Inflate(Resource.Layout.DatabaseSelectFragment, container, false);
        }

        public override void OnViewCreated(View view, Bundle savedInstanceState)
        {
            base.OnViewCreated(view, savedInstanceState);

            this.databaseFileInfoList = Android_Database.GetDatabaseFileList();

            var items = new List<SimpleListItem2View>();

            foreach(string databasefileName in this.databaseFileInfoList)
            {
                var item = new SimpleListItem2View();
                item.Heading = Path.GetFileNameWithoutExtension(databasefileName);
                //item.SubHeading = Tools.ToFuzzyByteString(databasefileName.Length);
                item.Tag = databasefileName;

                items.Add(item);
            }

            var folderListView = view.FindViewById<ListView>(Resource.Id.SelectFile);
            folderListView.ItemClick += ListView_ItemClick;

            var listAdapter = new SimpleListItem2Adapter(this.Activity, items);
            folderListView.Adapter = listAdapter;
        }

        private void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            ListView listView = sender as ListView;
            Java.Lang.Object itemObject = listView.GetItemAtPosition(e.Position);
            SimpleListItem2View item = Tools.Cast<SimpleListItem2View>(itemObject);

            var ea = new DatabaseEventArg();
            ea.DatabasePath = (string)item.Tag;

            this.OpenDatabase?.Invoke(this, ea);

            this.Dismiss();
        }

        public override void OnCancel(IDialogInterface dialog)
        {
            // Ersten Eintrag auswählen

            var ea = new DatabaseEventArg();
            if (databaseFileInfoList.Count > 0)
            {
                ea.DatabasePath = databaseFileInfoList[0];
            }

            this.CancelSelect?.Invoke(this, ea);
            
            base.OnCancel(dialog);
        }


    }
}
