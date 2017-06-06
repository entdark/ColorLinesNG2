using Xamarin.Forms;

namespace ColorLinesNG2 {
	public static class Strings {
		private static IStrings strings = null;
		public static void Init() {
			if (Device.RuntimePlatform == Device.iOS || Device.RuntimePlatform == Device.Android) {
				var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
				StringsShared.Culture = ci; // set the RESX for resource localization
				DependencyService.Get<ILocalize>().SetLocale(ci); // set the Thread for locale-aware methods
				//special hack since iOS localization does not work in shared
				if (!ci.Name.Contains("en") && StringsShared.Yes == "YES") {
					strings = DependencyService.Get<IStrings>();
				}
			}
		}
		
		public static string Animations {
			get {
				if (strings == null)
					return StringsShared.Animations;
				return strings.Animations;
			}
		}

		public static string ApplicationName {
			get {
				if (strings == null)
					return StringsShared.ApplicationName;
				return strings.ApplicationName;
			}
		}

		public static string CompleteTutorial {
			get {
				if (strings == null)
					return StringsShared.CompleteTutorial;
				return strings.CompleteTutorial;
			}
		}

		public static string ConfirmMove {
			get {
				if (strings == null)
					return StringsShared.ConfirmMove;
				return strings.ConfirmMove;
			}
		}

		public static string Exit {
			get {
				if (strings == null)
					return StringsShared.Exit;
				return strings.Exit;
			}
		}

		public static string ExitQ {
			get {
				if (strings == null)
					return StringsShared.ExitQ;
				return strings.ExitQ;
			}
		}

		public static string GameOver {
			get {
				if (strings == null)
					return StringsShared.GameOver;
				return strings.GameOver;
			}
		}

		public static string Hi {
			get {
				if (strings == null)
					return StringsShared.Hi;
				return strings.Hi;
			}
		}

		public static string Name {
			get {
				if (strings == null)
					return StringsShared.Name;
				return strings.Name;
			}
		}

		public static string NewRecord {
			get {
				if (strings == null)
					return StringsShared.NewRecord;
				return strings.NewRecord;
			}
		}

		public static string Next {
			get {
				if (strings == null)
					return StringsShared.Next;
				return strings.Next;
			}
		}

		public static string No {
			get {
				if (strings == null)
					return StringsShared.No;
				return strings.No;
			}
		}

		public static string Restart {
			get {
				if (strings == null)
					return StringsShared.Restart;
				return strings.Restart;
			}
		}

		public static string RestartQ {
			get {
				if (strings == null)
					return StringsShared.RestartQ;
				return strings.RestartQ;
			}
		}

		public static string Results {
			get {
				if (strings == null)
					return StringsShared.Results;
				return strings.Results;
			}
		}

		public static string Route {
			get {
				if (strings == null)
					return StringsShared.Route;
				return strings.Route;
			}
		}

		public static string Settings {
			get {
				if (strings == null)
					return StringsShared.Settings;
				return strings.Settings;
			}
		}

		public static string Skip {
			get {
				if (strings == null)
					return StringsShared.Skip;
				return strings.Skip;
			}
		}

		public static string TapMe {
			get {
				if (strings == null)
					return StringsShared.TapMe;
				return strings.TapMe;
			}
		}

		public static string TutBlocked {
			get {
				if (strings == null)
					return StringsShared.TutBlocked;
				return strings.TutBlocked;
			}
		}

		public static string TutMakeLine {
			get {
				if (strings == null)
					return StringsShared.TutMakeLine;
				return strings.TutMakeLine;
			}
		}

		public static string TutTapBall {
			get {
				if (strings == null)
					return StringsShared.TutTapBall;
				return strings.TutTapBall;
			}
		}

		public static string TutTapBlock {
			get {
				if (strings == null)
					return StringsShared.TutTapBlock;
				return strings.TutTapBlock;
			}
		}

		public static string Yes {
			get {
				if (strings == null)
					return StringsShared.Yes;
				return strings.Yes;
			}
		}
	}
}