using System;
using System.Collections.Generic;
using ZXing.Mobile;
using Android.OS;

using Android.App;
using Android.Widget;
using Android.Content.PM;
using Android.Content;

namespace VorratsUebersicht
{
	[Activity(Label = "ZXing.Net.Mobile", Theme = "@style/Theme.AppCompat.Light", ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.KeyboardHidden)]
	public class ZXingFragmentActivity : AndroidX.Fragment.App.FragmentActivity
	{
		ZXingScannerFragment scanFragment;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			SetContentView(Resource.Layout.ZxingFragmentActivity);

			var flashButton = FindViewById<Button>(Resource.Id.buttonZxingFlash);
			flashButton.Click += (sender, e) => scanFragment.ToggleTorch();
		}

		protected override void OnResume()
		{
			base.OnResume();


			if (scanFragment == null)
			{
				scanFragment = new ZXingScannerFragment();

				SupportFragmentManager.BeginTransaction()
					.Replace(Resource.Id.fragment_container, scanFragment)
					.Commit();
			}

			Scan();
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
			=> Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

		protected override void OnPause()
		{
			scanFragment?.StopScanning();

			base.OnPause();
		}

		void Scan()
		{

			var opts = new MobileBarcodeScanningOptions();
			opts.UseFrontCameraIfAvailable = Settings.GetBoolean("UseFrontCameraForEANScan", false);

			scanFragment.StartScanning(result =>
			{

				// Null result means scanning was cancelled
				if (result == null || string.IsNullOrEmpty(result.Text))
				{
					Toast.MakeText(this, "Scanning Cancelled", ToastLength.Long).Show();
					return;
				}

				RunOnUiThread(() => 
				{
					Intent intent = new Intent();
					intent.PutExtra("EANCode", result.Text);
					this.SetResult(Result.Ok, intent);
					this.Finish();
				});
			}, opts);
		}
	}
}
