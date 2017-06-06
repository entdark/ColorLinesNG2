using System;
using System.Threading.Tasks;

using CLAnimations;
using CLRenderer;

using ColorLinesNG2;

using Xamarin.Forms;

namespace CLDataTypes {
	public enum CLColour {
		CLNone,
		CLRed,
		CLYellow,
		CLGreen,
		CLCyan,
		CLBlue,
		CLPink,
		CLBrown,
		CLMax,
	}
	public enum CLLabelSize {
		CLMicro,
		CLSmall,
		CLMedium,
		CLLong,
		CLLarge,
	}

	public class CLLabel : IDisposable {
		public string Text {
			get {
				return this.label.Text;
			}
			set {
				Device.BeginInvokeOnMainThread(() => {
					this.label.Text = value;
				});
			}
		}
		public Color TextColor {
			get {
				return this.label.TextColor;
			}
			set {
				Device.BeginInvokeOnMainThread(() => {
					if (value.A == 0.0) {
						if (this.label.IsVisible)
							this.label.IsVisible = false;
					} else {
						if (!this.label.IsVisible)
							this.label.IsVisible = true;
					}
					this.label.TextColor = value;
				});
			}
		}

		public Action Action, ExtraDraw, OutAction;

		private float x, y, width, height;
		private TextAlignment align;

		private CLFormsLabel label;
		private View outTap;
		private bool added;

		public CLLabel(string text, float x, float y, float width, float height, Color textColour, TextAlignment align, float textScale = 1.0f, View hackyView = null) {
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.align = align;
			if (hackyView == null || !(hackyView is CLFormsLabel)) {
				this.added = false;
				this.label = new CLFormsLabel() {
					Text = text,
					TextColor = textColour,
					HorizontalTextAlignment = this.align,
					TextScale = textScale
				};
			} else {
				this.added = true;
				this.label = (CLFormsLabel)hackyView;
				Device.BeginInvokeOnMainThread(() => {
					this.label.Text = text;
					this.label.TextColor = textColour;
					this.label.HorizontalTextAlignment = this.align;
					this.label.TextScale = textScale;
				});
			}
			bool actionDelayed = false;
			var tapped = new TapGestureRecognizer();
			tapped.Tapped += (sender, ev) => {
				if (actionDelayed) {
					return;
				}
				actionDelayed = true;
				Task.Run(async () => {
					await Task.Delay(500);
					actionDelayed = false;
				});
				this?.Action?.Invoke();
			};
			this.label.GestureRecognizers.Add(tapped);
			this.outTap = new BoxView() {
				BackgroundColor = Color.Transparent
			};
//			bool outActionDelayed = false;
			tapped = new TapGestureRecognizer();
			tapped.Tapped += (sender, ev) => {
				if (actionDelayed) {
					return;
				}
				actionDelayed = true;
				Task.Run(async () => {
					await Task.Delay(500);
					actionDelayed = false;
				});
				this?.OutAction?.Invoke();
			};
			this.outTap.GestureRecognizers.Add(tapped);
		}
		public void Draw(int textureId, float []outBounds = null) {
			CLReDraw.Rect(this.x, this.y, this.width, this.height, textureId);
			if (!this.added) {
				if (outBounds != null) {
					float x = outBounds[0],
						y = outBounds[3],
						width = outBounds[1]-outBounds[0],
						height = outBounds[3]-outBounds[2];
					CLReDraw.View(this.outTap, x, y, width, height);
				} else {
					this.outTap = null;
				}
				CLReDraw.View(this.label, this.x, this.y, this.width, this.height);
				this.added = true;
			}
		}
		public void Dispose() {
			this.added = true;
			this.Action = this.OutAction = null;
			if (this.outTap != null)
				CLReDraw.ReleaseView(this.outTap);
			CLReDraw.ReleaseView(this.label);
		}
	}
	public class CLCheckBox : IDisposable {
		private const uint AnimFade = 256;

