using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Support.V7.App;

namespace ColorLinesNG2.Droid {
	[Activity(
		Label = "@string/ApplicationName",
		Theme = "@style/MyTheme.Splash",
		MainLauncher = true,
		NoHistory = true,
		ScreenOrientation = ScreenOrientation.Portrait
	)]
	public class SplashActivity : AppCompatActivity {
		public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState) {
			base.OnCreate(savedInstanceState, persistentState);
		}

		// Launches the startup task
		protected override void OnResume() {
			base.OnResume();
			StartActivity(new Intent(Application.Context, typeof(MainActivity)));
		}
		
		//to prevent closing while launching
		public override void OnBackPressed() {}
	}
}