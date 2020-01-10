using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public class StorageItemListViewAdapter : BaseAdapter<StorageItemListView>   // , IFilterable
    {
        public List<StorageItemListView> items;
        private Activity context;

        public StorageItemListViewAdapter(Activity context, List<StorageItemListView> items) : base()
        {
            this.context = context;
            this.items = items;
        }
        public override long GetItemId(int position)
        {
            return position;
        }
        public override StorageItemListView this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }

        /*
        // Funktioniert irgendwie nicht
        private PositionFilter filter;
        public Filter Filter
        {
            get
            {
                if (this.filter != null)
                    return this.Filter;

                this.filter = new PositionFilter(this);

                return this.filter;
            }
        }
        */
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            StorageItemListView item = items[position];
            View view = convertView;

            if (view == null) // no view to re-use, create new
                view = context.LayoutInflater.Inflate(Resource.Layout.StorageItemListView, null);

            ImageView image   = view.FindViewById<ImageView>(Resource.Id.Image);
            TextView  header  = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextHeader);
            TextView  details = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextDetails);
            TextView  info    = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextInfo);
            TextView  warning = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextWarning);
            TextView  error   = view.FindViewById<TextView>(Resource.Id.StorageItemListView_TextError);

            header.Text  = item.Heading;
            details.Text = item.SubHeading;

            if (!string.IsNullOrEmpty(item.InfoText))
            {
                info.Text    = item.InfoText;
                info.Visibility = ViewStates.Visible;
            }
            else
            {
                info.Visibility = ViewStates.Gone;
            }
			if (!string.IsNullOrEmpty(item.WarningText))
			{
	            warning.Text    = item.WarningText;
				warning.Visibility = ViewStates.Visible;
			}
			else
			{
				warning.Visibility = ViewStates.Gone;
			}
			if (!string.IsNullOrEmpty(item.ErrorText))
			{
	            error.Text    = item.ErrorText;
				error.Visibility = ViewStates.Visible;
			}
			else
			{
				error.Visibility = ViewStates.Gone;
			}

            if (item.Image == null)
                image.SetImageResource(Resource.Drawable.ic_photo_camera_black_24dp);
            else
                image.SetImageBitmap(item.Image);

            return view;
       }

        /*
        // https://forums.xamarin.com/discussion/46051/custom-filter-problem
        // Funktioniert aber irgendwie nicht
        private class PositionFilter : Filter
        {

            StorageItemListViewAdapter customAdapter;
            public PositionFilter(StorageItemListViewAdapter adapter) : base()
            {
                customAdapter = adapter;
            }

            protected override FilterResults PerformFiltering(ICharSequence constraint)
            {
                
                FilterResults filterResults = new FilterResults();
                if (constraint == null || constraint.Length() == 0)
                {
                    List<StorageItemListView> matchList = new List<StorageItemListView>();

                    foreach (StorageItemListView view in customAdapter.items)
                    {
                        if (view.Heading.ToUpper().Contains(constraint.ToString().ToUpper()))
                        {
                            matchList.Add(view);
                        }
                    }

                    Java.Lang.Object[] resultsValues;
                    resultsValues = new Java.Lang.Object[matchList.Count()];

                    for (int i = 0; i < matchList.Count(); i++)
                    {
                        resultsValues[i] = matchList[i];
                    }

                    filterResults.Count  = matchList.Count();
                    filterResults.Values = resultsValues;

                    return filterResults;
                }
                
                return filterResults;
            }

            protected override void PublishResults(ICharSequence constraint, FilterResults results)
            {
            }
        }
        */
    }
}