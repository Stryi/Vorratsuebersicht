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

        public event StorageItemQuantityListViewEventHandler ItemClicked;

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
            anzahl.Click -= OnItemClicked;
            anzahl.Click += OnItemClicked;

            TextView datum = view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Date);
            datum.Tag = position;
            datum.Click -= OnItemClicked;
            datum.Click += OnItemClicked;

            TextView lager = view.FindViewById<TextView>(Resource.Id.StorageItemQuantityList_Storage);
            lager.Tag = position;
            lager.Click -= OnItemClicked;
            lager.Click += OnItemClicked;

            anzahl.Text = item.AnzahlText;
            datum.Text  = item.BestBeforeText;
            lager.Text  = item.LagerText;

            if (string.IsNullOrEmpty(lager.Text))
            {
                lager.Visibility = ViewStates.Gone;
            }
            else
            {
                lager.Visibility = ViewStates.Visible;
            }
            
            if ((lager.Visibility == ViewStates.Visible) && string.IsNullOrEmpty(datum.Text))
            {
                datum.Visibility = ViewStates.Gone;
            }
            else
            {
                datum.Visibility = ViewStates.Visible;
            }

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

        private void OnItemClicked(object sender, EventArgs e)
        {
            if (!this.actionButtonsVisible)
                return;

            // CheckChanged hier aufrufen, da beim OnCheckChanged die CheckBox noch nicht gesetzt war.
            if (this.ItemClicked == null)
                return;

            TextView control = sender as TextView;
            if (control == null)
                return;

            int position = (int)control.Tag;

            var args = new StorageItemEventArgs();
            args.StorageItem = items[position].StorageItem;

            this.ItemClicked.Invoke(this, args);
        }

        private void DecreaseQuantity(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            int position = (int)button.Tag;

            var item = items[position];

            item.StorageItem.Quantity -= StorageItemQuantityListViewAdapter.StepValue;
            item.StorageItem.IsChanged = true;

            if (item.StorageItem.Quantity < 0)
            {
                item.StorageItem.Quantity = 0;
            }

            this.NotifyDataSetChanged();
        }

        private void IncreaseQuantity(object sender, EventArgs e)
        {
            ImageButton button = (ImageButton)sender;
            int position = (int)button.Tag;

            var item = items[position];

            item.StorageItem.Quantity += StorageItemQuantityListViewAdapter.StepValue;
            item.StorageItem.IsChanged = true;
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