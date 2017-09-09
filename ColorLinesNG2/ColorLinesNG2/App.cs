using System.Reflection;

using Xamarin.Forms;

namespace ColorLinesNG2 {
	public class App : Application {
		public const double MinDesktopWidth = 420.0;
		public const double MinDesktopHeight = 768.0;
		public const double MinDesktopRatio = 0.546875;
		public static readonly IAudioManager AudioManager = DependencyService.Get<IAudioManager>();
		public static readonly IStrings Strings;

		private ColorLinesNG game;

		static App() {
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
