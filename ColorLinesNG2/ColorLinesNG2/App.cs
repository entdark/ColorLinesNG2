using System.Reflection;

using Xamarin.Forms;

namespace ColorLinesNG2 {
	public class App : Application {
		public const double MinDesktopWidth = 377.0;
		public const double MinDesktopHeight = 690.0;
		public const double MinDesktopRatio = 0.546875;

		private static bool? isPhoneX = null;
		public static bool iPhoneX {
#if __IOS__
			get {
				if (isPhoneX == null) {
					if (new System.Version(ObjCRuntime.Constants.Version) < new System.Version(11, 0)) {
						isPhoneX = false;
					} else {
						if (ObjCRuntime.Runtime.Arch == ObjCRuntime.Arch.DEVICE) {
							using (var context = new LocalAuthentication.LAContext()) {
								isPhoneX = context.BiometryType == LocalAuthentication.LABiometryType.TypeFaceId;
							}
						} else {
							isPhoneX = UIKit.UIScreen.MainScreen.PreferredMode.Size.Height == 2436;
						}
					}
				}
				return (bool)isPhoneX;
			}
#else
			get => false;
#endif
		}
		public static double iOSTopOffset {
			get => iPhoneX ? 44.0 : 20.0;
		}
		public static double iOSBottomOffset {
			get => iPhoneX ? 44.0 : 0.0;
		}

		public static readonly IAudioManager AudioManager;
		public static readonly IStrings Strings;

		private ColorLinesNG game;

		static App() {
			App.AudioManager = DependencyService.Get<IAudioManager>();
			var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
			if (ci.Name.ToLower().Contains("ru"))
				App.Strings = new StringsRu();
			else
				App.Strings = new StringsDefault();
		}
		public App() {
			var assembly = typeof(App).GetTypeInfo().Assembly;
			foreach (var res in assembly.GetManifestResourceNames()) {
				System.Diagnostics.Debug.WriteLine("found resource: " + res);
			}
//			string localizedString = Strings.Yes;
			var mainPage = new MainPage();
			this.game = mainPage.Game;
			MainPage = mainPage;
		}

		protected override void OnStart() {
			// Handle when your app starts
		}

		protected override void OnSleep() {
			// Handle when your app sleeps
			this.game.Sleeping = true;
			App.AudioManager.DeactivateAudioSession();
			base.OnSleep();
		}

		protected override void OnResume() {
			// Handle when your app resumes
			this.game.Sleeping = false;
			this.game.GameView.HasRenderLoop = true;
			App.AudioManager.ReactivateAudioSession();
			base.OnResume();
		}
	}
}
