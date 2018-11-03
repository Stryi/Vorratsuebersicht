using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace VorratsUebersicht
{
    public class SdCardAccess
    {
        private Context context;

        const string storageWritePermission = Android.Manifest.Permission.WriteExternalStorage;

        public void Initializer(Context context)
        {
            this.context = context;
        }

        public bool Grand(Context context)
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.M)
                return true;

            if (context.CheckSelfPermission(storageWritePermission) == (int)Permission.Granted)
            {
                return true;
            }

            ((Activity)context).RequestPermissions(new string[] {storageWritePermission }, 20010);

            return false;
        }

    }
}