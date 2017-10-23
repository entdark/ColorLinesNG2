using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Support.V7.App;

namespace ColorLinesNG2.Droid {
	[Activity(
		Label = "@string/ApplicationName",
		Theme = "@style/SplashTheme",
		MainLauncher = true,
		NoHistory = true,
		ScreenOrientation = ScreenOrientation.Portrait
	)]
	public class SplashActivity : AppCompatActivity {
		protected override void OnResume() {
			base.OnResume();
			this.StartActivity(new Intent(Application.Context, typeof(MainActivity)));
		}

		//to prevent closing while launching
		public override void OnBackPressed() {}
	}
}