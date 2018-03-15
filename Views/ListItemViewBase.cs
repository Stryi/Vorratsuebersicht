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
using Android.Graphics;

namespace VorratsUebersicht
{
    public class ListItemViewBase
    {
        public virtual int Id {get;}
        public virtual string Heading {get; set;}
        public virtual string SubHeading {get; set;}
        public virtual Bitmap Image {get; set;}
        public virtual int WarningLevel {get; set;}
        public virtual Color WarningColor {get; set;}
    }
}