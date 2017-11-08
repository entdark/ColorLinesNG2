//#define FPS_TEST

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using CLAnimations;
using CLDataTypes;
using CLRenderer;
using CLTutorialRoutine;

using SimpleAStarExample;

using Xamarin.Forms;

namespace ColorLinesNG2 {
	public class CLField {
		private class CLSettings {
			private CLBallSkins ballsSkin = CLBallSkins.CLDefault;
			public CLBallSkins BallsSkin {
				get {
					return this.ballsSkin;
				}
				set {
					this.ballsSkin = value;
					Settings.BallsSkin = (int)value;
				}
			}
			private CLBackgrounds background = CLBackgrounds.CLDefault;
			public CLBackgrounds Background {
				get {
					return this.background;
				}
				set {
					this.background = value;
					Settings.Background = (int)value;
				}
			}
			private bool nextColours = true;
			public bool NextColours {
				get {
					return this.nextColours;
				}
				set {
					this.nextColours = value;
					Settings.NextColours = value;
				}
			}
			private bool animations = true;
			public bool Animations {
				get {
					return this.animations;
				}
				set {
					this.animations = value;
					Settings.Animations = value;
				}
			}
			private bool route = true;
			public bool Route {
				get {
					return this.route;
				}
				set {
					this.route = value;
					Settings.Route = value;
				}
			}
			private bool confirmMove = false;
			public bool ConfirmMove {
				get {
					return this.confirmMove;
				}
				set {
					this.confirmMove = value;
					Settings.ConfirmMove = value;
				}
			}
			private bool taught = false;
			public bool Taught {
				get {
					return this.taught;
				}
				set {
					this.taught = value;
					Settings.Taught = value;
				}
			}
			private bool sounds = true;
			public bool Sounds {
				get {
					return this.sounds;
				}
				set {
					this.sounds = value;
					Settings.Sounds = value;
				}
			}
			private bool music = true;
			public bool Music {
				get {
					return this.music;
				}
				set {
					this.music = value;
					Settings.Music = value;
				}
			}
			private string achievements = "";
			public string Achievements {
				get {
					return this.achievements;
				}
				set {
					this.achievements = value;
					Settings.Achievements = value;
				}
			}
			private string savedGame = "";
			public string SavedGame {
				get {
					return this.savedGame;
				}
				set {
					this.savedGame = value;
					Settings.SavedGame = value;
				}
			}
			public CLSettings() {
				this.ballsSkin = (CLBallSkins)Settings.BallsSkin;
				this.background = (CLBackgrounds)Settings.Background;
				this.nextColours = Settings.NextColours;
				this.animations = Settings.Animations;
				this.route = Settings.Route;
				this.confirmMove = Settings.ConfirmMove;
				this.taught = Settings.Taught;
				this.sounds = Settings.Sounds;
				this.music = Settings.Music;
				this.achievements = Settings.Achievements;
				this.savedGame = Settings.SavedGame;
			}
		}

		public const float c1d2 = 0.5f;			//1/2
		public const float c1d3 = 0.333333f;	//1/3
		public const float c1d4 = 0.25f;		//1/4
		public const float c1d5 = 0.2f;			//1/5
		public const float c1d6 = 0.166666f;	//1/6
		public const float c1d7 = 0.142857f;	//1/7
		public const float c1d8 = 0.125f;		//1/8
		public const float c1d9 = 0.111111f;	//1/9

		public static readonly Color GreenColor = Color.FromRgba(0, 170, 0, 255);
		public static readonly Color GreenColorTransparent = Color.FromRgba(0, 170, 0, 0);
		public static readonly Color RedColor = Color.FromRgba(255, 0, 0, 255);
		public static readonly Color GrayColor = Color.FromRgba(85, 85, 85, 255);
		public static readonly Color BlackColor = Color.FromRgba(0, 0, 0, 255);
		public static readonly Color WhiteColor = Color.FromRgba(255, 255, 255, 255);

		public const long AnimGreetingsDuration = 700;
		public const long AnimRevealDuration = 512;
		public const long AnimAppearingDuration = 128;
		public const long AnimBlowingDuration = 256;
		public const long AnimMovingDurationCoeff = 50;
		public const long AnimClearingDuration = 256;
		public const long AnimNonRouteDuration = 256;
		public const long AnimPopUpDuration = 170;
		public const long AnimAchievementUnlockedDuration = 1337;
		public const long AnimAchievementUnlockedHideDuration = 256;
		public const long AnimBackgroundFlashDuration = 10000;

		public const long StartMovingSound = CLField.AnimMovingDurationCoeff*5;

		public const long KeyNavigationFlashDuration = 1337;
		public const long KeyNavigationFlashDelay = 700;

		public const int BestScoresCount = 10;

		private long time;
		private bool activeField = true;
		private bool activeLabels = true;
		private bool firstSelected = false;
		private bool selectableNavigation = false;
		private int [][]fieldTextures;

		private int rows, columns;
		private float left, right, bottom, top;
		private float step;
		private int ballsCount;
		private int cellsCount;
		private CLCell cells, selected;
		private CLColour []nextColours;

		private int score;
		private bool extraScore;
		private string [,]scoresTable;

		private bool teaching;
		private bool teachingInit = false;
		private bool greetings = true;
		private bool greetingsClosing = false;
		private CLTutorial selectTutorial;
		private CLTutorial moveTutorial;
		private CLTutorial blowTutorial;
		private CLTutorial blockedTutorial;
		private CLLabel tutLineLabel, tutGreetingsLabel, tutSkipLabel;

		private CLAchievement []achievements;
		private CLSettings settings;
		private View cellsTapView;
		private CLFormsEntry entry;
		private CLLabel bestScoreLabel, userScoreLabel, menuLabel, startLabel, resultsLabel, popUpLabel;
		private CLCheckBox nextColoursCheckBox, animationsCheckBox, routeCheckBox, confirmMoveCheckBox, soundsCheckBox, musicCheckBox;
		private CLMenu menu;
		private Action<float[],bool> mainMenu, settingsMenu, achievementsMenu, galleryMenu;
		private bool popUpAnimating = false;
		private bool tapViewAdded = false;
		private bool achievementAnimating = false;
		private List<CLAnim> pendingAnimations = new List<CLAnim>();

		private CLField() {}
		public CLField(int [][]fieldTextures) : this(9, 9, fieldTextures) {}
		public CLField(int rows, int columns, int [][]fieldTextures) {
			this.settings = new CLSettings();
			this.teaching = !this.settings.Taught;
			this.achievements = CLAchievement.FromSettings(this.settings.Achievements);
			this.cellsCount = 0;
			CLCell c = this.cells = new CLCell(0, 0);
/*			c.Action = delegate() {
				this.CellAction(c);
			};*/
			this.cellsCount++;
			for (int i = 0; i < rows; i++) {
				for (int j = 0; j < columns; j++) {
					if (i == 0 && j == 0)
						continue;
					CLCell ocell = null;
					if (i > 0) {
						ocell = this.cells;
						for (int k = 1; k < i; k++)
							ocell = ocell.Bottom;
						for (int l = 0; l < j; l++)
							ocell = ocell.Right;
					}
					if (j == 0)
						c = null;
					CLCell cell = new CLCell(i, j, top: ocell, left: c);
/*					cell.Action = delegate() {
						this.CellAction(cell);
					};*/
					this.cellsCount++;
					c = cell;
				}
			}
			c = this.cells;
			this.selected = null;
			this.rows = rows;
			this.columns = columns;
			this.score = 0;
			this.scoresTable = LoadBestScores();
			this.fieldTextures = fieldTextures;
			this.nextColours = new CLColour[3];
			this.popUpLabel = null;
			Random r = new Random(DateTime.Now.Millisecond);
			for (uint i = 0; i < 3; i++) {
				this.nextColours[i] = (CLColour)r.Next((int)CLColour.CLRed, (int)CLColour.CLMax);
			}
			MessagingCenter.Subscribe<object,KeyMessage>(this, KeyMessage.Pressed, (sender, message) => {
				this.KeyNavigation(message.Key, message.IsCtrlPressed);
			});
			MessagingCenter.Subscribe<object>(this, KeyMessage.Inactive, (sender) => {
				this.selectableNavigation = false;
			});
		}
		private void CellAction(CLCell cell) {
			if (this.activeField) {
				if (!this.SelectBall(cell)) {
					if (!(this.teaching && cell.Highlight))
						cell.AnimTime = 0;
					this.MoveBall(cell);
				} else {
					CLField.PlaySound("Selected.mp3");
					this.firstSelected = true;
					if (this.settings.Animations || this.teaching) {
						if (!(this.teaching && cell.Highlight))
							cell.AnimTime = this.time;
					}
				}
			} else if (this.popUpLabel != null) {
				this.popUpLabel.OutAction?.Invoke();
			}
		}
		private string [,]LoadBestScores() {
			string []bsS = new string[CLField.BestScoresCount*2] {
				"user", "0", "user", "0", "user", "0", "user", "0", "user", "0",
				"user", "0", "user", "0", "user", "0", "user", "0", "user", "0",
			};
			string [,]bsD = new string[CLField.BestScoresCount, 2];
			var bs = Settings.Scores.Split('\n');
			if (bs.Length >= CLField.BestScoresCount)
				bsS = bs;
			for (int i = 0; i < CLField.BestScoresCount; i++) {
				bsD[i, 0] = bsS[i*2];
				bsD[i, 1] = bsS[i*2+1];
			}
			return bsD;
		}
		private void InitTutorials() {
			this.selectTutorial = new CLTutorial(
				App.Strings.TutTapBall,
				() => {
					this.ForAllCells((c,i,j) => {
						if ((i == 0 && j == 0)
						|| (i == 2 && j == 5)) {
							c.Highlight = true;
						} else {
							c.Highlight = false;
						}
					});
				},
				new CLCell []{
					new CLCell(0, 0) {
						Colour = CLColour.CLGreen
					},
					new CLCell(2, 5) {
						Colour = CLColour.CLGreen
					},
					new CLCell(5, 7) {
						Colour = CLColour.CLBlue
					},
					new CLCell(8, 1) {
						Colour = CLColour.CLRed
					},
					new CLCell(7, 0) {
						Colour = CLColour.CLCyan
					}
				}
			);
			this.moveTutorial = new CLTutorial(
				App.Strings.TutTapBlock,
				() => {
					this.ForAllCells((c,i,j) => {
						if (i == 8 && j == 8) {
							c.Highlight = true;
						} else {
							c.Highlight = false;
						}
					});
				},
				new CLCell []{
					new CLCell(1, 0) {
						Colour = CLColour.CLGreen
					},
					new CLCell(3, 7) {
						Colour = CLColour.CLGreen
					},
					new CLCell(8, 0) {
						Colour = CLColour.CLGreen
					}
				}
			);
			this.blowTutorial = new CLTutorial(
				App.Strings.TutMakeLine,
				() => {
					this.ForAllCells((c,i,j) => {
						if ((i == 8 && j >= 4)
						|| (i >= 4 && j >= 4 && i == j)
						|| (i >= 4 && j == 8)) {
							c.Highlight = true;
							c.HighlightForced = true;
							c.AnimTime = this.time;
						} else if ((i == 1 && j == 0)
						|| (i == 3 && j == 7)
						|| (i == 8 && j == 0)) {
							c.Highlight = true;
							c.AnimTime = this.time;
						} else if ((i == 0 && j == 0)
						|| (i == 2 && j == 5)) {
							if (c.Colour != CLColour.CLNone) {
								c.Highlight = true;
								c.AnimTime = this.time;
							}
						}
					});
				},
				new CLCell []{
					new CLCell(1, 2) {
						Colour = CLColour.CLBrown
					},
					new CLCell(0, 8) {
						Colour = CLColour.CLYellow
					},
					new CLCell(3, 3) {
						Colour = CLColour.CLBlue
					}
				}
			);
			this.blockedTutorial = new CLTutorial(
				App.Strings.TutBlocked,
				() => {
					this.ForAllCells((c, i, j) => {
						if ((i == 8 && j >= 4)
						|| (i >= 4 && j >= 4 && i == j)
						|| (i >= 4 && j == 8)) {
							c.Highlight = true;
							c.HighlightForced = true;
							c.AnimTime = this.time;
						} else if ((i == 1 && j == 0)
						|| (i == 3 && j == 7)
						|| (i == 8 && j == 0)
						|| (i == 0 && j == 0)
						|| (i == 2 && j == 5)) {
							if (c.Colour != CLColour.CLNone) {
								c.Highlight = true;
								c.AnimTime = this.time;
							}
						} else if ((i == 8 && j == 1)
						|| (i == 7 && j == 0)) {
							c.Highlight = true;
							c.HighlightBlocked = true;
							c.AnimTime = this.time;
						}
					});
				},
				new CLCell[]{
					new CLCell(5, 6) {
						Colour = CLColour.CLRed
					},
					new CLCell(6, 3) {
						Colour = CLColour.CLPink
					},
					new CLCell(4, 5) {
						Colour = CLColour.CLPink
					}
				}
			);
		}

