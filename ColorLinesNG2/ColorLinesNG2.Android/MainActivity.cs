using Android.App;
using Android.Content.PM;
using Android.OS;

using HockeyApp.Android;

namespace ColorLinesNG2.Droid {
	[Activity(
		Label = "@string/ApplicationName",
		Icon = "@drawable/icon",
		Theme = "@style/MainTheme",
		ScreenOrientation = ScreenOrientation.Portrait,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation
	)]
	public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity {
		private CLCrashListener crashListener = new CLCrashListener();

		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);
			this.LoadApplication(new ColorLinesNG2.App());
		}
		protected override void OnResume() {
			base.OnResume();
			CrashManager.Register(this, APIKeys.HockeyAppAndroid, crashListener);
		}
		protected override void OnPause() {
			UpdateManager.Unregister();
			base.OnPause();
		}
		protected override void OnDestroy() {
			UpdateManager.Unregister();
			base.OnDestroy();
		}

		private class CLCrashListener : CrashManagerListener {
			public override bool ShouldAutoUploadCrashes() {
				return true;
			}
		}
	}
}
