using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
	public class table_info
	{
        public int RecNo {get; set;}
        public int cid {get; set;}
        public string name {get; set;}

        public string type {get; set;}
        public int notnull {get; set;}
        public object dflt_value {get; set;}
        public bool pk {get; set;}
	}
}