		public void Draw(long time) {
			this.time = time;
//			CLReDraw.Rect(-1.0f, 1.0f, 2.0f, 2.0f, Color.Argb(255, 127, 127, 127));
			CLAnim.CheckTimes(this.time);
			this.DrawBackground();
			//just add them - they are transparent, anyways
			if (this.teaching && !this.teachingInit) {
				this.teachingInit = true;
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLSmall);
				this.bestScoreLabel.Draw(textureId);
				this.userScoreLabel.Draw(textureId);
				this.menuLabel.Draw(textureId);
				this.startLabel.Draw(textureId);
				this.resultsLabel.Draw(textureId);
				CLReDraw.Rect(this.left, this.top, step*9.0f, step, Color.FromRgba(0.0, 0.0, 0.0, 1.0));
				CLReDraw.Rect(this.left, this.bottom+step, step*9.0f, step, Color.FromRgba(0.0, 0.0, 0.0, 1.0));
			}
			if (this.teaching && this.greetings) {
				this.tutGreetingsLabel?.ExtraDraw();
				this.tutSkipLabel?.ExtraDraw();
				this.activeField = !CLAnim.ExecQueue(this.time);
				this.DrawFPS(step);
				return;
			}
			bool ignoreTapView = false;
			if (!this.tapViewAdded) {
				this.tapViewAdded = true;
				float xoffset = step * 0.32f * App.Strings.Name.Length;
				CLReDraw.View(this.cellsTapView, -1.0f, 1.0f, step*9.0f, step*9.0f);
				CLReDraw.View(this.entry, -1.0f+step*2.0f+xoffset, 1.0f-step*4.0f, step*5.0f, step);
				Task.Run(async () => {
					await Task.Delay(1000);
					Device.BeginInvokeOnMainThread(() => {
						this.entry.Unfocus();
						this.entry.IsVisible = false;
						this.entry.IsEnabled = false;
					});
					await Task.Delay(500);
					CLField.HideShowKeyboard(true);
				});
			}
//			bool appearing = false;
			this.ForAllCells((c,i,j) => {
/*				if (c.appearing) {
					appearing = true;
				}*/
				if (!(this.settings.Animations || this.teaching)) {
					c.AnimTime = 0;
					c.Appearing = false;
					c.Blowing = false;
					c.Moving = false;
				} else {
					if (c.Selected && c.AnimTime == 0) {
						c.AnimTime = this.time;
					}
				}
				if (!this.firstSelected) {
					ignoreTapView = c.Colour == CLColour.CLNone;
				}
				int cellTexture = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLCell);
				int colourTexture = this.GetTextureId(CLTextureTypes.CLBalls, c.Colour);
				int oldColourTexture = this.GetTextureId(CLTextureTypes.CLBalls, c.OldColour);
				c.Draw(time, j*step-1.0f, 1.0f-i*step, step, step, new int[] { cellTexture, colourTexture, oldColourTexture }, true);
			});
			float radius = step * 0.2f;
			bool nc = this.settings.NextColours;
			if (!this.teaching) {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLCell);
				CLReDraw.Rect(this.left+step*3.0f, this.top, step, step, textureId);
				if (this.nextColours[0] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*3.5f-radius, this.top-step*0.5f+radius, radius*2.0f, radius*2.0f, this.GetTextureId(CLTextureTypes.CLBalls, this.nextColours[0]));
				CLReDraw.Rect(this.left+step*4.0f, this.top, step, step, textureId);
				if (this.nextColours[1] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*4.5f-radius, this.top-step*0.5f+radius, radius*2.0f, radius*2.0f, this.GetTextureId(CLTextureTypes.CLBalls, this.nextColours[1]));
				CLReDraw.Rect(this.left+step*5.0f, this.top, step, step, textureId);
				if (this.nextColours[2] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*5.5f-radius, this.top-step*0.5f+radius, radius*2.0f, radius*2.0f, this.GetTextureId(CLTextureTypes.CLBalls, this.nextColours[2]));

				textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLSmall);
				this.bestScoreLabel.Draw(textureId);
				this.userScoreLabel.Draw(textureId);
				this.menuLabel.Draw(textureId);
				this.startLabel.Draw(textureId);
				this.resultsLabel.Draw(textureId);
			} else {
				if (!this.greetingsClosing)
					this.tutLineLabel?.ExtraDraw();
				this.tutSkipLabel?.ExtraDraw();
			}
			if (this.menu != null && this.menu.Show) {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
				var bounds = new float[] { left, right, bottom, top };
				this.menu.Draw(textureId, bounds);
			}
			this.activeLabels = this.activeField = !CLAnim.ExecQueue(this.time) && this.popUpLabel == null;
			this.DrawKeySelection();
			if (!this.popUpAnimating) {
				this.popUpLabel?.ExtraDraw?.Invoke();
			}
//			this.activeField = !appearing;
			this.DrawFPS(step);
		}
		/*
		 * fancy fps test:
		 * left to right (fps) = 60 30 20 | 15 12 10 | 8.(571428) 7.5 6.(6)
		 */
#if FPS_TEST && DEBUG
		private long lastTime;
		private void DrawFPS(float step) {
			long deltaTime = this.time - this.lastTime;
			deltaTime = (long)(deltaTime*0.06);
			int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMicro);
			CLReDraw.Rect(this.left+step*deltaTime, this.top-step, step, step, textureId);
			this.lastTime = this.time;
		}
#else
		private void DrawFPS(float step) {}
#endif
		private void DrawBackground() {
			if (this.settings.Background == CLBackgrounds.CLDefault)
				return;
			long delta = this.time;
			while (delta > CLField.AnimBackgroundFlashDuration) {
				delta -= CLField.AnimBackgroundFlashDuration;
			}
			CLBackgroundTextures bg = CLField.GetBgTexture(this.settings.Background);
			float danim = CLAnim.Jump(delta, CLField.AnimBackgroundFlashDuration);
			int textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, bg);
			Color fill = CLField.WhiteColor.MultiplyAlpha(0.5f + 0.5f * danim);
			float angle = 69.0f + 23.0f * this.time * 0.00002f;
#if __IOS__
			if (App.iPhoneX) {
				CLReDraw.Rect(-2.4f, 2.4f, 4.8f, 4.8f, textureId, fill, angle);
			} else
