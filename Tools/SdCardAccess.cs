using System;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;

namespace VorratsUebersicht
{
    using static Tools;

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

            TRACE("Request permissions on storage.");
            ((Activity)context).RequestPermissions(new string[] {storageWritePermission }, 20010);

            return false;
        }

    }
}