using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace ColorLinesNG2 {
	public static class Settings {
		private static ISettings AppSettings {
			get {
				return CrossSettings.Current;
			}
		}
		
#region SCORES
		private const string scoresKey = "scores_key";
		private static readonly string scoresDefault = "user\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0\nuser\n0";

		public static string Scores {
			get {
				return AppSettings.GetValueOrDefault(scoresKey, scoresDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(scoresKey, value);
			}
		}
#endregion
#region NEXT_COLOURS
		private const string nextColoursKey = "next_colours_key";
		private static readonly bool nextColoursDefault = true;

		public static bool NextColours {
			get {
				return AppSettings.GetValueOrDefault(nextColoursKey, nextColoursDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(nextColoursKey, value);
			}
		}
#endregion
#region ANIMATIONS
		private const string animationsKey = "animations_key";
		private static readonly bool animationsDefault = true;

		public static bool Animations {
			get {
				return AppSettings.GetValueOrDefault(animationsKey, animationsDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(animationsKey, value);
			}
		}
#endregion
#region ROUTE
		private const string routeKey = "route_key";
		private static readonly bool routeDefault = true;

		public static bool Route {
			get {
				return AppSettings.GetValueOrDefault(routeKey, routeDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(routeKey, value);
			}
		}
#endregion
#region CONFIRM_MOVE
		private const string confirmMoveKey = "confirm_move_key";
		private static readonly bool confirmMoveDefault = false;

		public static bool ConfirmMove {
			get {
				return AppSettings.GetValueOrDefault(confirmMoveKey, confirmMoveDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(confirmMoveKey, value);
			}
		}
#endregion
#region TAUGHT
		private const string taughtKey = "taught_key";
		private static readonly bool taughtDefault = false;

		public static bool Taught {
			get {
				return AppSettings.GetValueOrDefault(taughtKey, taughtDefault);
			}
			set {
				AppSettings.AddOrUpdateValue(taughtKey, value);
			}
		}
#endregion
	}
}