#endif
			if (Device.Idiom != TargetIdiom.Desktop) {
				CLReDraw.Rect(-2.0f, 2.0f, 4.0f, 4.0f, textureId, fill, angle);
			} else {
				CLReDraw.Rect(-2.5f, 2.5f, 5.0f, 5.0f, textureId, fill, angle);
			}
		}
		private bool hidingKeySelection = false;
		private long keySelectionFlashTime = 0;
		private void DrawKeySelection() {
			if (this.popUpLabel != null || (this.menu != null && this.menu.Show) || this.achievementAnimating) {
				this.hidingKeySelection = true;
				return;
			} else if (this.hidingKeySelection) {
				if (!this.activeField) {
					return;
				} else {
					this.hidingKeySelection = false;
					keySelectionFlashTime = this.time + CLField.KeyNavigationFlashDelay;
				}
			}
			if (!this.selectableNavigation)
				return;
			bool drawn = false;
			float border = 0.07f * this.step;

			Color fill;
			long delta = this.time - keySelectionFlashTime;
			if (delta < 0 || !this.settings.Animations) {
				fill = CLField.GreenColor;
			} else {
				const long duration = CLField.KeyNavigationFlashDuration;
				while (delta > duration) {
					delta -= duration;
				}
				float danim = CLAnim.Jump(delta, duration);
				fill = CLField.GreenColor.MultiplyAlpha(1.0f - danim);
			}

			this.ForAllCells((c,i,j) => {
				if (drawn)
					return;
				if (!c.KeySelected)
					return;
				float x = (j * step - 1.0f) - border,
					y = (1.0f - i * step) + border,
					width = step + (border * 2.0f),
					height = border;
				//top
				CLReDraw.Rect(x, y, width, height, fill);
				y -= (step + border);
				//bottom
				CLReDraw.Rect(x, y, width, height, fill);
				y += step;
				width = border;
				height = step;
				//left
				CLReDraw.Rect(x, y, width, height, fill);
				x += (step + border);
				//right
				CLReDraw.Rect(x, y, width, height, fill);
			});
		}

		private int AddScore(int ballsCount) {
			if (ballsCount == 5)
				return 10;
			else if (ballsCount == 6)
				return 12;
			else if (ballsCount == 7)
				return 18;
			else if (ballsCount == 8)
				return 28;
			else if (ballsCount >= 9)
				return 42;
			return 0;
		}
		private int AddExtraScore() {
			return this.extraScore ? 1 : 0;
		}

		public void InitVisuals(float left, float right, float bottom, float top, long time, View []hackyViews = null) {
			this.left = left;
			this.right = right;
			this.bottom = bottom;
			this.top = top;

			float stepLocal;
			if (this.rows > this.columns)
				stepLocal = 2.0f / this.rows;
			else
				stepLocal = 2.0f / this.columns;
			this.step = stepLocal;
			float width = step*3, height = step;

			if (hackyViews == null) {
				this.tapViewAdded = false;
				this.cellsTapView = new BoxView() {
					Color = Color.Transparent
				};
				this.entry = new CLFormsEntry() {
					Text = "",
					TextColor = CLField.GreenColor,
					Keyboard = Keyboard.Text,
					LettersLimit = 11,
//					IsVisible = false
				};
			} else {
				this.tapViewAdded = true;
				this.cellsTapView = (BoxView)hackyViews[0];
				this.entry = (CLFormsEntry)hackyViews[6];
				Device.BeginInvokeOnMainThread(() => {
					(this.cellsTapView as BoxView).Color = Color.Transparent;
					this.entry.Text = "";
					this.entry.TextColor = CLField.GreenColor;
					this.entry.Keyboard = Keyboard.Text;
					this.entry.LettersLimit = 11;
					this.entry.IsVisible = false;
					this.entry.IsEnabled = false;
				});
			}
			Gesture.SetTapped(this.cellsTapView, new Command<Point>((point) => {
				int column = (int)(this.columns * point.X);
				column = column != this.columns ? column : (column-1);
				int row = (int)(this.rows * point.Y);
				row = row != this.rows ? row : (row-1);
				CLCell cell = null;
				this.ForAllCells((ce, i, j) => {
					if (cell == null && i == row && j == column)
						cell = ce;
				});
				if (cell != null)
					this.CellAction(cell);
			}));
			if (hackyViews == null)
				hackyViews = new View [7]{ null, null, null, null, null, null, null };
			var colour = !this.teaching ? CLField.GreenColor : CLField.GreenColorTransparent;
			this.bestScoreLabel = new CLLabel(this.scoresTable[0,1], left, top, width, height, colour, TextAlignment.End, hackyView:hackyViews[1]);
			this.userScoreLabel = new CLLabel(this.score.ToString(), left+step*6.0f, top, width, height, colour, TextAlignment.End, hackyView: hackyViews[2]);
			this.menuLabel = new CLLabel(App.Strings.Menu, left, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[3]);
			this.startLabel = new CLLabel(App.Strings.Restart, left+step*3.0f, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[4]);
			this.resultsLabel = new CLLabel(App.Strings.Results, left+step*6.0f, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[5]);

			var bounds = new float[] { left, right, bottom, top };
			this.startLabel.Action = delegate() {
				if (!this.activeLabels)
					return;
				this.activeLabels = false;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
				}
				this.popUpLabel = new CLLabel(App.Strings.RestartQ, -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, height, CLField.RedColor, TextAlignment.Center);
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
					this.popUpLabel.Draw(textureId, bounds);
				};
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = startLabel.GetCoordinates();
					float []endRect = this.popUpLabel.GetCoordinates();
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
					}));
				}
				this.popUpLabel.Action = delegate() {
					this.popUpLabel.Action = null;
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					if (!this.CheckNewScore()) {
						this.ShowGameOver();
					} else {
						this.RequestUserName();
					}
				};
				this.popUpLabel.OutAction = delegate() {
					this.popUpLabel.OutAction = null;
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					this.activeField = true;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = this.popUpLabel.GetCoordinates();
						float []endRect = { -1.0f+step*3.0f, 1.0f-step*4.0f-step*c1d5, step*3.0f, step*3.0f*c1d5 };
						int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
				};
			};
			this.menuLabel.Action = delegate() {
				if (!this.activeLabels)
					return;
				this.activeLabels = false;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				this.popUpLabel?.Dispose();
				float offset = step * 0.5f;
				this.menu = new CLMenu(App.Strings.Menu, -1.0f+step, 1.0f-step, step*7.0f, step*7.0f, this.mainMenu);
				this.menuLabel.Text = App.Strings.Back;
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = menuLabel.GetCoordinates();
					float []endRect = { -1.0f+step, 1.0f-step, step*7.0f, step*7.0f };
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						bool ret = this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
						if (ret) {
							this.menu.Show = true;
						}
						return ret;
					}));
				} else {
					this.menu.Show = true;
				}
				this.menu.Disposed += (sender, ev) => {
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					this.menuLabel.Text = App.Strings.Menu;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = { -1.0f+step, 1.0f-step, step*7.0f, step*7.0f };
						float []endRect = { -1.0f+step*2.0f, 1.0f-step*2.0f, step*5.0f, step*5.0f };
						int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
				};
			};
			this.resultsLabel.Action = delegate() {
				if (!this.activeLabels && (this.entry == null || !this.entry.IsVisible))
					return;
				this.activeLabels = false;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
				}
				this.popUpLabel = new CLLabel(GetBestScoresList(), -1.0f+step, 1.0f-step, step*7.0f, step*7.0f, CLField.GreenColor, TextAlignment.Center, 1.337f);
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
/*					CLReDraw.Rect(-1.0f+step, 1.0f-step, step*7, step*7, textureId);
					this.popUp.DrawText(0.0f, -step*3.5f);*/
					this.popUpLabel.Draw(textureId, bounds);

				};
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					bool fromCentre = !(this.entry == null || !this.entry.IsVisible);
					float []startRect = fromCentre ? new float[] { -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, step } : resultsLabel.GetCoordinates();
					float []endRect = this.popUpLabel.GetCoordinates();
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
					int oldTextureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, fromCentre ? oldTextureId : -1);
					}));
				}
				this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
					this.popUpLabel.Action = this.popUpLabel.OutAction = null;
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					this.activeField = true;
					if (this.settings.Animations && this.popUpLabel != null) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = this.popUpLabel.GetCoordinates();
						float []endRect = { -1.0f+step*2.0f, 1.0f-step*2.0f, step*5.0f, step*5.0f };
						int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
					if (this.popUpLabel != null) {
						this.popUpLabel.Dispose();
					}
					this.popUpLabel = null;
				};
			};
			this.tutGreetingsLabel = new CLLabel(App.Strings.TapMe, left, 1.0f-step*4.0f, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			this.tutGreetingsLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.tutGreetingsLabel.Draw(textureId/*, bounds*/);
			};
			this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = delegate() {
				this.tutGreetingsLabel.Text = App.Strings.Hi;
				this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = delegate() {
					this.tutGreetingsLabel.Text = App.Strings.CompleteTutorial;
					this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = delegate() {
						this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = null;
						this.greetings = false;
						if (this.greetingsClosing)
							return;
						if (this.tutGreetingsLabel != null) {
							this.greetingsClosing = true;
							CLAnim.AddToQueue(new CLAnim(CLField.AnimGreetingsDuration, (start, end, checkTime) => {
								return this.AnimGreetings(start, end, checkTime);
							}));
							this.tutGreetingsLabel.Text = "";
							this.tutGreetingsLabel.Dispose();
						}
						this.tutGreetingsLabel = null;
					};
				};
			};
			this.tutLineLabel = new CLLabel("", left, bottom+step, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			this.tutLineLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.tutLineLabel.Draw(textureId);
			};
			this.tutSkipLabel = new CLLabel(App.Strings.Skip, left+step*3.0f, top, step*3.0f, step, CLField.RedColor, TextAlignment.Center);
			this.tutSkipLabel.Action = delegate() {
				this.tutSkipLabel.Action = null;
				this.greetings = false;
				this.greetingsClosing = false;
				if (this.tutGreetingsLabel != null) {
					this.tutGreetingsLabel.Text = "";
					this.tutGreetingsLabel.Dispose();
					this.tutGreetingsLabel = null;
				}
				this.tutSkipLabel.Text = "";
				this.tutSkipLabel.Dispose();
				this.tutSkipLabel = null;
				this.TutorialApply(this.selectTutorial);
				this.TutorialApply(this.moveTutorial);
				this.TutorialApply(this.blockedTutorial);
				this.TutorialApply(this.blowTutorial);
				this.TutorialApply(null);
				this.ClearField();
/*				this.bestScoreLabel.TextColor =
				this.userScoreLabel.TextColor =
				this.resultsLabel.TextColor =
				this.startLabel.TextColor =
				this.menuLabel.TextColor = CLField.GreenColor;*/
			};
			this.tutSkipLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLSmall);
				this.tutSkipLabel.Draw(textureId);
			};
			this.InitMenus();
			this.InitTutorials();
			this.InitAudio();
			bool gameLoaded = false;
			try {
				gameLoaded = this.LoadGame();
			} catch {}
			if (!gameLoaded)
				this.AddBalls(true);
		}
		private void InitMenus() {
			CLFormsLabel settingsMenuLabel = null, achievementsMenuLabel = null, galleryMenuLabel = null;
			bool mainMenuInited = false;
			this.mainMenu = (xywh,draw) => {
				if (!draw) {
					CLReDraw.ReleaseView(settingsMenuLabel);
					CLReDraw.ReleaseView(achievementsMenuLabel);
					CLReDraw.ReleaseView(galleryMenuLabel);
					mainMenuInited = false;
					return;
				}
				if (xywh == null) {
					return;
				}
				if (mainMenuInited)
					return;
				mainMenuInited = true;
				settingsMenuLabel = new CLFormsLabel() {
					Text = App.Strings.Settings,
					TextScale = 1.2f
				};
				var tap = new TapGestureRecognizer();
				tap.Tapped += (sender, ev) => {
					this.menu.Push(App.Strings.Settings, this.settingsMenu);
					CLField.PlaySound("MenuNav.mp3");
				};
				settingsMenuLabel.GestureRecognizers.Add(tap);
				achievementsMenuLabel = new CLFormsLabel() {
					Text = App.Strings.Achievements,
					TextScale = 1.2f
				};
				tap = new TapGestureRecognizer();
				tap.Tapped += (sender, ev) => {
					this.menu.Push(App.Strings.Achievements, this.achievementsMenu);
					CLField.PlaySound("MenuNav.mp3");
				};
				achievementsMenuLabel.GestureRecognizers.Add(tap);
				galleryMenuLabel = new CLFormsLabel() {
					Text = App.Strings.Gallery,
					TextScale = 1.2f
				};
				tap = new TapGestureRecognizer();
				tap.Tapped += (sender, ev) => {
					this.menu.Push(App.Strings.Gallery, this.galleryMenu);
					CLField.PlaySound("MenuNav.mp3");
				};
				galleryMenuLabel.GestureRecognizers.Add(tap);
				float x = xywh[0], y = xywh[1], width = xywh[2], height = xywh[3];
				CLReDraw.View(settingsMenuLabel, x, y-step*0, width, step);
				CLReDraw.View(achievementsMenuLabel, x, y-step*1, width, step);
				CLReDraw.View(galleryMenuLabel, x, y-step*2, width, step);
			};
			bool settingsMenuInited = false;
			this.settingsMenu = (xywh,draw) => {
				if (!draw) {
					this.settings.Sounds = this.soundsCheckBox.Check;
					this.settings.Music = this.musicCheckBox.Check;
					Device.BeginInvokeOnMainThread(() => {
						App.AudioManager.SoundsEnabled = this.settings.Sounds;
						if (App.AudioManager.MusicEnabled != this.settings.Music)
							App.AudioManager.MusicEnabled = this.settings.Music;
					});
					this.settings.NextColours = this.nextColoursCheckBox.Check;
					this.settings.Animations = this.animationsCheckBox.Check;
					this.settings.Route = this.routeCheckBox.Check;
					this.settings.ConfirmMove = this.confirmMoveCheckBox.Check;
					this.soundsCheckBox.Dispose();
					this.musicCheckBox.Dispose();
					this.nextColoursCheckBox.Dispose();
					this.animationsCheckBox.Dispose();
					this.routeCheckBox.Dispose();
					this.confirmMoveCheckBox.Dispose();
					settingsMenuInited = false;
					return;
				}
				if (xywh == null) {
					return;
				}
				if (!settingsMenuInited) {
					settingsMenuInited = true;
					float x = xywh[0], y = xywh[1], width = xywh[2], height = xywh[3];
					float offset = step * 0.5f;
					float spacing = step * 5.0f * c1d6;
					this.soundsCheckBox = new CLCheckBox(this.settings.Sounds, App.Strings.Sounds, x+offset, y-spacing*0.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f);
					this.musicCheckBox = new CLCheckBox(this.settings.Music, App.Strings.Music, x+offset, y-spacing*1.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f);
					this.nextColoursCheckBox = new CLCheckBox(this.settings.NextColours, App.Strings.Next, x+offset, y-spacing*2.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f);
					this.animationsCheckBox = new CLCheckBox(this.settings.Animations, App.Strings.Animations, x+offset, y-spacing*4.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f);
					this.routeCheckBox = new CLCheckBox(this.settings.Route, App.Strings.Route, x+offset, y-spacing*5.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f) {
						IsEnabled = this.settings.Animations
					};
					this.confirmMoveCheckBox = new CLCheckBox(this.settings.ConfirmMove, App.Strings.ConfirmMove, x+offset, y-spacing*3.0f, width-offset*2.0f, spacing, CLField.GreenColor, TextAlignment.Start, 1.2f);
					this.animationsCheckBox.Checked += (sender2, ev2) => {
						this.routeCheckBox.IsEnabled = ev2.Checked;
					};
				}
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
				int textureId2 = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMicro);
				this.soundsCheckBox.Draw(textureId, textureId2);
				this.musicCheckBox.Draw(textureId, textureId2);
				this.nextColoursCheckBox.Draw(textureId, textureId2);
				this.confirmMoveCheckBox.Draw(textureId, textureId2);
				//draw animations after route so that animations is on top of the drawing tree
				//and thus animations recognize gestures when route is disabled (placed just under animations)
				this.routeCheckBox.Draw(textureId, textureId2);
				this.animationsCheckBox.Draw(textureId, textureId2);
			};
			CLLabel achieve10 = null, achieve13 = null, achieve500 = null, achieve1000 = null;
			CLLabel []achieve = null;
			bool achievementsMenuInited = false;
			this.achievementsMenu = (xywh,draw) => {
				if (!draw) {
					foreach (var achievement in this.achievements) {
						achieve[(int)achievement.Id]?.Dispose();
					}
					achievementsMenuInited = false;
					return;
				}
				if (xywh == null) {
					return;
				}
				if (!achievementsMenuInited) {
					achievementsMenuInited = true;
					float x = xywh[0], y = xywh[1], width = xywh[2], height = xywh[3];
					float offset = step * 0.5f;
					achieve10 = new CLLabel("", x+step*2.0f, y-step*1.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					achieve13 = new CLLabel("", x+step*4.0f, y-step*1.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					achieve500 = new CLLabel("", x+step*2.0f, y-step*3.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					achieve1000 = new CLLabel("", x+step*4.0f, y-step*3.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					achieve = new CLLabel [(int)CLAchievements.CLMax]{ achieve10, achieve13, achieve500, achieve1000 };
					foreach (var achievement in this.achievements) {
						achieve[(int)achievement.Id].Action = delegate () {
							this.ShowAchievementDesctiption(achievement.Id, achieve[(int)achievement.Id].GetCoordinates());
						};
					}
				}
				foreach (var achievement in this.achievements) {
					int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
					achieve[(int)achievement.Id].Draw(textureId, grayscale: !achievement.Achieved);
				}
			};
			CLLabel defaultSkin = null, altSkin = null, alt2Skin = null, defaultBg = null, starsBg = null, nebulaBg = null;
			float[] xywhSelected = null, xywhSelected2 = null;
			bool galleryMenuInited = false;
			this.galleryMenu = (xywh, draw) => {
				if (!draw) {
					defaultSkin?.Dispose();
					altSkin?.Dispose();
					alt2Skin?.Dispose();
					defaultBg?.Dispose();
					starsBg?.Dispose();
					nebulaBg?.Dispose();
					xywhSelected = null;
					xywhSelected2 = null;
					galleryMenuInited = false;
					return;
				}
				if (xywh == null) {
					return;
				}
				if (!galleryMenuInited) {
					galleryMenuInited = true;
					float x = xywh[0], y = xywh[1], width = xywh[2], height = xywh[3];
					float offset = step * 0.5f;
					defaultSkin = new CLLabel("", x+step*1.0f, y-step*1.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					altSkin = new CLLabel("", x+step*3.0f, y-step*1.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					alt2Skin = new CLLabel("", x+step*5.0f, y-step*1.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					defaultBg = new CLLabel("", x+step*1.0f, y-step*3.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					starsBg = new CLLabel("", x+step*3.0f, y-step*3.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					nebulaBg = new CLLabel("", x+step*5.0f, y-step*3.0f, step, step, CLField.GreenColor, TextAlignment.Center);
					defaultSkin.Action = delegate() {
						this.ShowSkinSelection(CLBallSkins.CLDefault, defaultSkin.GetCoordinates());
					};
					altSkin.Action = delegate() {
						this.ShowSkinSelection(CLBallSkins.CLAlt, altSkin.GetCoordinates());
					};
					alt2Skin.Action = delegate() {
						this.ShowSkinSelection(CLBallSkins.CLAlt2, alt2Skin.GetCoordinates());
					};
					defaultBg.Action = delegate() {
						this.ShowBackgroundSelection(CLBackgrounds.CLDefault, defaultBg.GetCoordinates());
					};
					starsBg.Action = delegate() {
						this.ShowBackgroundSelection(CLBackgrounds.CLStars, starsBg.GetCoordinates());
					};
					nebulaBg.Action = delegate() {
						this.ShowBackgroundSelection(CLBackgrounds.CLNebula, nebulaBg.GetCoordinates());
					};
				}
				float border = 0.07f * step;
				if (CLAchievement.GetAchieved(this.achievements, CLAchievements.CLBlow10)
					|| CLAchievement.GetAchieved(this.achievements, CLAchievements.CLBlow13)) {
					var ballsSkin = this.settings.BallsSkin;
					if (ballsSkin == CLBallSkins.CLDefault) {
						xywhSelected = defaultSkin.GetCoordinates();
					} else if (ballsSkin == CLBallSkins.CLAlt) {
						xywhSelected = altSkin.GetCoordinates();
					} else {
						xywhSelected = alt2Skin.GetCoordinates();
					}
					xywhSelected[0] -= border;
					xywhSelected[1] += border;
					xywhSelected[2] += (border * 2.0f);
					xywhSelected[3] += (border * 2.0f);
					CLReDraw.Rect(xywhSelected[0], xywhSelected[1], xywhSelected[2], xywhSelected[3], CLField.GreenColor);
				}
				if (CLAchievement.GetAchieved(this.achievements, CLAchievements.CLScore500)
					|| CLAchievement.GetAchieved(this.achievements, CLAchievements.CLScore1000)) {
					var background = this.settings.Background;
					if (background == CLBackgrounds.CLDefault) {
						xywhSelected2 = defaultBg.GetCoordinates();
					} else if (background == CLBackgrounds.CLStars) {
						xywhSelected2 = starsBg.GetCoordinates();
					} else {
						xywhSelected2 = nebulaBg.GetCoordinates();
					}
					xywhSelected2[0] -= border;
					xywhSelected2[1] += border;
					xywhSelected2[2] += (border * 2.0f);
					xywhSelected2[3] += (border * 2.0f);
					CLReDraw.Rect(xywhSelected2[0], xywhSelected2[1], xywhSelected2[2], xywhSelected2[3], CLField.GreenColor);
				}

				int cellTextureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLCell);
				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, CLColour.CLCyan, CLBallSkins.CLDefault);
				float []xywh2 = defaultSkin.GetCoordinates();
				CLReDraw.Rect(xywh2[0], xywh2[1], xywh2[2], xywh2[3], cellTextureId);
				defaultSkin.Draw(textureId);

				textureId = this.GetTextureId(CLTextureTypes.CLBalls, CLColour.CLGreen, CLBallSkins.CLAlt); // turquoise
				xywh2 = altSkin.GetCoordinates();
				CLReDraw.Rect(xywh2[0], xywh2[1], xywh2[2], xywh2[3], cellTextureId);
				altSkin.Draw(textureId, grayscale: !CLAchievement.GetAchieved(this.achievements, CLAchievements.CLBlow10));

				textureId = this.GetTextureId(CLTextureTypes.CLBalls, CLColour.CLRed, CLBallSkins.CLAlt2); // sakura
				xywh2 = alt2Skin.GetCoordinates();
				CLReDraw.Rect(xywh2[0], xywh2[1], xywh2[2], xywh2[3], cellTextureId);
				alt2Skin.Draw(textureId, grayscale: !CLAchievement.GetAchieved(this.achievements, CLAchievements.CLBlow13));

				textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, CLBackgroundTextures.CLDefaultSmall);
				defaultBg.Draw(textureId);

				textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, CLBackgroundTextures.CLStarsSmall);
				starsBg.Draw(textureId, grayscale: !CLAchievement.GetAchieved(this.achievements, CLAchievements.CLScore500));

				textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, CLBackgroundTextures.CLNebulaSmall);
				nebulaBg.Draw(textureId, grayscale: !CLAchievement.GetAchieved(this.achievements, CLAchievements.CLScore1000));
			};
		}
		private void InitAudio() {
			Device.BeginInvokeOnMainThread(() => {
				App.AudioManager.PrecacheSounds(new []{
					"Achievement.mp3", "Blocked.mp3", "Blow.mp3", "GameOver.mp3", "MenuNav.mp3", "Move.mp3", "NewRecord.mp3", "Selected.mp3"
				});
				App.AudioManager.PlayBackgroundMusic("Music.mp3");
				App.AudioManager.SoundsEnabled = this.settings.Sounds;
				if (this.teaching) {
					App.AudioManager.MusicEnabled = false;
				} else {
					App.AudioManager.MusicEnabled = this.settings.Music;
				}
			});
		}

		private void ShowAchievementDesctiption(CLAchievements id, float[]startRectExternal) {
			if (this.popUpAnimating)
				return;
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			if (this.settings.Animations) {
				this.activeField = false;
				this.popUpAnimating = true;
				float []startRect = startRectExternal;
				float []endRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
				}));
			}
			CLAchievement achievement = CLAchievement.GetAchievement(this.achievements, id);
			this.popUpLabel = new CLLabel(App.Strings.Achievement + achievement.Description, -1.0f, 1.0f-step*4.0f, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate () {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.popUpLabel.Draw(textureId, new float[] { left, right, bottom, top });
			};
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				if (this.popUpAnimating)
					return;
				this.popUpLabel.Text = App.Strings.Reward + achievement.Unlocks;
				this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
					if (this.popUpAnimating)
						return;
					this.popUpLabel.Action = this.popUpLabel.OutAction = null;
					this.activeField = true;
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
						float []endRect = startRectExternal;
						int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
				};
			};
		}

		private void ShowSkinSelection(CLBallSkins ballsSkin, float[] startRectExternal) {
			if (this.popUpAnimating)
				return;
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			if (this.settings.Animations) {
				this.activeField = false;
				this.popUpAnimating = true;
				float []startRect = startRectExternal;
				float []endRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.ShowSkinColoursAnimation(ballsSkin, startRectExternal);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					bool ret = this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
					if (ret) {
						//we need this to add the label with text but continue animations
						this.popUpLabel.Draw(this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong), new float[] { left, right, bottom, top });
					}
					return ret;
				}), true);
			}
			bool defaultSkin = ballsSkin == CLBallSkins.CLDefault;
			bool achieved = false;
			CLAchievement achievement = null;
			switch (ballsSkin) {
			case CLBallSkins.CLAlt:
				achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLBlow10);
				achieved = achievement.Achieved;
				break;
			case CLBallSkins.CLAlt2:
				achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLBlow13);
				achieved = achievement.Achieved;
				break;
			}
			bool unlocked = !defaultSkin && achieved;
			bool selectSkin = false, showSecondLocked = false;
			string text = "";
			if (ballsSkin == this.settings.BallsSkin) {
				text = App.Strings.BallsSkinSelected;
			} else {
				if (defaultSkin || unlocked) {
					selectSkin = true;
					text = App.Strings.SelectBallsSkin;
				} else if (!unlocked) {
					showSecondLocked = true;
					text = App.Strings.RewardLocked;
				}
			}
			this.popUpLabel = new CLLabel(text, -1.0f, 1.0f-step*4.0f, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			var tapBallsLabel = new BoxView() {
				BackgroundColor = Color.Transparent
			};
			bool added = false;
			this.popUpLabel.ExtraDraw = delegate () {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.popUpLabel.Draw(textureId, new float []{ left, right, bottom, top });
				this.DrawSkinColours(ballsSkin);
				if (!added) {
					added = true;
					CLReDraw.View(tapBallsLabel, -1.0f, 1.0f-step*4.0f, step*9.0f, step*2.0f);
				}
			};
			Action commonAction = delegate() {
				if (this.popUpLabel == null)
					return;
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.popUpLabel.Dispose();
				this.popUpLabel = null;
				this.activeField = true;
				CLReDraw.ReleaseView(tapBallsLabel);
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
					float []endRect = startRectExternal;
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
					this.ShowSkinColoursAnimation(ballsSkin, startRectExternal, false);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
					}), true);
				}
			};
			Action inAction = delegate() {
				if (this.popUpAnimating)
					return;
				if (this.popUpLabel == null)
					return;
				CLField.PlaySound("MenuNav.mp3");
				if (selectSkin) {
					this.popUpLabel.Text = App.Strings.BallsSkinSelected;
					this.settings.BallsSkin = ballsSkin;
					selectSkin = false;
					return;
				} else if (showSecondLocked) {
					this.popUpLabel.Text = App.Strings.CompleteAchievement + achievement.Description;
					showSecondLocked = false;
					return;
				}
				commonAction();
			};
			Action outAction = delegate() {
				if (this.popUpAnimating)
					return;
				if (this.popUpLabel == null)
					return;
				if (showSecondLocked) {
					this.popUpLabel.Text = App.Strings.CompleteAchievement + achievement.Description;
					showSecondLocked = false;
					return;
				}
				commonAction();
			};
