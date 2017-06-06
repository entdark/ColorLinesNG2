using Foundation;

[assembly: Xamarin.Forms.Dependency(typeof(ColorLinesNG2.iOS.StringsDependency))]
namespace ColorLinesNG2.iOS {
	public class StringsDependency : IStrings {
		public string Animations {
			get {
				return NSBundle.MainBundle.LocalizedString("Animations", "");
			}
		}
		public string ApplicationName {
			get {
				return NSBundle.MainBundle.LocalizedString("ApplicationName", "");
			}
		}
		public string CompleteTutorial {
			get {
				return NSBundle.MainBundle.LocalizedString("CompleteTutorial", "");
			}
		}
		public string ConfirmMove {
			get {
				return NSBundle.MainBundle.LocalizedString("ConfirmMove", "");
			}
		}
		public string Exit {
			get {
				return NSBundle.MainBundle.LocalizedString("Exit", "");
			}
		}
		public string ExitQ {
			get {
				return NSBundle.MainBundle.LocalizedString("ExitQ", "");
			}
		}
		public string GameOver {
			get {
				return NSBundle.MainBundle.LocalizedString("GameOver", "");
			}
		}
		public string Hi {
			get {
				return NSBundle.MainBundle.LocalizedString("Hi", "");
			}
		}
		public string Name {
			get {
				return NSBundle.MainBundle.LocalizedString("Name", "");
			}
		}
		public string NewRecord {
			get {
				return NSBundle.MainBundle.LocalizedString("NewRecord", "");
			}
		}
		public string Next {
			get {
				return NSBundle.MainBundle.LocalizedString("Next", "");
			}
		}
		public string No {
			get {
				return NSBundle.MainBundle.LocalizedString("No", "");
			}
		}
		public string Restart {
			get {
				return NSBundle.MainBundle.LocalizedString("Restart", "");
			}
		}
		public string RestartQ {
			get {
				return NSBundle.MainBundle.LocalizedString("RestartQ", "");
			}
		}
		public string Results {
			get {
				return NSBundle.MainBundle.LocalizedString("Results", "");
			}
		}
		public string Route {
			get {
				return NSBundle.MainBundle.LocalizedString("Route", "");
			}
		}
		public string Settings {
			get {
				return NSBundle.MainBundle.LocalizedString("Settings", "");
			}
		}
		public string Skip {
			get {
				return NSBundle.MainBundle.LocalizedString("Skip", "");
			}
		}
		public string TapMe {
			get {
				return NSBundle.MainBundle.LocalizedString("TapMe", "");
			}
		}
		public string TutBlocked {
			get {
				return NSBundle.MainBundle.LocalizedString("TutBlocked", "");
			}
		}
		public string TutMakeLine {
			get {
				return NSBundle.MainBundle.LocalizedString("TutMakeLine", "");
			}
		}
		public string TutTapBall {
			get {
				return NSBundle.MainBundle.LocalizedString("TutTapBall", "");
			}
		}
		public string TutTapBlock {
			get {
				return NSBundle.MainBundle.LocalizedString("TutTapBlock", "");
			}
		}
		public string Yes {
			get {
				return NSBundle.MainBundle.LocalizedString("Yes", "");
			}
		}
	}
}