		private bool check = true;
		public bool Check {
			get { return this.check; }
			set {
				if (this.Checked != null) {
					this.Checked(this, new CLCheckBoxEventArgs(value));
				}
				this.check = value;
				Device.BeginInvokeOnMainThread(() => {
					this.yn.Text = value ? Strings.Yes : Strings.No;
					this.yn.TextColor = value ? this.textColour : CLField.GrayColor;
				});
			}
		}
		private bool isEnabled = true;
		public bool IsEnabled {
			get {
				return this.isEnabled;
			}
			set {
				this.isEnabled = value;
				Device.BeginInvokeOnMainThread(() => {
//					this.label.TextColor = (value) ? this.textColour : CLField.GrayColor;
//					this.yn.TextColor = (value && this.check) ? this.textColour : CLField.GrayColor;
					if (value) {
						if (this.added) {
							this.label.TranslateTo(0.0, 0.0, CLCheckBox.AnimFade, Easing.CubicOut);
							this.yn.TranslateTo(0.0, 0.0, CLCheckBox.AnimFade, Easing.CubicOut);
							this.label.FadeTo(1.0, CLCheckBox.AnimFade, Easing.CubicOut);
							this.yn.FadeTo(1.0, CLCheckBox.AnimFade, Easing.CubicOut);
						} else {
							this.label.TranslationY = 0.0;
							this.yn.TranslationY = 0.0;
							this.label.Opacity = 1.0;
							this.yn.Opacity = 1.0;
						}
					} else {
						if (this.added && this.label.Height > 0 && this.yn.Height > 0) {
							this.label.TranslateTo(0.0, 0.0-this.label.Height, CLCheckBox.AnimFade, Easing.CubicOut);
							this.yn.TranslateTo(0.0, 0.0-this.yn.Height, CLCheckBox.AnimFade, Easing.CubicOut);
							this.label.FadeTo(0.0, CLCheckBox.AnimFade, Easing.CubicOut);
							this.yn.FadeTo(0.0, CLCheckBox.AnimFade, Easing.CubicOut);
						} else {
							//this is too hacky, but whatever
							Task.Run(() => {
								bool breakfast = false;
								while (true) {
									if (breakfast)
										break;
									Device.BeginInvokeOnMainThread(() => {
										if (this.label.Height <= 0 || this.yn.Height <= 0) {
										} else {
											this.label.TranslationY = 0.0-this.label.Height;
											this.yn.TranslationY = 0.0-this.yn.Height;
											this.label.Opacity = 0.0;
											this.yn.Opacity = 0.0;
											breakfast = true;
										}
									});
									Task.Delay(500);
								}
							});
							this.label.FadeTo(0.0, CLCheckBox.AnimFade, Easing.CubicOut);
							this.yn.FadeTo(0.0, CLCheckBox.AnimFade, Easing.CubicOut);
						}
					}
				});
			}
		}

		private string text;
		private float x, y, width, height;
		private Color textColour;
		private TextAlignment align;

		private CLFormsLabel label, yn;
		private bool added;

