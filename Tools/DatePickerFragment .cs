using System;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;

namespace VorratsUebersicht
{
	// https://developer.xamarin.com/guides/android/user_interface/date_picker/

	public class DatePickerFragment : DialogFragment, DatePickerDialog.IOnDateSetListener
	{
		// TAG can be any string of your choice.
		public static readonly string TAG = "X:" + typeof (DatePickerFragment).Name.ToUpper();

		// Initialize this value to prevent NullReferenceExceptions.
		Action<DateTime?> DateSelectedHandler = delegate { };

		public static DatePickerFragment NewInstance(Action<DateTime?> onDateSelected)
		{
			DatePickerFragment frag = new DatePickerFragment();
			frag.DateSelectedHandler = onDateSelected;
			return frag;
		}

		public override Dialog OnCreateDialog(Bundle savedInstanceState)
		{
			DateTime currently = DateTime.Now;
			DatePickerDialog dialog = new DatePickerDialog(Activity, 
														   this, 
														   currently.Year, 
														   currently.Month,
														   currently.Day);

            dialog.SetButton("Ok",          OnOkEventHandling);
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