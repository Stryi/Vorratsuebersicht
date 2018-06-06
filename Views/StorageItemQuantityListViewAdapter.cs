using System;
using System.Collections.Generic;

using Android.App;
using Android.Views;
using Android.Widget;
using Android.Graphics;


namespace VorratsUebersicht
{
    public class StorageItemQuantityListViewAdapter : BaseAdapter<StorageItemQuantityListView>
    {
        List<StorageItemQuantityListView> items;
        Activity context;
        bool actionButtonsVisible = false;

        public StorageItemQuantityListViewAdapter(Activity context, List<StorageItemQuantityListView> items) : base()
        {
            this.context = context;
            this.items = items;
        }

        public void Add(StorageItemQuantityListView item)
        {
            this.items.Add(item);
        }

        public override long GetItemId(int position)
        {
            return position;
        }
        public override StorageItemQuantityListView this[int position]
        {
            get { return items[position]; }
        }
        public override int Count
        {
            get { return items.Count; }
        }
        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var item = items[position];
            View view = convertView;
            if (view == null) // no view to re-use, create new
            {
                view = context.LayoutInflater.Inflate(Resource.Layout.StorageItemQuantityListView, null);
                ImageButton buttonRemove = view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Remove);
                buttonRemove.Click += delegate 
                {
                    if (item.StorageItem.Quantity == 0)
                        return;

                    item.StorageItem.Quantity --;
                    item.StorageItem.QuantityDiff--;
                    System.Diagnostics.Trace.WriteLine(item.Heading);
                    this.NotifyDataSetChanged();
                };

                ImageButton buttonAdd = view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Add);
                buttonAdd.Click += delegate 
                {
                    item.StorageItem.Quantity++;
                    item.StorageItem.QuantityDiff++;
                    System.Diagnostics.Trace.WriteLine(item.Heading);
                    this.NotifyDataSetChanged();
                };                
            }
            view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Text).Text = item.Heading;
            view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Details).Text = item.SubHeading;

            // TODO: Die Farbe aus der Resource oder auch inzwischen aus de Konfiguration auslesen.
            if (item.WarningLevel > 0)
                view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Details).SetTextColor(item.WarningColor);
            else
                view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Details).SetTextColor(Color.Black);

            if (this.actionButtonsVisible)
            {
                view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Remove).Visibility = ViewStates.Visible;
                view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Add)   .Visibility = ViewStates.Visible;
            }
            else
            {
                view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Remove).Visibility = ViewStates.Invisible;
                view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Add)   .Visibility = ViewStates.Invisible;
            }
            return view;
        }

        public void ActivateButtons()
        {
            this.actionButtonsVisible = true;
        }
        public void DeactivateButtons()
        {
            this.actionButtonsVisible = false;
        }
    }
}