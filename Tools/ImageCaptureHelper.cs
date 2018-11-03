using System;
using System.Collections.Generic;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;

namespace VorratsUebersicht
{
    internal class ImageCaptureHelper
    {
        const int TakePhotoId = 1001;

        const string cameraPermission = Android.Manifest.Permission.Camera;
        const string storageWritePermission = Android.Manifest.Permission.WriteExternalStorage;

        private Context context;

        public string FilePath { get { return App._file.Path; } }

        public static class App
        {
            public static Java.IO.File _file;
            public static Java.IO.File _dir;
            //public static Bitmap bitmap;
            public static string fileName;
        }

        public bool Initializer(Context context)
        {
            this.context = context;

            StrictMode.VmPolicy.Builder builder = new StrictMode.VmPolicy.Builder();
            StrictMode.SetVmPolicy(builder.Build());

            if (IsThereAnAppToTakePicture())
            { 
                CreateDirectoryForPictures();
                return true;
            }
            return false;
        }

        private bool IsThereAnAppToTakePicture()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            IList<ResolveInfo> availableActivities =
                this.context.PackageManager.QueryIntentActivities(intent, PackageInfoFlags.MatchDefaultOnly);
            return availableActivities != null && availableActivities.Count > 0;
        }

        public void TakePicture()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
            {
                if (this.context.CheckSelfPermission(cameraPermission) == (int)Permission.Granted &&
                    this.context.CheckSelfPermission(storageWritePermission) == (int)Permission.Granted)
                {
                    StartCameraActivity();
                }
                else
                {
                    ((Activity)this.context).RequestPermissions(new string[] { cameraPermission, storageWritePermission }, TakePhotoId);
                }
            }
            else
            {
                StartCameraActivity();
            }
        }


        public void RequestPermissions(int requestCode, Permission[] grantResults)
        {
            switch (requestCode)
            {
                case TakePhotoId:
                    if (grantResults[0] == Permission.Granted)
                    {
                        StartCameraActivity();
                    }
                    break;
            }
        }

        private void CreateDirectoryForPictures()
        {
            var pictureFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures);
            bool folderExists = pictureFolder.Exists();

            if (folderExists)
            {
                App._dir = new Java.IO.File(pictureFolder, "Vorräte Bilder");
                if (!App._dir.Exists())
                {
                    App._dir.Mkdirs();
                }
                return;
            }

            string internalPicturePath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyPictures);
            App._dir = new Java.IO.File(internalPicturePath, "Vorräte Bilder");
            if (!App._dir.Exists())
            {
                App._dir.Mkdirs();
            }
        }

        private void StartCameraActivity()
        {
            Intent intent = new Intent(MediaStore.ActionImageCapture);
            App.fileName = String.Format("picture_{0}.jpg", Guid.NewGuid());
            App._file = new Java.IO.File(App._dir, App.fileName);
            intent.PutExtra(MediaStore.ExtraOutput, Android.Net.Uri.FromFile(App._file));
            ((Activity)this.context).StartActivityForResult(intent, TakePhotoId);
        }

    }
}