using Android.App;
using Android.Content.PM;
using Android.OS;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

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

			AppCenter.Start(APIKeys.AppCenterAndroid, typeof(Analytics), typeof(Crashes));

			global::Xamarin.Forms.Forms.Init(this, bundle);
			this.LoadApplication(new ColorLinesNG2.App());
		}
		protected override void OnResume() {
			base.OnResume();
		}
		protected override void OnPause() {
			base.OnPause();
		}
		protected override void OnDestroy() {
			base.OnDestroy();
		}
	}
}
