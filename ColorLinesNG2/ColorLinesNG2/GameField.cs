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
			public CLSettings() {
				this.nextColours = Settings.NextColours;
				this.animations = Settings.Animations;
				this.route = Settings.Route;
				this.confirmMove = Settings.ConfirmMove;
				this.taught = Settings.Taught;
			}
		}

		public static readonly Color GreenColor = Color.FromRgba(0, 170, 0, 255);
		public static readonly Color GreenColorTransparent = Color.FromRgba(0, 170, 0, 0);
		public static readonly Color RedColor = Color.FromRgba(255, 0, 0, 255);
		public static readonly Color GrayColor = Color.FromRgba(85, 85, 85, 255);
		public static readonly Color BlackColor = Color.FromRgba(0, 0, 0, 255);
		public static readonly Color WhiteColor = Color.FromRgba(255, 255, 255, 255);

		public const long AnimGreetingsDuration = 700;
		public const long AnimRevealDuration = 512;
		public const long AnimAppearingDuration = 128;
		public const long AnimBlowingDuration = 512;
		public const long AnimMovingDurationCoeff = 50;
		public const long AnimClearingDuration = 256;
		public const long AnimNonRouteDuration = 256;
		public const long AnimPopUpDuration = 170;

		public const int BestScoresCount = 10;

		private long time;
		private bool activeField = true;
		private bool activeLabels = true;
		private bool firstSelected = false;
		private int []fieldTextures;

		private int rows, columns;
		private float left, right, bottom, top;
		private float step;
		private int ballsCount;
		private int cellsCount;
		private CLCell cells, selected;
		private CLColour[] nextColours;

		private int score;
		private bool extraScore;
		private string[,] scoresTable;

		private bool teaching;
		private bool teachingInit = false;
		private bool greetings = true;
		private bool greetingsClosing = false;
		private CLTutorial selectTutorial;
		private CLTutorial moveTutorial;
		private CLTutorial blowTutorial;
		private CLTutorial blockedTutorial;
		private CLLabel tutLineLabel, tutGreetingsLabel, tutSkipLabel;

		private CLSettings settings;
		private View cellsTapView;
		private CLFormsEntry entry;
		private CLLabel bestScoreLabel, userScoreLabel, resultsLabel, startLabel, settingsLabel, popUpLabel;
		private CLCheckBox nextColoursCheckBox, animationsCheckBox, routeCheckBox, confirmMoveCheckBox;
		private bool popUpAnimating = false;
		private bool tapViewAdded = false;

		private CLField() {}
		public CLField(int []fieldTextures) : this(9, 9, fieldTextures) {}
		public CLField(int rows, int columns, int []fieldTextures) {
			this.settings = new CLSettings();
			this.teaching = !this.settings.Taught;
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
		}
		private void CellAction(CLCell cell) {
			if (this.activeField) {
				if (!this.SelectBall(cell)) {
					if (!(this.teaching && cell.Highlight))
						cell.AnimTime = 0;
					this.MoveBall(cell);
				} else {
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
				Strings.TutTapBall,
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
				Strings.TutTapBlock,
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
				Strings.TutMakeLine,
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
				Strings.TutBlocked,
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
			//just add them - they are transparent, anyways
			if (this.teaching && !this.teachingInit) {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLSmall;
				this.bestScoreLabel.Draw(this.fieldTextures[textureId]);
				this.userScoreLabel.Draw(this.fieldTextures[textureId]);
				this.resultsLabel.Draw(this.fieldTextures[textureId]);
				this.startLabel.Draw(this.fieldTextures[textureId]);
				this.settingsLabel.Draw(this.fieldTextures[textureId]);
				CLReDraw.Rect(this.left, this.top, step*9, step, Color.FromRgba(0.0, 0.0, 0.0, 1.0));
				CLReDraw.Rect(this.left, this.bottom+step, step*9, step, Color.FromRgba(0.0, 0.0, 0.0, 1.0));
				this.teachingInit = true;
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
				float xoffset = step * 0.32f * Strings.Name.Length;
				CLReDraw.View(this.cellsTapView, -1.0f, 1.0f, step*9, step*9);
				CLReDraw.View(this.entry, -1.0f+step*2+xoffset, 1.0f-step*4, step*5, step);
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
				c.Draw(time, j*step-1.0f, 1.0f-i*step, step, step, this.fieldTextures, true);
			});
			float radius = step * 0.2f;
			bool nc = this.settings.NextColours;
			if (!this.teaching) {
				CLReDraw.Rect(this.left+step*3, this.top, step, step, this.fieldTextures[0]);
				if (this.nextColours[0] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*3.5f-radius, this.top-step*0.5f+radius, radius*2, radius*2, this.fieldTextures[(int)this.nextColours[0]]);
				CLReDraw.Rect(this.left+step*4, this.top, step, step, this.fieldTextures[0]);
				if (this.nextColours[1] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*4.5f-radius, this.top-step*0.5f+radius, radius*2, radius*2, this.fieldTextures[(int)this.nextColours[1]]);
				CLReDraw.Rect(this.left+step*5, this.top, step, step, this.fieldTextures[0]);
				if (this.nextColours[2] != CLColour.CLNone && nc)
					CLReDraw.Rect(this.left+step*5.5f-radius, this.top-step*0.5f+radius, radius*2, radius*2, this.fieldTextures[(int)this.nextColours[2]]);

				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLSmall;
				this.bestScoreLabel.Draw(this.fieldTextures[textureId]);
				this.userScoreLabel.Draw(this.fieldTextures[textureId]);
				this.resultsLabel.Draw(this.fieldTextures[textureId]);
				this.startLabel.Draw(this.fieldTextures[textureId]);
				this.settingsLabel.Draw(this.fieldTextures[textureId]);
			} else {
				if (!this.greetingsClosing)
					this.tutLineLabel?.ExtraDraw();
				this.tutSkipLabel?.ExtraDraw();
			}
			this.activeLabels = this.activeField = !CLAnim.ExecQueue(this.time) && this.popUpLabel == null;
			if (!this.popUpAnimating)
				this.popUpLabel?.ExtraDraw?.Invoke();
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
			int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMicro;
			CLReDraw.Rect(this.left+step*deltaTime, this.top-step, step, step, this.fieldTextures[textureId]);
			this.lastTime = this.time;
		}
#else
		private void DrawFPS(float step) {}
#endif

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
			this.time = time;

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
				this.CellAction(cell);
			}));
			if (hackyViews == null)
				hackyViews = new View [7]{ null, null, null, null, null, null, null };
			var colour = !this.teaching ? CLField.GreenColor : CLField.GreenColorTransparent;
			this.bestScoreLabel = new CLLabel(this.scoresTable[0,1], left, top, width, height, colour, TextAlignment.End, hackyView:hackyViews[1]);
			this.userScoreLabel = new CLLabel(this.score.ToString(), left+step*6, top, width, height, colour, TextAlignment.End, hackyView: hackyViews[2]);
			this.resultsLabel = new CLLabel(Strings.Results, left, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[3]);
			this.startLabel = new CLLabel(Strings.Restart, left+step*3, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[4]);
			this.settingsLabel = new CLLabel(Strings.Settings, left+step*6, bottom+step, width, height, colour, TextAlignment.Center, hackyView: hackyViews[5]);

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
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { left+step*3, bottom+step, width, height };
					float []endRect = { -1.0f+step*2, 1.0f-step*4, step*5, height };
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
					}));
				}
				this.popUpLabel = new CLLabel(Strings.RestartQ, -1.0f+step*2, 1.0f-step*4, step*5, height, CLField.RedColor, TextAlignment.Center);
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
					this.popUpLabel.Draw(this.fieldTextures[textureId], bounds);
				};
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
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = { -1.0f+step*2, 1.0f-step*4, step*5, height };
						float []endRect = { -1.0f+step*3, 1.0f-step*4-step*1/5, step*3, step*3/5 };
						int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
				};
			};
			this.settingsLabel.Action = delegate() {
				if (!this.activeLabels)
					return;
				this.activeLabels = false;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
				}
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { left+step*6, bottom+step, width, height };
					float []endRect = { -1.0f+step, 1.0f-step, step*7, step*7 };
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId);
					}));
				}
				float offset = step * 0.5f;
				this.popUpLabel = new CLLabel("", -1.0f+step, 1.0f-step, step*7, step*7, CLField.RedColor, TextAlignment.Center);
				this.nextColoursCheckBox = new CLCheckBox(this.settings.NextColours, Strings.Next, -1.0f+step+offset, 1.0f-step-offset, step*6, step, CLField.GreenColor, TextAlignment.Start, 1.2f);
				this.animationsCheckBox = new CLCheckBox(this.settings.Animations, Strings.Animations, -1.0f+step+offset, 1.0f-step*3.0f-offset, step*6, step, CLField.GreenColor, TextAlignment.Start, 1.2f);
				this.routeCheckBox = new CLCheckBox(this.settings.Route, Strings.Route, -1.0f+step+offset, 1.0f-step*4.0f-offset, step*6, step, CLField.GreenColor, TextAlignment.Start, 1.2f) {
					IsEnabled = this.settings.Animations
				};
				this.confirmMoveCheckBox = new CLCheckBox(this.settings.ConfirmMove, Strings.ConfirmMove, -1.0f+step+offset, 1.0f-step*2.0f-offset, step*6, step, CLField.GreenColor, TextAlignment.Start, 1.2f);
				this.animationsCheckBox.Checked += (sender, ev) => {
					this.routeCheckBox.IsEnabled = ev.Checked;
				};
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
					this.popUpLabel.Draw(this.fieldTextures[textureId], bounds);
					textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
					int textureId2 = (int)CLColour.CLMax+(int)CLLabelSize.CLMicro;
					this.nextColoursCheckBox.Draw(this.fieldTextures[textureId], this.fieldTextures[textureId2]);
					this.confirmMoveCheckBox.Draw(this.fieldTextures[textureId], this.fieldTextures[textureId2]);
					//draw animations after route so that animations is on top of the drawing tree
					//and thus animations recognize gestures when route is disabled (placed just under animations)
					this.routeCheckBox.Draw(this.fieldTextures[textureId], this.fieldTextures[textureId2]);
					this.animationsCheckBox.Draw(this.fieldTextures[textureId], this.fieldTextures[textureId2]);
				};