//			this.popUpLabel.Action = inAction;
			this.popUpLabel.OutAction = outAction;
			var tap = new TapGestureRecognizer();
			tap.Tapped += (sender, ev) => {
				inAction();
			};
			tapBallsLabel.GestureRecognizers.Add(tap);
		}
		private void ShowSkinColoursAnimation(CLBallSkins ballsSkin, float []startRectExternal, bool appearing = true) {
			int cellTextureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLCell);
			for (CLColour i = CLColour.CLRed; i < CLColour.CLMax; i++) {
				float xoffset = ((int)i-(int)CLColour.CLRed)*step;
				float []startRect = appearing ? startRectExternal : new float []{ -1.0f+step*1.0f+xoffset, 1.0f-step*5.0f, step, step };
				float []endRect = !appearing ? startRectExternal : new float []{ -1.0f+step*1.0f+xoffset, 1.0f-step*5.0f, step, step };
				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, i, ballsSkin);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					return this.AnimPopUp(start, end, checkTime, startRect, endRect, cellTextureId, appearing: appearing);
				}), true);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: appearing);
				}), true);
			}
		}
		private void DrawSkinColours(CLBallSkins ballsSkin) {
			int cellTextureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLCell);
			for (CLColour i = CLColour.CLRed; i < CLColour.CLMax; i++) {
				float xoffset = ((int)i-(int)CLColour.CLRed)*step;
				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, i, ballsSkin);
				CLReDraw.Rect(-1.0f+step*1.0f+xoffset, 1.0f-step*5.0f, step, step, cellTextureId);
				CLReDraw.Rect(-1.0f+step*1.0f+xoffset, 1.0f-step*5.0f, step, step, textureId);
			}
		}

		private void ShowBackgroundSelection(CLBackgrounds background, float[] startRectExternal) {
			if (this.popUpAnimating)
				return;
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			if (this.settings.Animations) {
				this.activeField = false;
				this.popUpAnimating = true;
				float []startRect = startRectExternal;
				float []endRect = { -1.0f+step*3.0f, 1.0f-step*5.0f, step*3.0f, step*3.0f };
				CLBackgroundTextures bg = CLField.GetBgTexture(background, true);
				int textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, bg);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
				}), true);
				float []endRect2 = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
				int textureId2 = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					bool ret = this.AnimPopUp(start, end, checkTime, startRect, endRect2, textureId2);
					if (ret) {
						//we need this to add the label with text but continue animations
						this.popUpLabel.Draw(this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong), new float[] { left, right, bottom, top });
					}
					return ret;
				}), true);
			}
			bool defaultBg = background == CLBackgrounds.CLDefault;
			bool achieved = false;
			CLAchievement achievement = null;
			switch (background) {
			case CLBackgrounds.CLStars:
				achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLScore500);
				achieved = achievement.Achieved;
				break;
			case CLBackgrounds.CLNebula:
				achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLScore1000);
				achieved = achievement.Achieved;
				break;
			}
			bool unlocked = !defaultBg && achieved;
			bool selectBg = false, showSecondLocked = false;
			string text = "";
			if (background == this.settings.Background) {
				text = App.Strings.BackgroundSelected;
			} else {
				if (defaultBg || unlocked) {
					selectBg = true;
					text = App.Strings.SelectBackground;
				} else if (!unlocked) {
					showSecondLocked = true;
					text = App.Strings.RewardLocked;
				}
			}
			this.popUpLabel = new CLLabel(text, -1.0f, 1.0f-step*4.0f, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			var tapBgLabel = new BoxView() {
				BackgroundColor = Color.Transparent
			};
			bool added = false;
			this.popUpLabel.ExtraDraw = delegate () {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.popUpLabel.Draw(textureId, new float []{ left, right, bottom, top });
				CLBackgroundTextures bg = CLField.GetBgTexture(background, true);
				textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, bg);
				CLReDraw.Rect(-1.0f+step*3.0f, 1.0f-step*5.0f, step*3.0f, step*3.0f, textureId);
				if (!added) {
					added = true;
					CLReDraw.View(tapBgLabel, -1.0f+step*3.0f, 1.0f-step*4.0f, step*3.0f, step*4.0f);
				}
			};
			Action commonAction = delegate() {
				if (this.popUpLabel == null)
					return;
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.popUpLabel.Dispose();
				this.popUpLabel = null;
				this.activeField = true;
				CLReDraw.ReleaseView(tapBgLabel);
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { -1.0f+step*3.0f, 1.0f-step*5.0f, step*3.0f, step*3.0f };
					float []endRect = startRectExternal;
					CLBackgroundTextures bg = CLField.GetBgTexture(background, true);
					int textureId = this.GetTextureId(CLTextureTypes.CLBackgrounds, bg);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
					}), true);
					float []startRect2 = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
					int textureId2 = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect2, endRect, textureId2, appearing: false);
					}), true);
				}
			};
			Action inAction = delegate() {
				if (this.popUpAnimating)
					return;
				if (this.popUpLabel == null)
					return;
				CLField.PlaySound("MenuNav.mp3");
				if (selectBg) {
					this.popUpLabel.Text = App.Strings.BackgroundSelected;
					this.settings.Background = background;
					selectBg = false;
					return;
				} else if (showSecondLocked) {
					this.popUpLabel.Text = App.Strings.CompleteAchievement + achievement.Description;
					showSecondLocked = false;
					return;
				}
				commonAction();
			};
			Action outAction = delegate() {
				if (this.popUpAnimating)
					return;
				if (this.popUpLabel == null)
					return;
				if (showSecondLocked) {
					this.popUpLabel.Text = App.Strings.CompleteAchievement + achievement.Description;
					showSecondLocked = false;
					return;
				}
				commonAction();
			};