		public CLCheckBox(bool check, string text, float x, float y, float width, float height, Color textColour, TextAlignment align, float textScale = 1.337f) {
			this.text = text;
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.align = align;
			this.textColour = textColour;
			this.label = new CLFormsLabel() {
				Text = text,
				TextColor = this.textColour,
				HorizontalTextAlignment = this.align,
				TextScale = textScale
			};
			var tapped = new TapGestureRecognizer();
			tapped.Tapped += (sender, ev) => {
				if (this.IsEnabled)
					this.Check = !this.Check;
			};
			this.label.GestureRecognizers.Add(tapped);
			this.yn = new CLFormsLabel() {
				TextColor = this.textColour,
				HorizontalTextAlignment = (this.align == TextAlignment.End) ? TextAlignment.Start : TextAlignment.End,
				TextScale = textScale
			};
			this.yn.GestureRecognizers.Add(tapped);
			this.added = false;
			this.Check = check;
			this.IsEnabled = true;
		}
		public void Draw(int textureId, int textureId2) {
/*			float []textureCoords = !this.check ? null : new float[] {
					1.0f, 0.0f,
					0.0f, 0.0f,
					1.0f, 1.0f,
					0.0f, 1.0f,
				};*/
			float ox = this.x+this.width-this.height, oy = this.y, ow = this.height, oh = this.height;
//			CLReDraw.Rect(this.x, this.y, this.width-oh, oh, textureId);
//			CLReDraw.Rect(ox, oy, ow, oh, textureId2, textureCoords);
			if (!added) {
				CLReDraw.View(this.label, this.x, this.y, this.width, this.height);
				CLReDraw.View(this.yn, this.x, this.y, this.width, this.height);
//				CLReDraw.View(this.yn, ox, oy, ow, oh);
				this.added = true;
			}
		}
		public void Dispose() {
			this.added = true;
			CLReDraw.ReleaseView(this.label);
			CLReDraw.ReleaseView(this.yn);
		}

		public event EventHandler<CLCheckBoxEventArgs> Checked;
		
		public class CLCheckBoxEventArgs : EventArgs {
			public CLCheckBoxEventArgs(bool check) {
				this.check = check;
			}
			private bool check;

			public bool Checked {
				get { return this.check; }
				private set { this.check = value; }
			}
		}
	}
	public class CLCell {
		private const long AnimJump = 500;
		private const long AnimAppearing = 500;
		private const long AnimHighlight = 1000;

		public CLCell Top { get; private set; }
		public CLCell Bottom { get; private set; }
		public CLCell Left { get; private set; }
		public CLCell Right { get; private set; }

		private CLColour oldColour;
		public CLColour OldColour {
			get {
				return this.oldColour;
			}
			private set {
				this.oldColour = value;
			}
		}
		private CLColour colour;
		public CLColour Colour {
			get {
				return this.colour;
			}
			set {
				this.OldColour = this.colour;
				this.colour = value;
			}
		}

		public int Row, Column;
		public bool Selected;
		public Action Action;

//		private View tap;
//		private bool added;

		public bool Blowing = false, BlowingStarted = false;
		public bool Appearing = false;
		public bool Moving = false;
		public bool Highlight = false;
		public bool HighlightBlocked = false;
		public bool HighlightForced = false;
		public long AnimTime;

