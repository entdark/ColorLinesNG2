using Xamarin.Forms;

namespace ColorLinesNG2 {
	public static class Strings {
		private static IStrings strings = null;
		public static void Init() {
			var ci = DependencyService.Get<ILocalize>().GetCurrentCultureInfo();
			if (ci.Name.Contains("ru"))
				strings = new StringsRu();
			else
				strings = new StringsDefault();
		}

		public static string Achieve10Desc {
			get {
				return strings.Achieve10Desc;
			}
		}

		public static string Achieve13Desc {
			get {
				return strings.Achieve13Desc;
			}
		}

		public static string Achieve500Desc {
			get {
				return strings.Achieve500Desc;
			}
		}

		public static string Achieve1000Desc {
			get {
				return strings.Achieve1000Desc;
			}
		}

		public static string AchieveBlowReward {
			get {
				return strings.AchieveBlowReward;
			}
		}

		public static string AchieveScoreReward {
			get {
				return strings.AchieveScoreReward;
			}
		}

		public static string Achievement {
			get {
				return strings.Achievement;
			}
		}

		public static string Achievements {
			get {
				return strings.Achievements;
			}
		}

		public static string Animations {
			get {
				return strings.Animations;
			}
		}

		public static string ApplicationName {
			get {
				return strings.ApplicationName;
			}
		}
		
		public static string Back {
			get {
				return strings.Back;
			}
		}

		public static string BackgroundSelected {
			get {
				return strings.BackgroundSelected;
			}
		}

		public static string BallsSkinSelected {
			get {
				return strings.BallsSkinSelected;
			}
		}

		public static string CompleteAchievement {
			get {
				return strings.CompleteAchievement;
			}
		}

		public static string CompleteTutorial {
			get {
				return strings.CompleteTutorial;
			}
		}

		public static string ConfirmMove {
			get {
				return strings.ConfirmMove;
			}
		}

		public static string Exit {
			get {
				return strings.Exit;
			}
		}

		public static string ExitQ {
			get {
				return strings.ExitQ;
			}
		}

		public static string FirstReward {
			get {
				return strings.FirstReward;
			}
		}

		public static string Gallery {
			get {
				return strings.Gallery;
			}
		}

		public static string GameOver {
			get {
				return strings.GameOver;
			}
		}

		public static string Hi {
			get {
				return strings.Hi;
			}
		}

		public static string Menu {
			get {
				return strings.Menu;
			}
		}

		public static string Music {
			get {
				return strings.Music;
			}
		}

		public static string Name {
			get {
				return strings.Name;
			}
		}

		public static string NewRecord {
			get {
				return strings.NewRecord;
			}
		}

		public static string Next {
			get {
				return strings.Next;
			}
		}

		public static string No {
			get {
				return strings.No;
			}
		}

		public static string Restart {
			get {
				return strings.Restart;
			}
		}

		public static string RestartQ {
			get {
				return strings.RestartQ;
			}
		}

		public static string Results {
			get {
				return strings.Results;
			}
		}

		public static string Reward {
			get {
				return strings.Reward;
			}
		}

		public static string RewardLocked {
			get {
				return strings.RewardLocked;
			}
		}

		public static string Route {
			get {
				return strings.Route;
			}
		}

		public static string SelectBackground {
			get {
				return strings.SelectBackground;
			}
		}

		public static string SelectBallsSkin {
			get {
				return strings.SelectBallsSkin;
			}
		}

		public static string Settings {
			get {
				return strings.Settings;
			}
		}

		public static string Skip {
			get {
				return strings.Skip;
			}
		}

		public static string Sounds {
			get {
				return strings.Sounds;
			}
		}

		public static string TapMe {
			get {
				return strings.TapMe;
			}
		}

		public static string TutBlocked {
			get {
				return strings.TutBlocked;
			}
		}

		public static string TutMakeLine {
			get {
				return strings.TutMakeLine;
			}
		}

		public static string TutTapBall {
			get {
				return strings.TutTapBall;
			}
		}

		public static string TutTapBlock {
			get {
				return strings.TutTapBlock;
			}
		}

		public static string Yes {
			get {
				return strings.Yes;
			}
		}
	}
}