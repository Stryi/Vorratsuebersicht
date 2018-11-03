using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public class SimpleListItem2Adapter : BaseAdapter<SimpleListItem2View>
    {
	    private readonly List<SimpleListItem2View> list;
		private readonly Activity activity;

		public SimpleListItem2Adapter (Activity activity, List<SimpleListItem2View> list)
		{
			this.list = list;
			this.activity = activity;
		}

		public override long GetItemId (int position)
		{
			return position;
		}

		public override SimpleListItem2View this [int index] {
			get { return this.list [index]; }
		}

		public override int Count {
			get { return this.list.Count; }
		}

		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			var view = convertView;

			if (view == null) {
				view = this.activity.LayoutInflater.Inflate (Android.Resource.Layout.SimpleListItem2, null);
			}

			var kitten = this.list[position];

			TextView text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			text1.Text = kitten.Heading;

			TextView text2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);
			text2.Text = kitten.SubHeading;

			return view;
		}
    }
}