//			this.popUpLabel.Action = inAction;
			this.popUpLabel.OutAction = outAction;
			var tap = new TapGestureRecognizer();
			tap.Tapped += (sender, ev) => {
				inAction();
			};
			tapBgLabel.GestureRecognizers.Add(tap);
		}

		private bool CheckBlowAchievements(int ballsCount) {
			bool saveAchievements = false;
			if (ballsCount >= CLAchievement.Blow10) {
				CLAchievement achievement;
				if (ballsCount >= CLAchievement.Blow13) {
					saveAchievements = this.CheckBlowAchievements(CLAchievement.Blow10);
					achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLBlow13);
				} else {
					achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLBlow10);
				}
				if (!achievement.Achieved) {
					achievement.Achieved = true;
					this.activeField = false;
					if (!this.achievementAnimating) {
						this.achievementAnimating = true;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimAchievementUnlockedDuration, (start, end, checkTime) => {
							return this.AnimAchievementUnlocked(start, end, checkTime, achievement);
						}));
					} else {
						this.pendingAnimations.Add(new CLAnim(CLField.AnimAchievementUnlockedDuration, (start, end, checkTime) => {
							return this.AnimAchievementUnlocked(start, end, checkTime, achievement);
						}));
					}
					return true;
				}
			}
			return saveAchievements;
		}
		private bool CheckScoreAchievements(int score) {
			if (score >= CLAchievement.Score500) {
				CLAchievement achievement;
				if (score >= CLAchievement.Score1000) {
					achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLScore1000);
				} else {
					achievement = CLAchievement.GetAchievement(this.achievements, CLAchievements.CLScore500);
				}
				if (!achievement.Achieved) {
					achievement.Achieved = true;
					this.activeField = false;
					if (!this.achievementAnimating) {
						this.achievementAnimating = true;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimAchievementUnlockedDuration, (start, end, checkTime) => {
							return this.AnimAchievementUnlocked(start, end, checkTime, achievement);
						}));
					} else {
						this.pendingAnimations.Add(new CLAnim(CLField.AnimAchievementUnlockedDuration, (start, end, checkTime) => {
							return this.AnimAchievementUnlocked(start, end, checkTime, achievement);
						}));
					}
					return true;
				}
			}
			return false;
		}
		bool firstAchievement = true;
		private void CheckAchievements(int score, int ballsCount) {
/*			if (this.firstAchievement) {
				foreach (var achievement in this.achievements) {
					if (achievement.Achieved) {
						this.firstAchievement = false;
						break;
					}
				}
			}*/
			bool saveAchievements = this.CheckBlowAchievements(ballsCount)
								  | this.CheckScoreAchievements(score);
			if (saveAchievements)
				this.settings.Achievements = CLAchievement.ToSettings(this.achievements);
		}

		private string GetBestScoresList() {
			const int positionWidth = 19;
			string list = "";
			for (int i = 0; i < CLField.BestScoresCount; i++) {
				if (i > 0) {
					list += "\n\n";
				}
				int dots = positionWidth - this.scoresTable[i,0].Length - this.scoresTable[i,1].Length - 4;
				list += string.Format("{0,2}", (i+1).ToString()) + ". " + this.scoresTable[i,0];
				for (int j = 0; j < dots; j++)
					list += ".";
				list += this.scoresTable[i,1];
			}
			return list;
		}
		
		private void AddBalls(bool restart = false) {
			this.TutorialApply(this.selectTutorial);
			CLCell []tutBalls = CLTutorial.GetAddBalls();

			this.extraScore = !this.settings.NextColours;

			Random r = new Random(DateTime.UtcNow.Millisecond);
			CLCell cell = this.cells;

			int count = this.rows*this.columns - this.ballsCount;
			if (restart)
				count = 5;
			else if (count > 3)
				count = 3;
			if (this.teaching && tutBalls != null)
				count = tutBalls.Length;

			for (int i = 0, n = count; i < n; i++, cell = this.cells) {
				if (!this.teaching) {
					int column = r.Next(this.columns),
						row = r.Next(this.rows);
					for (int k = 0; k < row; k++)
						cell = cell.Bottom;
					for (int l = 0; l < column; l++)
						cell = cell.Right;
					//if the cell already has a ball then seek for the next empty one
					//or if the cell is in a blowing queue
					if (cell.Colour != CLColour.CLNone || cell.Blowing) {
						i--;
						continue;
					}
				} else {
					if (tutBalls == null)
						break;
					int column = tutBalls[i].Column,
						row = tutBalls[i].Row;
					for (int k = 0; k < row; k++)
						cell = cell.Bottom;
					for (int l = 0; l < column; l++)
						cell = cell.Right;
				}
				if (!(restart && i > 2)) {
					if (!this.teaching)
						cell.Colour = this.nextColours[i];
					else
						cell.Colour = tutBalls[i].Colour;
					if (this.settings.Animations && !(this.teaching && this.greetings)) {
						cell.AnimTime = this.time;
						cell.Appearing = true;
						CLCell c = cell;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimAppearingDuration, (start, end, checkTime) => {
							return this.AnimAppearing(start, end, checkTime, c);
						}));
					}
					this.nextColours[i] = (CLColour)r.Next((int)CLColour.CLRed, (int)CLColour.CLMax);
				} else {
					if (!this.teaching)
						cell.Colour = (CLColour)r.Next((int)CLColour.CLRed, (int)CLColour.CLMax);
					else
						cell.Colour = tutBalls[i].Colour;
					if (this.settings.Animations && !(this.teaching && this.greetings)) {
						cell.AnimTime = this.time;
						cell.Appearing = true;
						CLCell c = cell;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimAppearingDuration, (start, end, checkTime) => {
							return this.AnimAppearing(start, end, checkTime, c);
						}));
					}
				}
				if (!restart && this.BlowBalls(cell) && n < 3)
					n = 3;
			}
			if (restart)
				this.settings.SavedGame = "";
			int currentBallsCount = 0;
			this.ForAllCells((c,i,j) => {
				if (c.Colour != CLColour.CLNone)
					currentBallsCount++;
			});
			this.ballsCount = currentBallsCount;
			int nextCount = this.rows*this.columns - this.ballsCount;
			if (nextCount == 1) {
				this.nextColours[1] = this.nextColours[2] = CLColour.CLNone;
			} else if (nextCount == 2) {
				this.nextColours[2] = CLColour.CLNone;
			} else if (nextCount <= 0) {
				if (!this.CheckNewScore()) {
					this.ShowGameOver();
				} else {
					this.RequestUserName();
				}
				return;
			}
			if (this.settings.Animations || this.teaching) {
				this.activeField = false;
			}
		}

		protected enum CLDirection {
			CLLeft,
			CLRight,
			CLDown,
			CLUp,
			CLLeftDown,
			CLRightDown,
			CLLeftUp,
			CLRightUp,
		}
		private void BlowByDirection(CLCell newCell, CLCell cell, CLDirection dir) {
			CLCell c = cell;
			CLColour tempColour = newCell.Colour;
			if (dir == CLDirection.CLLeft) {
				while (c.Left != null && c.Colour == c.Left.Colour) {
					c.Colour = CLColour.CLNone;
					c.Blowing = true;
					c = c.Left;
				}
			} else if (dir == CLDirection.CLDown) {
				while (c.Bottom != null && c.Colour == c.Bottom.Colour) {
					c.Colour = CLColour.CLNone;
					c.Blowing = true;
					c = c.Bottom;
				}
			} else if (dir == CLDirection.CLLeftDown) {
				while (c.Bottom != null && c.Left != null && c.Colour == c.Bottom.Left.Colour) {
					c.Colour = CLColour.CLNone;
					c.Blowing = true;
					c = c.Bottom.Left;
				}
			} else if (dir == CLDirection.CLRightDown) {
				while (c.Bottom != null && c.Right != null && c.Colour == c.Bottom.Right.Colour) {
					c.Colour = CLColour.CLNone;
					c.Blowing = true;
					c = c.Bottom.Right;
				}
			}
			c.Colour = CLColour.CLNone;
			c.Blowing = true;
			newCell.Colour = tempColour;
		}
		private bool BlowBalls(CLCell cell) {
			bool willBlow = false;
			int count = 1, countTotal = 0;
			
			//just to be sure.....
			if (cell.Colour == CLColour.CLNone)
				return false;

			/* LEFT-RIGHT (horizontal) balls sequence */
			CLCell c = cell;
			while (c.Left != null && c.Colour == c.Left.Colour) {
				count++;
				c = c.Left;
			}
			c = cell;
			while (c.Right != null && c.Colour == c.Right.Colour) {
				count++;
				c = c.Right;
			}
			if (count >= 5) {
				countTotal = count;
				willBlow = true;
				this.BlowByDirection(cell, c, CLDirection.CLLeft);
			}
			
			/* BOTTOM-TOP (vertical) balls sequence */
			count = 1;
			c = cell;
			while (c.Bottom != null && c.Colour == c.Bottom.Colour) {
				count++;
				c = c.Bottom;
			}
			c = cell;
			while (c.Top != null && c.Colour == c.Top.Colour) {
				count++;
				c = c.Top;
			}
			if (count >= 5) {
				if (willBlow)
					countTotal--;
				countTotal += count;
				willBlow = true;
				this.BlowByDirection(cell, c, CLDirection.CLDown);
			}
			
			/* BOTTOM|LEFT-TOP|RIGHT (increasing diagonal) balls sequence */
			count = 1;
			c = cell;
			while (c.Bottom != null && c.Left != null && c.Colour == c.Bottom.Left.Colour) {
				count++;
				c = c.Bottom.Left;
			}
			c = cell;
			while (c.Top != null && c.Right != null && c.Colour == c.Top.Right.Colour) {
				count++;
				c = c.Top.Right;
			}
			if (count >= 5) {
				if (willBlow)
					countTotal--;
				countTotal += count;
				willBlow = true;
				this.BlowByDirection(cell, c, CLDirection.CLLeftDown);
			}
			
			/* BOTTOM|RIGHT-TOP|LEFT (decreasing diagonal) balls sequence */
			count = 1;
			c = cell;
			while (c.Bottom != null && c.Right != null && c.Colour == c.Bottom.Right.Colour) {
				count++;
				c = c.Bottom.Right;
			}
			c = cell;
			while (c.Top != null && c.Left != null && c.Colour == c.Top.Left.Colour) {
				count++;
				c = c.Top.Left;
			}
			if (count >= 5) {
				if (willBlow)
					countTotal--;
				countTotal += count;
				willBlow = true;
				this.BlowByDirection(cell, c, CLDirection.CLRightDown);
			}

			if (willBlow) {
				this.ballsCount -= countTotal;
				this.score += AddScore(countTotal);
				this.score += AddExtraScore();
				var colour = cell.Colour;
				cell.Colour = CLColour.CLNone;
				cell.Blowing = true;
				if (this.settings.Animations || this.teaching) {
					this.activeField = false;
					bool firstCall = true;
					CLAnim.AddToQueue(new CLAnim(CLField.AnimBlowingDuration, (start, end, checkTime) => {
						if (firstCall) {
							firstCall = false;
							CLField.PlaySound("Blow.mp3");
						}
						bool ret = this.AnimBlowing(start, end, checkTime, colour);
						if (ret) {
							this.userScoreLabel.Text = this.score.ToString();
							this.CheckAchievements(this.score, countTotal);
						}
						return ret;
					}));
				} else {
					this.userScoreLabel.Text = this.score.ToString();
					this.CheckAchievements(this.score, countTotal);
				}
				this.SaveGame();
			}

			return willBlow;
		}
		public bool SelectBall(CLCell cell) {
			if (cell == null)
				return false;
			if (this.teaching && !this.selectTutorial.Done && !cell.Highlight)
				return false;
			if (this.teaching && !this.selectTutorial.Done && this.selected != null) {
				return false;
			}
			if (cell.Colour == CLColour.CLNone) {
				if (this.selected != null && this.settings.ConfirmMove) {
					if (cell.Selected) {
						cell.Selected = false;
						return false;
					}
					this.ForAllCells((c,i,j) => {
						if (c != this.selected)
							c.Selected = false;
					});
					cell.Selected = true;
					return true;
				}
				return false;
			}
			if (this.selected == cell) {
				if (this.teaching && !this.moveTutorial.Done)
					return false;
				this.ForAllCells((c, i, j) => {
					c.Selected = false;
				});
				this.selected = null;
				return true;
			}
			if (this.teaching
				&& this.selectTutorial.Done
				&& this.moveTutorial.Done
				&& ((!this.blowTutorial.Done && !this.blockedTutorial.Applied)
				|| (this.blowTutorial.Done && this.blockedTutorial.Applied && !this.blockedTutorial.Done)
				|| (!this.blowTutorial.Done && this.blockedTutorial.Applied && this.blockedTutorial.Done))
				&& !cell.Highlight)
				return false;
			if (this.selected != null) {
				if (this.teaching && !this.moveTutorial.Done)
					return false;
//				this.selected.Selected = false;
			}
			this.ForAllCells((c,i,j) => {
				c.Selected = false;
			});
			this.selected = cell;
			this.selected.Selected = true;
			this.TutorialApply(this.moveTutorial);
			return true;
		}

		public bool IsReachable(CLCell from, CLCell to) {
			//the same point, wtf? should never happen
			if (from == to)
				return false;
			bool[,] map = new bool[this.columns, this.rows];
			this.ForAllCells((c,i,j) => {
				if (c.Colour == CLColour.CLNone)
					map[j,i] = true;
				else
					map[j,i] = false;
			});
			SearchParameters searchParameters = new SearchParameters(new CLPoint(from.Column, from.Row), new CLPoint(to.Column, to.Row), map);
			PathFinder pathFinder = new PathFinder(searchParameters);
			var path = pathFinder.FindPath();
			if (path.Count > 0) {
				if (this.settings.Animations || this.teaching) {
					var pathActual = new List<CLPoint>() {
						new CLPoint(from.Column, from.Row)
					};
					pathActual.AddRange(path);
					CLColour colour = from.Colour;
					to.Moving = true;
					long duration = this.settings.Route ? pathActual.Count*CLField.AnimMovingDurationCoeff : CLField.AnimNonRouteDuration;
					long startMovingSound = duration-CLField.StartMovingSound;
					bool playedSound = false;
					CLAnim.AddToQueue(new CLAnim(duration, (start, end, checkTime) => {
						if (this.settings.Route && !playedSound && (this.time-start) > startMovingSound) {
							playedSound = true;
							CLField.PlaySound("Move.mp3");
						}
						return this.AnimMoving(start, end, checkTime, colour, pathActual);
					}));
					if (!this.settings.Route)
						CLField.PlaySound("Move.mp3");
				}
				return true;
			}
			return false;
		}
		private bool MoveBall(CLCell to) {
			if (this.selected == null)
				return false;
			if (to == null)
				return false;
			if (this.teaching && !this.moveTutorial.Done && !to.Highlight)
				return false;
			if (this.teaching
				&& this.moveTutorial.Done
				&& ((!this.blowTutorial.Done && !this.blockedTutorial.Applied)
				|| (this.blowTutorial.Done && this.blockedTutorial.Applied && !this.blockedTutorial.Done)
				|| (!this.blowTutorial.Done && this.blockedTutorial.Applied && this.blockedTutorial.Done))
				&& !to.Highlight)
				return false;
			CLCell from = this.selected;
			if (!IsReachable(from, to)) {
				if (this.teaching) {
					this.TutorialApply(this.blockedTutorial);
				}
				CLField.PlaySound("Blocked.mp3");
				return false;
			}
			if (this.teaching && this.blockedTutorial.Applied && !this.blockedTutorial.Done) {
				if (from.HighlightBlocked) {
					this.ForAllCells((c,i,j) => {
						if (c.HighlightBlocked) {
							c.Highlight = false;
							c.HighlightBlocked = false;
							c.AnimTime = 0;
						}
					});
					this.TutorialApply(this.blowTutorial, true);
				}
			}
			if (this.teaching
				&& this.moveTutorial.Done
				&& ((!this.blowTutorial.Done && !this.blockedTutorial.Applied)
				|| (this.blowTutorial.Done && this.blockedTutorial.Applied && !this.blockedTutorial.Done)
				|| (!this.blowTutorial.Done && this.blockedTutorial.Applied && this.blockedTutorial.Done))) {
				to.Highlight = true;
				if (from.AnimTime != 0)
					to.AnimTime = from.AnimTime;
				if (!from.HighlightForced)
					from.Highlight = false;
			}
			to.Colour = from.Colour;
			from.Colour = CLColour.CLNone;
			this.ForAllCells((c, i, j) => {
				c.Selected = false;
			});
			this.selected = null;
			if (!this.BlowBalls(to))
				this.AddBalls();
			return true;
		}

		private bool CheckNewScore() {
			int i = CLField.BestScoresCount - 1;
			for (;i >= 0 && int.Parse(this.scoresTable[i,1]) < this.score; i--);
			if (i > CLField.BestScoresCount-2)
				return false;
			return true;
		}
		private void SaveUserScore() {
			int i = CLField.BestScoresCount - 1;
			for (;i >= 0 && int.Parse(this.scoresTable[i,1]) < this.score; i--);
			for (int j = CLField.BestScoresCount-1; j > i+1; j--) {
				this.scoresTable[j,0] = this.scoresTable[j-1,0];
				this.scoresTable[j,1] = this.scoresTable[j-1,1];
			}
			this.scoresTable[i+1,0] = this.entry.Text.Trim();
			this.scoresTable[i+1,1] = this.score.ToString();
			string scores = "";
			for (i = 0; i < CLField.BestScoresCount; i++)
				scores += this.scoresTable[i,0] + "\n" + this.scoresTable[i,1] + "\n";
			Settings.Scores = scores;
		}
		private bool firstEntry = true;
		private void RequestUserName() {
			CLField.PlaySound("NewRecord.mp3");
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			var bounds = new float[] { this.left, this.right, this.bottom, this.top };
			this.popUpLabel = new CLLabel(App.Strings.NewRecord, -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, step, CLField.GreenColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
				this.popUpLabel.Draw(textureId, bounds);
			};
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.activeLabels = false;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
				}
				if (this.popUpLabel == null)
					this.popUpLabel = new CLLabel(App.Strings.Name, -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, step, CLField.GreenColor, TextAlignment.Start);
				bool entryRemoved = false;
				Action action = delegate() {
					this.activeLabels = false;
					if (this.popUpAnimating)
						return;
					if (!string.IsNullOrWhiteSpace(this.entry.Text)
					&& !string.IsNullOrEmpty(this.entry.Text)) {
						entryRemoved = true;
						this.SaveUserScore();
						this.settings.SavedGame = "";
//						CLReDraw.ReleaseView(this.entry);
						this.resultsLabel.Action();
						this.entry.Text = "";
						this.entry.IsVisible = false;
						this.entry.IsEnabled = false;
//						this.entry = null;
						Task.Run(async () => {
							await Task.Delay(500);
							CLField.HideShowKeyboard(true);
						});
						this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
							this.activeLabels = true;
							if (this.popUpAnimating)
								return;
							this.popUpLabel.Action = this.popUpLabel.OutAction = null;
							//TODO: add fancy adding new score animation
							if (this.settings.Animations) {
								this.activeField = false;
								this.popUpAnimating = true;
								float []startRect = this.popUpLabel.GetCoordinates();
								float []endRect = { -1.0f+step*2, 1.0f-step*2, step*5, step*5 };
								int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLarge);
								CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
									return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
								}));
							}
							this.activeField = true;
							this.ClearField();
							this.popUpLabel.Dispose();
							this.popUpLabel = null;
						};
					}
				};
				if (this.entry != null && this.entry.IsVisible)
					return;
				this.entry.IsVisible = true;
				this.entry.IsEnabled = true;
