[assembly: Xamarin.Forms.Dependency(typeof(ColorLinesNG2.Shared.StringsNew))]
namespace ColorLinesNG2.Shared {
	public class StringsImplementation : IStrings {
		public System.Globalization.CultureInfo Culture {
			get {
				return StringsNew.Culture;
			}
			set {
				StringsNew.Culture = value;
			}
		}

		public string Animations {
			get {
				return StringsNew.Animations;
			}
		}

		public string ApplicationName {
			get {
				return StringsNew.ApplicationName;
			}
		}

		public string CompleteTutorial {
			get {
				return StringsNew.CompleteTutorial;
			}
		}

		public string Exit {
			get {
				return StringsNew.Exit;
			}
		}

		public string ExitQ {
			get {
				return StringsNew.ExitQ;
			}
		}

		public string GameOver {
			get {
				return StringsNew.GameOver;
			}
		}

		public string Hi {
			get {
				return StringsNew.Hi;
			}
		}

		public string Name {
			get {
				return StringsNew.Name;
			}
		}

		public string NewRecord {
			get {
				return StringsNew.NewRecord;
			}
		}

		public string Next {
			get {
				return StringsNew.Next;
			}
		}

		public string No {
			get {
				return StringsNew.No;
			}
		}

		public string Restart {
			get {
				return StringsNew.Restart;
			}
		}

		public string RestartQ {
			get {
				return StringsNew.RestartQ;
			}
		}

		public string Results {
			get {
				return StringsNew.Results;
			}
		}

		public string Route {
			get {
				return StringsNew.Route;
			}
		}

		public string Settings {
			get {
				return StringsNew.Settings;
			}
		}

		public string Skip {
			get {
				return StringsNew.Skip;
			}
		}

		public string TapMe {
			get {
				return StringsNew.TapMe;
			}
		}

		public string TutBlocked {
			get {
				return StringsNew.TutBlocked;
			}
		}

		public string TutMakeLine {
			get {
				return StringsNew.TutMakeLine;
			}
		}

		public string TutTapBall {
			get {
				return StringsNew.TutTapBall;
			}
		}

		public string TutTapBlock {
			get {
				return StringsNew.TutTapBlock;
			}
		}

		public string Yes {
			get {
				return StringsNew.Yes;
			}
		}
	}
}