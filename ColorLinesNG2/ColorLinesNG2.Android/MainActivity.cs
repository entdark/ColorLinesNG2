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
		protected override void OnCreate(Bundle bundle) {
			base.OnCreate(bundle);

			global::Xamarin.Forms.Forms.Init(this, bundle);
			this.LoadApplication(new ColorLinesNG2.App());
		}
		protected override void OnResume() {
			base.OnResume();
			CrashManager.Register(this, APIKeys.HockeyAppAndroid);
		}
		protected override void OnPause() {
			UpdateManager.Unregister();
			base.OnPause();
		}
		protected override void OnDestroy() {
			UpdateManager.Unregister();
			base.OnDestroy();
		}
	}
}