//				this.popUpLabel.Action = this.popUpLabel.OutAction = action;
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
					if (Device.Idiom == TargetIdiom.Desktop)
						this.popUpLabel.Draw(textureId, bounds);
					else
						this.popUpLabel.Draw(textureId/*, bounds*/);
//					CLReDraw.View(this.entry, -1.0f+step*2.0f+xoffset, 1.0f-step*4, step*5, step);
				};
				var task = new Task(async () => {
					while (true) {
						await Task.Delay(2000);
						if (entryRemoved || this.entry == null || !this.entry.IsVisible) {
							CLField.HideShowKeyboard(true);
							break;
						}
						Device.BeginInvokeOnMainThread(() => {
							if (this.entry != null
/*							&& (string.IsNullOrWhiteSpace(this.entry.Text)
							|| string.IsNullOrEmpty(this.entry.Text))*/
							&& !this.entry.IsFocused && !entryRemoved) {
								//the entry mays get nulled while invoking
								if (this.entry != null && this.entry.IsVisible && !entryRemoved) {
									this.entry.Focus();
								} else {
									CLField.HideShowKeyboard(true);
								}
							}
						});
					}
				});
				task.Start();
				if (!firstEntry) {
					return;
				}
				firstEntry = false;
				float xoffset = step * 0.32f * App.Strings.Name.Length;
/*				this.entry = new CLFormsEntry() {
					TextColor = CLField.GreenColor,
					Keyboard = Keyboard.Text,
					LettersLimit = 11
				};*/
//				this.entry.FontSize *= 1.0;// / 1.6f
				this.entry.Completed += (sender, ev) => {
					action();
				};
