using System;

using System.Linq;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public class AltDatePickerFragment : DialogFragment
    {
        private View view;
        private LinearLayout root;
        private bool DayWasPicked;
        private string last_clicked_tag = "";

        private TextView date_textview = null;
        private int max_days = 31;
        private int base_year;

        public DateTime? Date = null;
        public static readonly string TAG = "X:" + typeof(AltDatePickerFragment).Name.ToUpper();
        Action<DateTime?> DateSelectedHandler = delegate { };

        struct DateParams
        {
            public int rows, columns;
            public string tag_prefix;
            public int val_base;
            public int amount;

            public DateParams(string tag_prefix, int base_year)
            {
                this.tag_prefix = tag_prefix;
                switch (tag_prefix)
                {
                    case "Y":
                        this.rows = 1;
                        this.columns = 4;
                        this.amount = 4;
                        this.val_base = base_year;
                        break;
                    case "M":
                        this.rows = 2;
                        this.columns = 6;
                        this.amount = 12;
                        this.val_base = 1;
                        break;
                    default:
                        this.rows = 7;
                        this.columns = 5;
                        this.amount = 31;
                        this.val_base = 1;
                        break;
                }
            }
        }

        public static AltDatePickerFragment NewInstance(Action<DateTime?> onDateSelected, DateTime? date)
        {
            AltDatePickerFragment frag = new AltDatePickerFragment();
            frag.Date = date;
            frag.DateSelectedHandler = onDateSelected;
            return frag;
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {

            if (this.Date == null)
                this.Date = DateTime.Now;

            // Mit oder ohne Titel
            Dialog.Window.RequestFeature(WindowFeatures.NoTitle);
            //Dialog.SetTitle("Jahr/Monat/Tag wählen");

            this.view = inflater.Inflate(Resource.Layout.AltDatePickerFragment, container, false);
            this.root = this.view.FindViewById<LinearLayout>(Resource.Id.pd_root);
            InitBase(DateTime.Now.Year);
            return this.view;
        }

        private void InitBase(int base_year)
        {
            LinearLayout.LayoutParams lp;
            LinearLayout ll;
            Button b;

            this.DayWasPicked = false;
            this.base_year = base_year;
            this.root.RemoveAllViews();

            /*
                        // Überschrift und 'Kein Datum' in die oberste Zeile
                        ll = new LinearLayout(this.view.Context);
                        lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                        ll.Orientation = Orientation.Horizontal;
                        ll.WeightSum = 10;
                        this.root.AddView(ll);
                        TextView tv = new TextView(this.view.Context);
                        tv.Text = "Jahr / Monat / Tag wählen";
                        lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                        lp.Width = 0;
                        lp.Weight = 7;
                        lp.Gravity = GravityFlags.Center;
                        lp.SetMargins(1, 1, 1, 1);
                        tv.Gravity = GravityFlags.Center;
                        tv.SetTextSize(Android.Util.ComplexUnitType.Dip, 20f);
                        tv.LayoutParameters = lp;
                        ll.AddView(tv);

                        b = new Button(this.view.Context);
                        b.Text = "Kein Datum";
                        lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                        lp.Width = 0;
                        lp.Weight = 3;
                        lp.SetMargins(1, 1, 1, 1);
                        b.SetBackgroundColor(Android.Graphics.Color.OrangeRed);
                        b.SetTextColor(Android.Graphics.Color.Black);
                        b.SetPadding(0, 0, 0, 0);
                        b.LayoutParameters = lp;
                        b.Click += delegate { DateSelectedHandler(null); Dismiss(); };
                        ll.AddView(b);
            */

            ll = BuildGrid("Y", 10);
            b = new Button(this.view.Context);
            b.Text = ">";
            lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            lp.Width = 0;
            lp.Weight = 1;
            lp.SetMargins(1, 1, 1, 1);
            b.SetBackgroundColor(Android.Graphics.Color.LightGray);
            b.SetTextColor(Android.Graphics.Color.Black);
            b.SetPadding(0, 0, 0, 0);
            b.LayoutParameters = lp;
            b.Click += OnNextYearCLicked;
            ll.AddView(b);

            b = new Button(this.view.Context);
            b.Text = "<";
            lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            lp.Width = 0;
            lp.Weight = 1;
            lp.SetMargins(1, 1, 1, 1);
            b.SetBackgroundColor(Android.Graphics.Color.LightGray);
            b.SetTextColor(Android.Graphics.Color.Black);
            b.SetPadding(0, 0, 0, 0);
            b.LayoutParameters = lp;
            b.Click += OnPrevYearCLicked;
            ll.AddView(b, 0);


            BuildGrid("M");
            ll = BuildGrid("D");

            this.date_textview = new TextView(this.view.Context);
            lp= new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            lp.Width = 0;
            lp.Weight = 2;
            lp.Gravity = GravityFlags.Center;
            lp.SetMargins(1, 1, 1, 1);

            this.date_textview.Gravity = GravityFlags.Center;
            //this.date_textview.SetTextSize(Android.Util.ComplexUnitType.Dip, 20f);
            //this.date_textview.SetBackgroundColor(Android.Graphics.Color.Aquamarine);
            //this.date_textview.SetTextAppearance(Android.Resource.Style.TextAppearanceDeviceDefaultLarge);
            this.date_textview.LayoutParameters = lp;
            ll.AddView(this.date_textview);

            b = new Button(this.view.Context);
            b.Text = "Kein Datum";
            lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            lp.Width = 0;
            lp.Weight = 3;
            lp.SetMargins(1, 1, 1, 1);
            b.SetBackgroundColor(Android.Graphics.Color.OrangeRed);
            b.SetTextColor(Android.Graphics.Color.Black);
            b.SetPadding(0, 0, 0, 0);
            b.LayoutParameters = lp;
            b.Click += delegate { DateSelectedHandler(null); Dismiss(); };
            ll.AddView(b);

            b = new Button(this.view.Context);
            b.Text = "OK";
            lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
            lp.Width = 0;
            lp.Weight = 3;
            lp.SetMargins(1, 1, 1, 1);
            b.SetBackgroundColor(Android.Graphics.Color.LightGreen);
            b.SetPadding(0, 0, 0, 0);
            b.LayoutParameters = lp;
            b.Click += OnCloseCLicked;
            ll.AddView(b);

            DateTime? d = this.Date;
            UpdateGrid("Y", d.Value.Year, false);
            UpdateGrid("M", d.Value.Month, false);
            UpdateGrid("D", d.Value.Day, false);

            ShowDate();
        }

        private LinearLayout BuildGrid(string tag_prefix, int weight_sum=0, int button_weight=1)
        {
            DateParams par = new DateParams(tag_prefix, this.base_year);

            LinearLayout ll = this.root;
            LinearLayout llc = null;

            if (par.rows > 1)
            {
                ll = new LinearLayout(this.view.Context);
                ll.Orientation = Orientation.Vertical;
                ll.SetPadding(0, 10, 0, 0);
                this.root.AddView(ll);
            }
            int amount = par.amount;
            for (int r = 0; r < par.rows; r++)
            {
                llc = new LinearLayout(this.view.Context);
                LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                llc.Orientation = Orientation.Horizontal;
                //llc.SetPadding(0, 0, 0, 0);
                llc.WeightSum = weight_sum == 0 ? par.columns*2 : weight_sum;
                ll.AddView(llc);
                for (int c = 0; c < par.columns; c++)
                {
                    Button b = new Button(this.view.Context);
                    b.Text = "" + (r * par.columns + c + par.val_base);
                    b.Tag = par.tag_prefix + (r * par.columns + c + par.val_base);
                    b.Click += OnClicked;
                    llc.AddView(b);
                    amount -= 1;
                    if (amount <= 0) break;
                }
            }
            return llc;
        }

        private void OnClicked(object sender, EventArgs e)
        {
            Button b = (Button)sender;
            string tag = (string)b.Tag;
            int val = Int32.Parse(tag.Substring(1, tag.Length - 1));

            if (tag == this.last_clicked_tag)
            {
                DateSelectedHandler(this.Date);
                this.Dismiss();
                return;
            }
            this.last_clicked_tag = tag;

            /*
            // Doppelclick
            if ((tag.Substring(0, 1) == "M" && val == this.Date.Value.Month) || (tag.Substring(0, 1) == "D" && val == this.Date.Value.Day)
                || (DayWasPicked && tag.Substring(0, 1) == "Y" && val == this.Date.Value.Year))
            {
                DateSelectedHandler(this.Date);
                this.Dismiss();
                return;
            }
            */
            UpdateGrid(tag.Substring(0, 1), val, true);
        }

        private void UpdateGrid(string tag_prefix, int val, bool user_input)
        {
            LinearLayout.LayoutParams lp;
            DateParams par = new DateParams(tag_prefix, this.base_year);

            for (int i = 0; i < par.amount; i++)
            {
                Button b = (Button)this.view.FindViewWithTag(par.tag_prefix + (i + par.val_base));
                if (par.val_base + i == val)
                    b.SetBackgroundColor(Android.Graphics.Color.LightGoldenrodYellow);
                else
                    b.SetBackgroundColor(Android.Graphics.Color.LightGray);// new Android.Graphics.Color(Resource.Color.Text_Warning));
                b.SetTextColor(Android.Graphics.Color.Black);
                lp = new LinearLayout.LayoutParams(LinearLayout.LayoutParams.MatchParent, LinearLayout.LayoutParams.WrapContent);
                lp.Weight = 2;
                lp.Width = 0;
                lp.SetMargins(1, 1, 1, 1);
                b.SetPadding(0, 0, 0, 0);
                b.Visibility = (tag_prefix != "D" || i < max_days) ? ViewStates.Visible : ViewStates.Invisible;
                b.LayoutParameters = lp;
            }

            int day = this.Date.Value.Day;
            int month = this.Date.Value.Month;
            int year = this.Date.Value.Year;

            if (par.tag_prefix == "Y")
                year = val;
            else if (par.tag_prefix == "M")
                month = val;
            else
            {
                if (user_input)
                    DayWasPicked = true;
                day = val;
            }

            if (par.tag_prefix != "D")
            {
                this.max_days = 30;
                int[] ldays = { 1, 3, 5, 7, 8, 10, 12 };
                if (ldays.Contains(month))
                    this.max_days = 31;
                if (month == 2)
                {
                    // Schaltjahrberechnung funktioniert so bis 2399
                    if ((year % 4 == 0) && (year % 100 != 0))
                        this.max_days = 29;
                    else
                        this.max_days = 28;
                }
                if (!DayWasPicked)
                    day = this.max_days;
            }
            if (day > this.max_days)
                day = this.max_days;
            this.Date = new DateTime(year, month, day);

            if (par.tag_prefix != "D")
                UpdateGrid("D", day, false);

            ShowDate();
        }

        private void ShowDate()
        {
            if (this.date_textview != null)
            {
                string s = this.Date.Value.ToShortDateString();
                s = s.Insert(s.Length-4,"\n");
                this.date_textview.Text = s;
            }
        }
        private void OnCloseCLicked(object sender, EventArgs e)
        {
            DateSelectedHandler(this.Date);
            this.Dismiss();
        }
        private void OnNextYearCLicked(object sender, EventArgs e)
        {
            this.InitBase(this.base_year + 1);
        }
        private void OnPrevYearCLicked(object sender, EventArgs e)
        {
            this.InitBase(this.base_year - 1);
        }
    }
}