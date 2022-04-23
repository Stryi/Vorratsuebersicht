using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace VorratsUebersicht
{
	// https://developer.xamarin.com/guides/android/user_interface/date_picker/

	public class DatePickerFragment : Android.Support.V4.App.DialogFragment, DatePickerDialog.IOnDateSetListener
	{
        public DateTime? Date = null;

		// TAG can be any string of your choice.
		public static readonly string TAG = "X:" + typeof (DatePickerFragment).Name.ToUpper();

		// Initialize this value to prevent NullReferenceExceptions.
		Action<DateTime?> DateSelectedHandler = delegate { };

		public static DatePickerFragment NewInstance(Action<DateTime?> onDateSelected, DateTime? date)
		{
			DatePickerFragment frag = new DatePickerFragment();
            if (date.HasValue)
            {
                frag.Date = date.Value.AddMonths(-1);
            }
			frag.DateSelectedHandler = onDateSelected;
			return frag;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
            if (this.Date == null)
            {
			    this.Date = DateTime.Now;
            }
			DatePickerDialog dialog = new DatePickerDialog(Activity, 
				this, 
				this.Date.Value.Year, 
				this.Date.Value.Month,
				this.Date.Value.Day);

            dialog.SetButton(this.Resources.GetString(Resource.String.App_Ok),          OnOkEventHandling);
            dialog.SetButton2("Kein Datum", OnCancelEventHandling);

			return dialog;
		}

        void OnOkEventHandling(object sender, DialogClickEventArgs e)
        {
            DatePickerDialog dialog = sender as DatePickerDialog;
            if (dialog == null)
                return;
            int year  = dialog.DatePicker.Year;
            int month = dialog.DatePicker.Month;
            int day   = dialog.DatePicker.DayOfMonth;

            // Note: monthOfYear is a value between 0 and 11, not 1 and 12!
            DateTime selectedDate = new DateTime(year, month + 1, day);
            DateSelectedHandler(selectedDate);
        }


        void OnCancelEventHandling(object sender, DialogClickEventArgs e)
        {            
            DateSelectedHandler(null);
        }

        public void OnDateSet(DatePicker view, int year, int monthOfYear, int dayOfMonth)
        {
        }
    }
}