/*				this.entry.Unfocused += (sender, ev) => {
					if (!(task.Status == TaskStatus.WaitingToRun
						|| task.Status == TaskStatus.Running
						|| task.Status == TaskStatus.RanToCompletion)) {
						task.Start();
					}
				};*/
			};
		}
		private void ShowGameOver() {
			CLField.PlaySound("GameOver.mp3");
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			var bounds = new float[] { this.left, this.right, this.bottom, this.top };
			this.popUpLabel = new CLLabel(App.Strings.GameOver, -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, step, CLField.RedColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
				this.popUpLabel.Draw(textureId, bounds);
			};
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.activeLabels = true;
				if (this.popUpAnimating)
					return;
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { -1.0f+step*2.0f, 1.0f-step*4.0f, step*5.0f, step };
					float []endRect = { -1.0f+step*3.0f, 1.0f-step*4.0f-step*c1d5, step*3.0f, step*3.0f*c1d5 };
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLMedium);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
					}));
				}
				this.activeField = true;
				this.ClearField();
				this.popUpLabel.Dispose();
				this.popUpLabel = null;
			};
		}
		private void ClearField(bool restart = true) {
			for (CLCell cb = this.cells; cb != null; cb = cb.Bottom) {
				for (CLCell cr = cb; cr != null; cr = cr.Right) {
					cr.Colour = CLColour.CLNone;
					cr.Appearing = cr.Blowing = cr.BlowingStarted = cr.Moving = false;
				}
			}
			if (this.settings.Animations) {
				this.popUpAnimating = true;
				CLAnim.AddToTop(new CLAnim(CLField.AnimClearingDuration, (start, end, checkTime) => {
					return this.AnimFieldClearing(start, end, checkTime);
				}, true), true);
			}
			if (this.selected != null) {
				this.ForAllCells((c, i, j) => {
					c.Selected = false;
				});
				this.selected = null;
			}
			if (restart) {
				this.ballsCount = 0;
				Random r = new Random(DateTime.Now.Millisecond);
				for (uint i = 0; i < 3; i++) {
					this.nextColours[i] = (CLColour)r.Next((int)CLColour.CLRed, (int)CLColour.CLMax);
				}
				if (!this.settings.Animations) {
					this.AddBalls(true);
				}
			}
			this.score = 0;
			this.userScoreLabel.Text = this.score.ToString();
			this.bestScoreLabel.Text = this.scoresTable[0,1];
		}
		
		private void ForAllCells(Action<CLCell,int,int> action) {
			CLCell cell = this.cells;
			for (int i = 0; i < this.rows; i++, cell = cell.Bottom) {
				for (int j = 0; j < this.columns && cell != null; j++, cell = cell.Right) {
					action(cell, i, j);
				}
				cell = this.cells;
				for (int k = 0; k < i; k++, cell = cell.Bottom);
				if (cell == null)
					break;
			}
		}

		private void ShowMessage(string message) {
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			this.popUpLabel = new CLLabel(message, -1.0f, 1.0f-step*4.0f, step*9.0f, step, CLField.GreenColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate() {
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				this.popUpLabel.Draw(textureId, new float[] { left, right, bottom, top });
			};
			if (this.settings.Animations) {
				this.activeField = false;
				this.popUpAnimating = true;
				float []startRect = { -1.0f+step*1.0f, 1.0f-step*4.0f-step*c1d9, step*7.0f, step*7.0f*c1d9 };
				float []endRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
				int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
				CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
					return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
				}));
			}
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.activeField = true;
				this.popUpLabel.Dispose();
				this.popUpLabel = null;
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { -1.0f, 1.0f-step*4.0f, step*9.0f, step };
					float []endRect = { -1.0f+step*1.0f, 1.0f-step*4.0f-step*c1d9, step*7.0f, step*7.0f*c1d9 };
					int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
					}));
				}
			};
		}

		private bool AnimBlowing(long start, long end, bool checkTime, CLColour colour) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.TutorialApply(null);
				this.ForAllCells((c,i,j) => {
					c.Blowing = false;
					c.BlowingStarted = false;
				});
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = CLAnim.EaseOutCubic(dt, dr);
			this.ForAllCells((c,i,j) => {
				if (!c.Blowing)
					return;
				c.BlowingStarted = true;
				float x = j*step-1.0f, y = 1.0f-i*step, width = step, height = step;
				float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

				dx = width * -0.23f * 0.5f;
				dy = height * 0.23f * 0.5f;
				dwidth = width * 0.23f;
				dheight = height * 0.23f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				Color fill;
/*				if (dt > dr * 0.5f) {
					dt = (long)(dt - (dr * 0.5f));
					dr = (long)(dr * 0.5f);
					danim = 1.0f-CLAnim.Linear(dt, dr);*/
					fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
/*				} else {
					fill = CLReDraw.WhiteColor;
				}*/
				
				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, colour);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId, fill);
			});
			return false;
		}
		private bool AnimAppearing(long start, long end, bool checkTime, CLCell cell) {
			long dt = this.time-start,
				 dr = end-start;
/*			if (this.time + 20 > end) {
				this.ForAllCells((c, i, j) => {
					c.appearing = false;
				});
			}*/
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.ForAllCells((c,i,j) => {
					if (c == cell)
						c.Appearing = false;
				});
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = 1.0f-CLAnim.EaseOutCubic(dt, dr);
			this.ForAllCells((c,i,j) => {
				//wrong colour may happen on appearing then auto-blowing
				if (!c.Appearing || c != cell || (c.Colour == CLColour.CLNone && !c.Blowing))
					return;
				var colour = !c.Blowing ? c.Colour : c.OldColour;
				float x = j*step-1.0f, y = 1.0f-i*step, width = step, height = step;
				float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

				dx = width * 0.7f * 0.5f;
				dy = height * -0.7f * 0.5f;
				dwidth = width * -0.7f;
				dheight = height * -0.7f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				Color fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);

				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, colour);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId, fill);
			});
			return false;
		}
		private bool AnimMoving(long start, long end, bool checkTime, CLColour colour, List<CLPoint> path) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.TutorialApply(this.blowTutorial);
				this.ForAllCells((c,g,h) => {
					c.Moving = false;
				});
				return true;
			}
			if (checkTime) {
				return false;
			}
			if (this.settings.Route) {
				int count = path.Count-1;
				float dc = 1.0f / count;
				float da = CLAnim.EaseInOutQuad(dt, dr);
				float dk = da / dc;

				int k = (int)dk;
				k = (k == count) ? (k-1) : k;
				int j = path[k].X, i = path[k].Y,
					j2 = path[k+1].X, i2 = path[k+1].Y;
				float danim = (da - (dc * k)) / dc;

				float x = j*step-1.0f, y = 1.0f-i*step, width = step, height = step,
					  x2 = j2*step-1.0f, y2 = 1.0f-i2*step;
				float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

				dx = x2-x;
				dy = y2-y;
				dwidth = 0.0f;
				dheight = 0.0f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, colour);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);
			} else {
				float danim = CLAnim.Linear(dt, dr);
				int j, i;
				if (danim < 0.5f) {
					danim = CLAnim.EaseOutCubic(dt, dr*0.5f);
					j = path[0].X;
					i = path[0].Y;
				} else {
					danim = 1.0f-CLAnim.EaseOutCubic(dt-dr*0.5f, dr*0.5f);
					j = path[path.Count-1].X;
					i = path[path.Count-1].Y;
				}

				float x = j*step-1.0f, y = 1.0f-i*step, width = step, height = step;
				float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

				dx = width * 0.7f * 0.5f;
				dy = height * -0.7f * 0.5f;
				dwidth = width * -0.7f;
				dheight = height * -0.7f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, colour);
				Color fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId, fill);
			}
			return false;
		}
		private bool AnimFieldClearing(long start, long end, bool checkTime) {
			long dt = this.time-start,
				 dr = end-start;
/*			if (this.time + 20 > end) {
				this.ForAllCells((c, i, j) => {
					c.appearing = false;
				});
			}*/
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.popUpAnimating = false;
				this.AddBalls(true);
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = CLAnim.EaseOutCubic(dt, dr);
			this.ForAllCells((c,i,j) => {
				if (c.OldColour == CLColour.CLNone && c.Colour == c.OldColour)
					return;
				//should never happen
				if (c.OldColour == CLColour.CLNone && c.Appearing)
					return;
				var colour = !c.Appearing ? c.OldColour : c.Colour;
				float x = j*step-1.0f, y = 1.0f-i*step, width = step, height = step;
				float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

				dx = width * 0.7f * 0.5f;
				dy = height * -0.7f * 0.5f;
				dwidth = width * -0.7f;
				dheight = height * -0.7f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				int textureId = this.GetTextureId(CLTextureTypes.CLBalls, colour);
				Color fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId, fill);
			});
			return false;
		}
		private bool AnimPopUp(long start, long end, bool checkTime, float []startRect, float []endRect, int textureId, int oldTextureId = -1, bool appearing = true) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.popUpAnimating = false;
				return true;
			}
			if (checkTime) {
				return false;
			}

			float danim = CLAnim.EaseOutQuad(dt, dr);

			float x = startRect[0], y = startRect[1], width = startRect[2], height = startRect[3];
			float dx = endRect[0]-startRect[0], dy = endRect[1]-startRect[1], dwidth = endRect[2]-startRect[2], dheight = endRect[3]-startRect[3];

			dx *= danim;
			dy *= danim;
			dwidth *= danim;
			dheight *= danim;

			Color fill;
			if (appearing) {
				fill = CLField.WhiteColor.MultiplyAlpha(danim);
			} else {
				fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
			}

			if (oldTextureId >= 0) {
				CLReDraw.Rect(x, y, width, height, oldTextureId);
			}
			CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId, fill);
			return false;
		}
		private bool AnimGreetings(long start, long end, bool checkTime) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.greetingsClosing = false;
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = CLAnim.EaseInOutQuad(dt, dr);

			Color fill = CLField.BlackColor.MultiplyAlpha(1.0f-danim);
			CLReDraw.Rect(left, 1.0f, step*9, step*9, fill);

			CLReDraw.Rect(left, bottom+step, step*9, step, CLField.BlackColor);

			float x = this.left, y = 1.0f-step*4, width = step*9, height = step;
			float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;

			dx = 0.0f;
			dy = (bottom+step)-y;
			dwidth = 0.0f;
			dheight = 0.0f;

			dx *= danim;
			dy *= danim;
			dwidth *= danim;
			dheight *= danim;

			int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
			CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);
			return false;
		}
		private bool AnimRevealField(long start, long end, bool checkTime) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.tutLineLabel.Text = "";
				this.tutLineLabel.Dispose();
				this.tutLineLabel = null;
				if (this.tutSkipLabel != null) {
					this.tutSkipLabel.Text = "";
					this.tutSkipLabel.Dispose();
					this.tutSkipLabel = null;
				}
				this.bestScoreLabel.TextColor =
				this.userScoreLabel.TextColor =
				this.resultsLabel.TextColor =
				this.startLabel.TextColor =
				this.menuLabel.TextColor = CLField.GreenColor;
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = CLAnim.Linear(dt, dr);

			Color fill = CLField.BlackColor.MultiplyAlpha(1.0f-danim);
			CLReDraw.Rect(left, top, step*9, step, fill);

			fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
			int textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong);
			CLReDraw.Rect(left, bottom+step, step*9, step, textureId, fill);
			if (this.tutSkipLabel != null) {
				textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLSmall);
				CLReDraw.Rect(left+step*3, top, step*3, step, textureId, fill);
				this.tutSkipLabel.TextColor = CLField.RedColor.MultiplyAlpha(1.0f-danim);
			}
			this.tutLineLabel.TextColor = CLField.GreenColor.MultiplyAlpha(1.0f-danim);
			this.bestScoreLabel.TextColor =
			this.userScoreLabel.TextColor =
			this.resultsLabel.TextColor =
			this.startLabel.TextColor =
			this.menuLabel.TextColor = CLField.GreenColor.MultiplyAlpha(danim);
			return false;
		}
		private bool startUnlocking = true;
		private bool AnimAchievementUnlocked(long start, long end, bool checkTime, CLAchievement achievement) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
				}
				this.popUpLabel = new CLLabel(App.Strings.Achievement + achievement.Description, -1.0f+step*1.0f, 1.0f-step*4.0f, step*8.0f, step, CLField.GreenColor, TextAlignment.Center);
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
					CLReDraw.Rect(-1.0f, 1.0f-step*4.0f, step, step, textureId);
					textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong8);
					this.popUpLabel.Draw(textureId, new float[] { left, right, bottom, top });
				};
				this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
					this.popUpLabel.Action = this.popUpLabel.OutAction = null;
					this.activeField = true;
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
					Action showFirstAchievement = delegate() {
						if (this.firstAchievement) {
							this.ShowMessage(App.Strings.FirstReward);
//							this.firstAchievement = false;
						}
					};
					bool pendingAnimations = this.pendingAnimations.Count > 0;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimAchievementUnlockedHideDuration, (start2, end2, checkTime2) => {
							bool ret = this.AnimAchievementUnlockedHide(start2, end2, checkTime2, achievement);
							if (ret && !pendingAnimations)
								showFirstAchievement();
							return ret;
						}));
					}
					if (pendingAnimations) {
						this.activeField = false;
						CLAnim.AddToQueue(this.pendingAnimations[0]);
						this.pendingAnimations.RemoveAt(0);
					} else {
						if (!this.settings.Animations)
							showFirstAchievement();
						this.achievementAnimating = false;
					}
				};
				this.startUnlocking = true;
				return true;
			}
			if (checkTime) {
				return false;
			}
			if (this.startUnlocking) {
				this.startUnlocking = false;
				CLField.PlaySound("Achievement.mp3");
			}

			const float firstLength = 0.23f, stayStill = 0.38f;
			const float splitFraction = firstLength+stayStill;
			if (dt >= dr * splitFraction) {
				dt = dt - (long)(dr * splitFraction);
				dr = (long)(dr * (1.0f-splitFraction));

				float danim = CLAnim.EaseOutQuad(dt, dr);

				float x = -1.0f+step*3.5f, y = 1.0f-step*3.5f, width = step*2.0f, height = step*2.0f;
				float dx = -1.0f-x, dy = 1.0f-step*4.0f-y, dwidth = step-width, dheight = step-height;
				float x2 = -1.0f+step*5.5f, y2 = 1.0f-step*3.5f, width2 = 0.0f, height2 = step*2.0f;
				float dx2 = -1.0f+step*1.0f-x2, dy2 = 1.0f-step*4.0f-y2, dwidth2 = step*8.0f-width2, dheight2 = step-height2;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;
				
				dx2 *= danim;
				dy2 *= danim;
				dwidth2 *= danim;
				dheight2 *= danim;

				int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);

				textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong8);
				CLReDraw.Rect(x2 + dx2, y2 + dy2, width2 + dwidth2, height2 + dheight2, textureId, new float[] {
					1.0f-danim, 1.0f,
					1.0f, 1.0f,
					1.0f-danim, 0.0f,
					1.0f, 0.0f,
				});
			} else if (dt < dr * firstLength) {
				dr = (long)(dr * firstLength);

				float danim = CLAnim.EaseOutQuad(dt, dr);

				float x = -1.0f+step*3.5f, y = bottom, width = step*2.0f, height = step*2.0f;
				float dx = 0.0f, dy = 1.0f-step*3.5f-y, dwidth = 0.0f, dheight = 0.0f;

				dx *= danim;
				dy *= danim;
				dwidth *= danim;
				dheight *= danim;

				int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);
			} else {
				float x = -1.0f+step*3.5f, y = 1.0f-step*3.5f, width = step*2.0f, height = step*2.0f;
				int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
				CLReDraw.Rect(x, y, width, height, textureId);
			}
			return false;
		}
		private bool AnimAchievementUnlockedHide(long start, long end, bool checkTime, CLAchievement achievement) {
			long dt = this.time-start,
				 dr = end-start;
			//start > this.time should never happen but let's be safe..
			if (this.time > end || start > this.time) {
				this.popUpAnimating = false;
				return true;
			}
			if (checkTime) {
				return false;
			}
			
			float danim = CLAnim.EaseOutQuad(dt, dr);

			float x = -1.0f, y = 1.0f-step*4.0f, width = step, height = step;
			float dx = 0.0f, dy = bottom-y, dwidth = 0.0f, dheight = 0.0f;
			float x2 = -1.0f+step*1.0f, y2 = 1.0f-step*4.0f, width2 = step*8.0f, height2 = step;
			float dx2 = 0.0f, dy2 = bottom-y2, dwidth2 = 0.0f, dheight2 = 0.0f;

			dx *= danim;
			dy *= danim;
			dwidth *= danim;
			dheight *= danim;
				
			dx2 *= danim;
			dy2 *= danim;
			dwidth2 *= danim;
			dheight2 *= danim;

			int textureId = this.GetTextureId(CLTextureTypes.CLAchievements, achievement.Id);
			CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);

			textureId = this.GetTextureId(CLTextureTypes.CLLabels, CLLabelSize.CLLong8);
			CLReDraw.Rect(x2 + dx2, y2 + dy2, width2 + dwidth2, height2 + dheight2, textureId);
			return false;
		}

		private void TutorialApply(CLTutorial tutorial, bool unDone = false) {
			if (this.teaching && this.tutLineLabel != null) {
				string text = CLTutorial.Apply(tutorial, unDone);
				if (text != null)
					this.tutLineLabel.Text = text;
				if (tutorial == null) {
					this.teaching = false;
					Device.BeginInvokeOnMainThread(() => {
						App.AudioManager.MusicEnabled = true;
					});
					this.teachingInit = true;
					this.settings.Taught = true;
					if (this.settings.Animations) {
						CLAnim.AddToQueue(new CLAnim(CLField.AnimRevealDuration, (start, end, checkTime) => {
							return this.AnimRevealField(start, end, checkTime);
						}));
					} else {
						this.tutLineLabel.Text = "";
						this.tutLineLabel.Dispose();
						this.tutLineLabel = null;
						if (this.tutSkipLabel != null) {
							this.tutSkipLabel.Text = "";
							this.tutSkipLabel.Dispose();
							this.tutSkipLabel = null;
						}
						this.bestScoreLabel.TextColor =
						this.userScoreLabel.TextColor =
						this.resultsLabel.TextColor =
						this.startLabel.TextColor =
						this.menuLabel.TextColor = CLField.GreenColor;
					}
					int count = 0;
					this.ForAllCells((c,i,j) => {
						c.Highlight = false;
						c.HighlightForced = false;
						c.HighlightBlocked = false;
						if (c.Colour != CLColour.CLNone) {
							count++;
						}
					});
					this.ballsCount = count;
				}
			}
		}

		private int GetTextureId(CLTextureTypes textureType, object texture, CLBallSkins? forcedSkin = null) {
			int i, j = (int)texture;
			if (textureType == CLTextureTypes.CLLabels) {
				i = (int)CLTextures.CLLabels;
			} else if (textureType == CLTextureTypes.CLAchievements) {
				i = (int)CLTextures.CLAchievements;
			} else if (textureType == CLTextureTypes.CLBackgrounds) {
				i = (int)CLTextures.CLBackgrounds;
			} else { //Balls
				if (forcedSkin == null) {
					i = (int)CLTextures.CLDefault + (int)this.settings.BallsSkin;
				} else {
					i = (int)CLTextures.CLDefault + (int)forcedSkin;
				}
				if ((CLColour)texture == CLColour.CLNone) {
					j = 0;
				} else {
					j--;
				}
			}
			return this.fieldTextures[i][j];
		}

		private static CLBackgroundTextures GetBgTexture(CLBackgrounds background, bool preview = false, bool small = false) {
			if (small) {
				switch (background) {
				default:
				case CLBackgrounds.CLDefault:
					return CLBackgroundTextures.CLDefaultSmall;
				case CLBackgrounds.CLStars:
					return CLBackgroundTextures.CLStarsSmall;
				case CLBackgrounds.CLNebula:
					return CLBackgroundTextures.CLNebulaSmall;
				}
			} else if (preview) {
				switch (background) {
				default:
				case CLBackgrounds.CLDefault:
					return CLBackgroundTextures.CLDefaultPreview;
				case CLBackgrounds.CLStars:
					return CLBackgroundTextures.CLStarsPreview;
				case CLBackgrounds.CLNebula:
					return CLBackgroundTextures.CLNebulaPreview;
				}
			}
			switch (background) {
			default:
			case CLBackgrounds.CLStars:
				return CLBackgroundTextures.CLStars;
			case CLBackgrounds.CLNebula:
				return CLBackgroundTextures.CLNebula;
			}
		}

		public static void PlaySound(string name) {
			Device.BeginInvokeOnMainThread(() => {
				App.AudioManager.PlaySound(name);
			});
		}