		private CLCell() {}
		//cell is a four linked list
		public CLCell(int row, int column, CLCell top = null, CLCell bottom = null, CLCell left = null, CLCell right = null) {
			this.Colour = CLColour.CLNone;
			this.Row = row;
			this.Column = column;
			this.Selected = false;
			this.Top = top;
			this.Bottom = bottom;
			this.Left = left;
			this.Right = right;
			if (top != null) top.Bottom = this;
			if (bottom != null) bottom.Top = this;
			if (left != null) left.Right = this;
			if (right != null) right.Left = this;
/*			this.tap = new BoxView() {
				BackgroundColor = Color.Transparent
			};
			var tapped = new TapGestureRecognizer();
			tapped.Tapped += (sender, ev) => {
				this?.Action?.Invoke();
			};
			this.tap.GestureRecognizers.Add(tapped);
			this.added = false;*/
			this.Blowing = false;
			this.BlowingStarted = false;
			this.Appearing = false;
			this.Moving = false;
			this.AnimTime = 0;
		}
#if DEBUG
		private static Color CLColourToColor(CLColour colour) {
			if (colour == CLColour.CLRed)
				return Color.FromRgba(255, 0, 0, 255);
			else if (colour == CLColour.CLYellow)
				return Color.FromRgba(255, 255, 0, 255);
			else if (colour == CLColour.CLGreen)
				return Color.FromRgba(0, 255, 0, 255);
			else if (colour == CLColour.CLCyan)
				return Color.FromRgba(0, 255, 255, 255);
			else if (colour == CLColour.CLBlue)
				return Color.FromRgba(0, 0, 255, 255);
			else if (colour == CLColour.CLPink)
				return Color.FromRgba(255, 0, 255, 255);
			else if (colour == CLColour.CLBrown)
				return Color.FromRgba(63, 15, 0, 255);
			else
				return Color.FromRgba(127, 127, 127, 255);
		}
#endif
		public void Draw(long time, float x, float y, float width, float height, int []fieldTextures, bool ignoreTapView = false) {
			if (!this.Selected) {
//#if DEBUG
//				CLReDraw.Rect(x, y, width, height, Color.FromRgba(127, 127, 127, 255));
//#endif
				CLReDraw.Rect(x, y, width, height, fieldTextures[0]);
			} else {
				CLReDraw.Rect(x, y, width, height, fieldTextures[0], new float[] {
					1.0f, 0.0f,
					0.0f, 0.0f,
					1.0f, 1.0f,
					0.0f, 1.0f,
				});
			}
			float dx = 0.0f, dy = 0.0f, dwidth = 0.0f, dheight = 0.0f;
			long delta = time - this.AnimTime;
			if (this.Highlight) {
				while (delta > CLCell.AnimHighlight) {
					delta -= CLCell.AnimHighlight;
				}
				float danim = 1.0f-CLAnim.Jump(delta, CLCell.AnimHighlight);
				danim *= 0.5f;
				Color highlightColour;
				if (!this.HighlightBlocked) {
					highlightColour = CLField.GreenColor.MultiplyAlpha(0.5f-danim);
				} else {
					highlightColour = CLField.RedColor.MultiplyAlpha(0.5f-danim);
				}
				CLReDraw.Rect(x, y, width, height, highlightColour);
			}
			bool tempNonBlowing = (this.Blowing && !this.BlowingStarted && !this.Moving && !this.Appearing);
			if ((this.Colour != CLColour.CLNone && !(this.Appearing || this.Moving)) || tempNonBlowing) {
				delta = time - this.AnimTime;
				if (this.Selected && this.AnimTime != 0) {
					dx = width * -0.1337f * 0.5f;
					dy = height * -0.1337f;
					dwidth = width * 0.1337f;
					dheight = height * -0.1337f;
					delta += CLCell.AnimJump;
					while (delta > CLCell.AnimJump) {
						delta -= CLCell.AnimJump;
					}
					float danim = CLAnim.Jump(delta, CLCell.AnimJump);
					dx *= danim;
					dy *= danim;
					dwidth *= danim;
					dheight *= danim;
				}/* else if (this.appearing && delta <= CLCell.AnimAppearing) {
					dx = width * 0.7f * 0.5f;
					dy = height * -0.7f * 0.5f;
					dwidth = width * -0.7f;
					dheight = height * -0.7f;
					float danim = 1.0f-CLAnim.EaseOutCubic(delta, CLCell.AnimAppearing);
					dx *= danim;
					dy *= danim;
					dwidth *= danim;
					dheight *= danim;
				} else if (this.appearing && delta > CLCell.AnimAppearing) {
					this.appearing = false;
				}*/
				int index;
				if (!tempNonBlowing) {
					index = (int)this.Colour;
				} else {
					index = (int)this.OldColour;
				}
				int textureId = fieldTextures[index];
				CLReDraw.Rect(x + dx, y + dy, width + dwidth, height + dheight, textureId);
//#if DEBUG
//				CLReDraw.Circle(x+width/2, y-height/2, (height > width) ? (width * 0.35f) : (height * 0.35f), CLColourToColor(this.colour));
//#endif
			}
/*			if (!this.added && !ignoreTapView) {
				CLReDraw.View(this.tap, x, y, width, height);
				this.added = true;
			}*/
		}
	}
}