/*				this.popUp.Action = delegate() {
					this.popUpLabel.Action = null;
					this.activeField = true;
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
				};*/
				this.popUpLabel.OutAction = delegate() {
					this.popUpLabel.OutAction = null;
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					this.activeField = true;
					this.settings.NextColours = this.nextColoursCheckBox.Check;
					this.settings.Animations = this.animationsCheckBox.Check;
					this.settings.Route = this.routeCheckBox.Check;
					this.settings.ConfirmMove = this.confirmMoveCheckBox.Check;
					this.nextColoursCheckBox.Dispose();
					this.animationsCheckBox.Dispose();
					this.routeCheckBox.Dispose();
					this.confirmMoveCheckBox.Dispose();
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = { -1.0f+step, 1.0f-step, step*7, step*7 };
						float []endRect = { -1.0f+step*2, 1.0f-step*2, step*5, step*5 };
						int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
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
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					bool fromCentre = (this.entry == null || !this.entry.IsVisible);
					float []startRect = fromCentre ? new float[] { left, bottom+step, width, height } : new float[] { -1.0f+step*2, 1.0f-step*4, step*5, step };
					float []endRect = { -1.0f+step, 1.0f-step, step*7, step*7 };
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
					int oldTextureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
					CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
						return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, fromCentre ? -1 : oldTextureId);
					}));
				}
				this.popUpLabel = new CLLabel(GetBestScoresList(), -1.0f+step, 1.0f-step, step*7, step*7, CLField.GreenColor, TextAlignment.Center, 1.337f);
				this.popUpLabel.ExtraDraw = delegate() {
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
/*					CLReDraw.Rect(-1.0f+step, 1.0f-step, step*7, step*7, this.fieldTextures[textureId]);
					this.popUp.DrawText(0.0f, -step*3.5f);*/
					this.popUpLabel.Draw(this.fieldTextures[textureId], bounds);

				};
				this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
					this.popUpLabel.Action = this.popUpLabel.OutAction = null;
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					this.activeField = true;
					if (this.popUpLabel != null) {
						this.popUpLabel.Dispose();
					}
					this.popUpLabel = null;
					if (this.settings.Animations) {
						this.activeField = false;
						this.popUpAnimating = true;
						float []startRect = { -1.0f+step, 1.0f-step, step*7, step*7 };
						float []endRect = { -1.0f+step*2, 1.0f-step*2, step*5, step*5 };
						int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
						CLAnim.AddToQueue(new CLAnim(CLField.AnimPopUpDuration, (start, end, checkTime) => {
							return this.AnimPopUp(start, end, checkTime, startRect, endRect, textureId, appearing: false);
						}));
					}
				};
			};
			this.tutGreetingsLabel = new CLLabel(Strings.TapMe, left, 1.0f-step*4, step*9, step, CLField.GreenColor, TextAlignment.Center);
			this.tutGreetingsLabel.ExtraDraw = delegate () {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLong;
				this.tutGreetingsLabel.Draw(this.fieldTextures[textureId]/*, bounds*/);
			};
			this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = delegate() {
				this.tutGreetingsLabel.Text = Strings.Hi;
				this.tutGreetingsLabel.Action/* = this.tutGreetingsLabel.OutAction*/ = delegate() {
					this.tutGreetingsLabel.Text = Strings.CompleteTutorial;
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
			this.tutLineLabel = new CLLabel("", left, bottom+step, step*9, step, CLField.GreenColor, TextAlignment.Center);
			this.tutLineLabel.ExtraDraw = delegate () {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLong;
				this.tutLineLabel.Draw(this.fieldTextures[textureId]);
			};
			this.tutSkipLabel = new CLLabel(Strings.Skip, left+step*3, top, step*3, step, CLField.RedColor, TextAlignment.Center);
			this.tutSkipLabel.Action = delegate () {
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
				this.settingsLabel.TextColor = CLField.GreenColor;*/
			};
			this.tutSkipLabel.ExtraDraw = delegate () {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLSmall;
				this.tutSkipLabel.Draw(this.fieldTextures[textureId]);
			};
			this.InitTutorials();
			this.AddBalls(true);
		}
		private string GetBestScoresList() {
			const int positionWidth = 19;
			string list = "";
			for (int i = 0; i < CLField.BestScoresCount; i++) {
				if (i > 0) {
					list += "\n\n";
				}
				int dots = positionWidth - this.scoresTable[i,0].Length - this.scoresTable[i,1].Length - 4;
				list += String.Format("{0,2}", (i+1).ToString()) + ". " + this.scoresTable[i,0];
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
					CLAnim.AddToQueue(new CLAnim(CLField.AnimBlowingDuration, (start, end, checkTime) => {
						bool ret = this.AnimBlowing(start, end, checkTime, colour);
						if (ret)
							this.userScoreLabel.Text = this.score.ToString();
						return ret;
					}));
				} else {
					this.userScoreLabel.Text = this.score.ToString();
				}
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
			CLCell cell = this.cells;
			for (int i = 0; i < this.rows; i++, cell = cell.Bottom) {
				for (int j = 0; j < this.columns; j++, cell = cell.Right) {
					if (cell == null)
						break;
					if (cell.Colour == CLColour.CLNone)
						map[j, i] = true;
					else
						map[j, i] = false;
				}
				cell = this.cells;
				for (int k = 0; k < i; k++, cell = cell.Bottom);
				if (cell == null)
					break;
			}
			SearchParameters searchParameters = new SearchParameters(new System.Drawing.Point(from.Column, from.Row), new System.Drawing.Point(to.Column, to.Row), map);
			PathFinder pathFinder = new PathFinder(searchParameters);
			var path = pathFinder.FindPath();
			if (path.Count > 0) {
				if (this.settings.Animations || this.teaching) {
					var pathActual = new List<System.Drawing.Point>();
					pathActual.Add(new System.Drawing.Point(from.Column, from.Row));
					pathActual.AddRange(path);
					CLColour colour = from.Colour;
					to.Moving = true;
					long duration = this.settings.Route ? pathActual.Count*CLField.AnimMovingDurationCoeff : CLField.AnimNonRouteDuration;
					CLAnim.AddToQueue(new CLAnim(duration, (start, end, checkTime) => {
						return this.AnimMoving(start, end, checkTime, colour, pathActual);
					}));
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
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			var bounds = new float[] { this.left, this.right, this.bottom, this.top };
			this.popUpLabel = new CLLabel(Strings.NewRecord, -1.0f+step*2, 1.0f-step*4, step*5, step, CLField.GreenColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate() {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
				this.popUpLabel.Draw(this.fieldTextures[textureId], bounds);
			};
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.activeLabels = true;
				if (this.popUpAnimating)
					return;
				this.activeField = false;
				if (this.popUpLabel != null) {
					this.popUpLabel.Dispose();
					this.popUpLabel = null;
				}
				if (this.popUpLabel == null)
					this.popUpLabel = new CLLabel(Strings.Name, -1.0f+step*2, 1.0f-step*4, step*5, step, CLField.GreenColor, TextAlignment.Start);
				bool entryRemoved = false;
				Action action = delegate() {
					this.activeLabels = true;
					if (this.popUpAnimating)
						return;
					if (!string.IsNullOrWhiteSpace(this.entry.Text)
					&& !string.IsNullOrEmpty(this.entry.Text)) {
						entryRemoved = true;
						this.SaveUserScore();
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
							this.popUpLabel.Action = this.popUpLabel.OutAction = null;
							this.activeLabels = true;
							if (this.popUpAnimating)
								return;
							//TODO: add fancy adding new score animation
							if (this.settings.Animations) {
								this.activeField = false;
								this.popUpAnimating = true;
								float[] startRect = { -1.0f+step, 1.0f-step, step*7, step*7 };
								float[] endRect = { -1.0f+step*2, 1.0f-step*2, step*5, step*5 };
								int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLarge;
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
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
					this.popUpLabel.Draw(this.fieldTextures[textureId]/*, bounds*/);
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
				float xoffset = step * 0.32f * Strings.Name.Length;
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
			this.activeField = false;
			if (this.popUpLabel != null) {
				this.popUpLabel.Dispose();
			}
			var bounds = new float[] { this.left, this.right, this.bottom, this.top };
			this.popUpLabel = new CLLabel(Strings.GameOver, -1.0f+step*2, 1.0f-step*4, step*5, step, CLField.RedColor, TextAlignment.Center);
			this.popUpLabel.ExtraDraw = delegate() {
				int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
				this.popUpLabel.Draw(this.fieldTextures[textureId], bounds);
			};
			this.popUpLabel.Action = this.popUpLabel.OutAction = delegate() {
				this.popUpLabel.Action = this.popUpLabel.OutAction = null;
				this.activeLabels = true;
				if (this.popUpAnimating)
					return;
				if (this.settings.Animations) {
					this.activeField = false;
					this.popUpAnimating = true;
					float []startRect = { -1.0f+step*2, 1.0f-step*4, step*5, step };
					float []endRect = { -1.0f+step*3, 1.0f-step*4-step*1/5, step*3, step*3/5 };
					int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLMedium;
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

				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[(int)colour], fill);
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

				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[(int)colour], fill);
			});
			return false;
		}
		private bool AnimMoving(long start, long end, bool checkTime, CLColour colour, List<System.Drawing.Point> path) {
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

				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[(int)colour]);
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

				Color fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[(int)colour], fill);
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

				Color fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);

				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[(int)colour], fill);
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
				CLReDraw.Rect(x, y, width, height, this.fieldTextures[oldTextureId]);
			}
			CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[textureId], fill);
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

			int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLong;
			CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, this.fieldTextures[textureId]);
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
				this.settingsLabel.TextColor = CLField.GreenColor;
				return true;
			}
			if (checkTime) {
				return false;
			}
			float danim = CLAnim.Linear(dt, dr);

			Color fill = CLField.BlackColor.MultiplyAlpha(1.0f-danim);
			CLReDraw.Rect(left, top, step*9, step, fill);

			fill = CLField.WhiteColor.MultiplyAlpha(1.0f-danim);
			int textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLLong;
			CLReDraw.Rect(left, bottom+step, step*9, step, this.fieldTextures[textureId], fill);
			if (this.tutSkipLabel != null) {
				textureId = (int)CLColour.CLMax+(int)CLLabelSize.CLSmall;
				CLReDraw.Rect(left+step*3, top, step*3, step, this.fieldTextures[textureId], fill);
				this.tutSkipLabel.TextColor = CLField.RedColor.MultiplyAlpha(1.0f-danim);
			}
			this.tutLineLabel.TextColor = CLField.GreenColor.MultiplyAlpha(1.0f-danim);
			this.bestScoreLabel.TextColor =
			this.userScoreLabel.TextColor =
			this.resultsLabel.TextColor =
			this.startLabel.TextColor =
			this.settingsLabel.TextColor = CLField.GreenColor.MultiplyAlpha(danim);
			return false;
		}

		private void TutorialApply(CLTutorial tutorial, bool unDone = false) {
			if (this.teaching && this.tutLineLabel != null) {
				string text = CLTutorial.Apply(tutorial, unDone);
				if (text != null)
					this.tutLineLabel.Text = text;
				if (tutorial == null) {
					this.teaching = false;
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
						this.settingsLabel.TextColor = CLField.GreenColor;
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
	}
}