#if __ANDROID__
		private static void HideShowKeyboard(bool hide) {
			Device.BeginInvokeOnMainThread(() => {
				var view = ((Android.App.Activity)Forms.Context).CurrentFocus;
				if (view == null) {
					view = new Android.Views.View(Forms.Context);
				}
				var inputMethodManager = (Android.Views.InputMethods.InputMethodManager)Forms.Context.GetSystemService(Android.Content.Context.InputMethodService);
				if (hide)
					inputMethodManager.HideSoftInputFromWindow(view.WindowToken, 0);
				else
					inputMethodManager.ShowSoftInput(view, 0);
			});
		}
#else
		private static void HideShowKeyboard(bool hide) {}
#endif
		public bool OnBackButtonPressed() {
			if (this.popUpLabel != null && this.popUpLabel.OutAction != null) {
				CLField.PlaySound("MenuNav.mp3");
				this.popUpLabel.OutAction();
				return true;
			}
			if (this.menu == null || !this.menu.Show)
				return false;
			this.menu.Pop();
			return true;
		}

		private long lastKeyTime = 0L;
		private void KeyNavigation(CLKey key, bool isCtrlPressed) {
			if ((this.time - this.lastKeyTime) < 25) {
				return;
			}
			this.lastKeyTime = this.time;
			CLCell ks = null, ksNew = null;
			this.ForAllCells((c,i,j) => {
				if (c.KeySelected)
					ks = c;
			});
			if (ks == null) {
				ks = this.cells;
				int odd = this.rows & 1;
				int center = (this.rows >> 1) + odd;
				while (center > 1) {
					ks = ks.Bottom;
					center--;
				}
				odd = this.columns & 1;
				center = (this.columns >> 1) + odd;
				while (center > 1) {
					ks = ks.Right;
					center--;
				}
				ks.KeySelected = true;
				this.selectableNavigation = true;
				keySelectionFlashTime = this.time + CLField.KeyNavigationFlashDelay;
				if (!((key == CLKey.CLEnter || key == CLKey.CLEscape)
					&& (this.popUpLabel?.Action != null
					|| this.popUpLabel?.OutAction != null)))
					return;
			}
			switch (key) {
			default:
			case CLKey.CLNone:
				return;
			case CLKey.CLLeft:
				if (ks.Left != null) {
					if (isCtrlPressed) {
						CLCell findNew = ks.Left;
						while (findNew != null && findNew.Left != null) {
							if (findNew.Colour != CLColour.CLNone)
								break;
							findNew = findNew.Left;
						}
						ksNew = findNew;
					} else {
						ksNew = ks.Left;
					}
				} else {
					CLCell findNew = ks.Right;
					while (findNew != null && findNew.Right != null) {
						findNew = findNew.Right;
					}
					ksNew = findNew;
				}
				break;
			case CLKey.CLRight:
				if (ks.Right != null) {
					if (isCtrlPressed) {
						CLCell findNew = ks.Right;
						while (findNew != null && findNew.Right != null) {
							if (findNew.Colour != CLColour.CLNone)
								break;
							findNew = findNew.Right;
						}
						ksNew = findNew;
					} else {
						ksNew = ks.Right;
					}
				} else {
					CLCell findNew = ks.Left;
					while (findNew != null && findNew.Left != null) {
						findNew = findNew.Left;
					}
					ksNew = findNew;
				}
				break;
			case CLKey.CLUp:
				if (ks.Top != null) {
					if (isCtrlPressed) {
						CLCell findNew = ks.Top;
						while (findNew != null && findNew.Top != null) {
							if (findNew.Colour != CLColour.CLNone)
								break;
							findNew = findNew.Top;
						}
						ksNew = findNew;
					} else {
						ksNew = ks.Top;
					}
				} else {
					CLCell findNew = ks.Bottom;
					while (findNew != null && findNew.Bottom != null) {
						findNew = findNew.Bottom;
					}
					ksNew = findNew;
				}
				break;
			case CLKey.CLDown:
				if (ks.Bottom != null) {
					if (isCtrlPressed) {
						CLCell findNew = ks.Bottom;
						while (findNew != null && findNew.Bottom != null) {
							if (findNew.Colour != CLColour.CLNone)
								break;
							findNew = findNew.Bottom;
						}
						ksNew = findNew;
					} else {
						ksNew = ks.Bottom;
					}
				} else {
					CLCell findNew = ks.Top;
					while (findNew != null && findNew.Top != null) {
						findNew = findNew.Top;
					}
					ksNew = findNew;
				}
				break;
			case CLKey.CLEnter:
				if (this.popUpAnimating/* || this.achievementAnimating*/)
					return;
				if (this.popUpLabel != null) {
					if (this.popUpLabel.Action != null) {
						CLField.PlaySound("MenuNav.mp3");
						this.popUpLabel.Action?.Invoke();
						return;
					} else if (this.popUpLabel.OutAction != null) {
						CLField.PlaySound("MenuNav.mp3");
						this.popUpLabel.OutAction?.Invoke();
						return;
					}
				}
				if (this.selectableNavigation && this.activeField)
					this.CellAction(ks);
				return;
			case CLKey.CLEscape:
				if (this.popUpAnimating/* || this.achievementAnimating*/)
					return;
				if (this.popUpLabel != null) {
					if (this.popUpLabel.OutAction != null) {
						CLField.PlaySound("MenuNav.mp3");
						this.popUpLabel.OutAction?.Invoke();
						return;
					}/* else if (this.popUpLabel.Action != null) {
						CLField.PlaySound("MenuNav.mp3");
						this.popUpLabel.Action?.Invoke();
						return;
					}*/
				} else if (this.menu != null && this.menu.Show) {
					this.menu.Pop();
					return;
				}
				return;
			}
			if (this.entry != null && this.entry.IsVisible)
				return;
			if (this.popUpLabel != null || (this.menu != null && this.menu.Show))
				return;
			if (this.popUpAnimating)
				return;
			if (ksNew == null)
				return;
			if (!this.selectableNavigation) {
				this.selectableNavigation = true;
				keySelectionFlashTime = this.time + CLField.KeyNavigationFlashDelay;
				return;
			}
			keySelectionFlashTime = this.time + CLField.KeyNavigationFlashDelay;
			ks.KeySelected = false;
			ksNew.KeySelected = true;
		}

		private void SaveGame() {
			if (this.teaching)
				return;
			string savedGame = "";
			bool firstAdded = false;
			this.ForAllCells((c,i,j) => {
				if (c.Colour == CLColour.CLNone)
					return;
				if (!firstAdded) {
					firstAdded = true;
				} else {
					savedGame += ";";
				}
				savedGame += string.Format("{0}:{1}={2}", i.ToString(), j.ToString(), ((int)c.Colour).ToString());
			});
			for (int i = 0; i < 3; i++) {
				savedGame += string.Format(";-1:{0}={1}", i.ToString(), ((int)this.nextColours[i]).ToString());
			}
			savedGame += string.Format(";-1:3={0}", this.score.ToString());
			this.settings.SavedGame = savedGame;
		}

		private bool LoadGame() {
			if (this.teaching)
				return false;
			string savedGame = this.settings.SavedGame;
			if (string.IsNullOrEmpty(savedGame))
				return false;
			string []coordinatesValues = savedGame.Split(';');
			if (coordinatesValues == null)
				return false;
			int totalCells = 0, scoreLoaded = 0;
			int maxCells = this.rows*this.columns;
			CLCell [,]loadedCells = new CLCell[this.rows+1,this.columns];
			foreach (string coordinatesValue in coordinatesValues) {
				if (totalCells > maxCells)
				if (string.IsNullOrEmpty(coordinatesValue))
					return false;
				string []csv = coordinatesValue.Split(new char[2] { ':', '=' });
				if (csv == null || csv.Length != 3)
					return false;
				int i = int.Parse(csv[0]),
					j = int.Parse(csv[1]),
					v = int.Parse(csv[2]);
				if (i == -1) {
					if (j == 3) {
						scoreLoaded = v;
						continue;
					}
					loadedCells[this.rows,j] = new CLCell(-1, j) {
						Colour = (CLColour)v
					};
					continue;
				}
				loadedCells[i,j] = new CLCell(i, j) {
					Colour = (CLColour)v
				};
				totalCells++;
			}
			if (scoreLoaded <= 0)
				return false;
			if (totalCells > maxCells)
				return false;
			int loadedBallsCount = 0;
			this.ForAllCells((c,i,j) => {
				if (loadedCells[i,j] == null)
					return;
				c.Colour = loadedCells[i,j].Colour;
				if (c.Colour != CLColour.CLNone)
					loadedBallsCount++;
				if (this.settings.Animations) {
					c.AnimTime = this.time;
					c.Appearing = true;
					CLAnim.AddToQueue(new CLAnim(CLField.AnimAppearingDuration, (start, end, checkTime) => {
						return this.AnimAppearing(start, end, checkTime, c);
					}, true), true);
				}
			});
			this.extraScore = !this.settings.NextColours;
			this.ballsCount = loadedBallsCount;
			this.nextColours[0] = loadedCells[this.rows,0].Colour;
			this.nextColours[1] = loadedCells[this.rows,1].Colour;
			this.nextColours[2] = loadedCells[this.rows,2].Colour;
			this.score = scoreLoaded;
			this.userScoreLabel.Text = this.score.ToString();
			this.ShowMessage(App.Strings.RestoredGame);
			return true;
		}
	}
}