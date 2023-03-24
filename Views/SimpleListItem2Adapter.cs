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
		private int listItemLayout = Android.Resource.Layout.SimpleListItem2;

		public SimpleListItem2Adapter (Activity activity, List<SimpleListItem2View> list, int? listItemLayout = null)
		{
			this.list = list;
			this.activity = activity;

			if (listItemLayout != null )
			{
				this.listItemLayout = listItemLayout.Value;
			}
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
				view = this.activity.LayoutInflater.Inflate (this.listItemLayout, null);
			}

			var kitten = this.list[position];

			TextView text1 = view.FindViewById<TextView> (Android.Resource.Id.Text1);
			text1.Text = kitten.Heading;

			TextView text2 = view.FindViewById<TextView> (Android.Resource.Id.Text2);
			if (text2 != null)
			{
				text2.Text = kitten.SubHeading;
				text2.SetTextSize(Android.Util.ComplexUnitType.Sp, 12);
				text2.Alpha = 0.5f;
			}

			return view;
		}
    }
}