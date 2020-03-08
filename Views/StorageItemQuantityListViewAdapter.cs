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
        public static decimal StepValue = 1;

        public event StorageItemQuantityListViewEventHandler DateClicked;

        public delegate void StorageItemQuantityListViewEventHandler(object sender, StorageItemEventArgs e);

        public class StorageItemEventArgs : EventArgs
        {
            public StorageItemQuantityResult StorageItem;
        }

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
            }

            ImageButton buttonRemove = view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Remove);
            buttonRemove.Tag = position;
            buttonRemove.Click -= DecreaseQuantity;
            buttonRemove.Click += DecreaseQuantity;

            ImageButton buttonAdd = view.FindViewById<ImageButton>(Resource.Id.StorageItemQuantityList_Add);
            buttonAdd.Tag = position;
            buttonAdd.Click -= IncreaseQuantity;
            buttonAdd.Click += IncreaseQuantity;

            TextView anzahl = view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Quantity);
            anzahl.Tag = position;
            anzahl.Click -= OnDatumChange;
            anzahl.Click += OnDatumChange;

            TextView datum = view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Date);
            datum.Tag = position;
            datum.Click -= OnDatumChange;
            datum.Click += OnDatumChange;

            TextView lager = view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Storage);
            lager.Tag = position;

            anzahl.Text = item.AnzahlText;
            datum.Text  = item.BestBeforeText;
            lager.Text  = item.LagerText;

            if (string.IsNullOrEmpty(lager.Text))
                lager.Visibility = ViewStates.Gone;
            else
                lager.Visibility = ViewStates.Visible;
            
            // TODO: Die Farbe aus der Resource oder auch inzwischen aus de Konfiguration auslesen.
            if (item.WarningLevel > 0)
                datum.SetTextColor(item.WarningColor);
            else
                datum.SetTextColor(Color.Black);

            if (this.actionButtonsVisible)
            {
                buttonRemove.Visibility = ViewStates.Visible;
                buttonAdd   .Visibility = ViewStates.Visible;
            }
            else
            {
                buttonRemove.Visibility = ViewStates.Gone;
                buttonAdd   .Visibility = ViewStates.Gone;
            }
            return view;
        }

        private void OnDatumChange(object sender, EventArgs e)
        {
            if (!this.actionButtonsVisible)
                return;

            // CheckChanged hier aufrufen, da beim OnCheckChanged die CheckBox noch nicht gesetzt war.
            if (this.DateClicked == null)
                return;

            TextView control = sender as TextView;
            if (control == null)
                return;

            int position = (int)control.Tag;

            var args = new StorageItemEventArgs();
            args.StorageItem = items[position].StorageItem;

            this.DateClicked.Invoke(this, args);
        }

        private void DecreaseQuantity(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            int position = (int)button.Tag;

            var item = items[position];

            if (item.StorageItem.Quantity - StorageItemQuantityListViewAdapter.StepValue < 0)
                return;

            item.StorageItem.Quantity     -= StorageItemQuantityListViewAdapter.StepValue;
            item.StorageItem.QuantityDiff -= StorageItemQuantityListViewAdapter.StepValue;

            this.NotifyDataSetChanged();
        }

        private void IncreaseQuantity(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            int position = (int)button.Tag;

            var item = items[position];

            item.StorageItem.Quantity     += StorageItemQuantityListViewAdapter.StepValue;
            item.StorageItem.QuantityDiff += StorageItemQuantityListViewAdapter.StepValue;
            this.NotifyDataSetChanged();
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