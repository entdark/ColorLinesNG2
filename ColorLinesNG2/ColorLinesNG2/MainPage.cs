using System.Collections.Generic;
using Xamarin.Forms;

namespace ColorLinesNG2 {
	public class MainPage : ContentPage {
		public ColorLinesNG Game { get; private set; }
		public MainPage() {
			var backgroundLayout = new RelativeLayout() {
				BackgroundColor = Color.Black
			};
			var mainLayout = new RelativeLayout() {
				BackgroundColor = Color.Transparent
			};

			var textures = new List<string>();
			foreach (string cellColour in cellColours) {
				textures.Add(string.Format("CLNG_{0}.png", cellColour));
			}
			View []hackyViews = null;
			if (Settings.Taught) {
				hackyViews = this.InitHackyViews();
			}
			this.Game = new ColorLinesNG(mainLayout, textures, hackyViews);

			mainLayout.Children.Add(
				this.Game.GameView,
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Height;
				})
			);
			if (Settings.Taught) {
				this.AddHackyViews(mainLayout, hackyViews);
			}

			backgroundLayout.Children.Add(
				mainLayout,
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					switch (Device.RuntimePlatform) {
					default:
					case Device.Android:
						return 0.0;
					case Device.iOS:
						return 20.0;
					}
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width;
				}),
				Constraint.RelativeToParent(parent => {
					switch (Device.RuntimePlatform) {
					default:
					case Device.Android:
						return parent.Height;
					case Device.iOS:
						return parent.Height - 20.0;
					}
				})
			);

			Content = backgroundLayout;
		}

		private View []InitHackyViews() {
			var colour = Color.FromRgba(0.0, 0.0, 0.0, 1.0);
			var tapFieldBoxView = new BoxView() {
				Color = Color.Transparent
			};
			var bestScoreLabel = new CLFormsLabel() {
				Text = "0",
				TextColor = colour,
				HorizontalTextAlignment = TextAlignment.End
			};
			var userScoreLabel = new CLFormsLabel() {
				Text = "0",
				TextColor = colour,
				HorizontalTextAlignment = TextAlignment.End
			};
			var resultsLabel = new CLFormsLabel() {
				Text = Strings.Results,
				TextColor = colour
			};
			var startLabel = new CLFormsLabel() {
				Text = Strings.Restart,
				TextColor = colour
			};
			var settingsLabel = new CLFormsLabel() {
				Text = Strings.Settings,
				TextColor = colour
			};
			var nameEntry = new CLFormsEntry() {
				Text = "",
				Keyboard = Keyboard.Text,
//				IsVisible = false,
				LettersLimit = 11
			};
			return new View []{ tapFieldBoxView, bestScoreLabel, userScoreLabel, resultsLabel, startLabel, settingsLabel, nameEntry };
		}
		private void AddHackyViews(RelativeLayout mainLayout, View []hackyViews) {
			mainLayout.Children.Add(
				hackyViews[0],
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					return (parent.Height - parent.Width) * 0.5;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width;
				})
			);
			mainLayout.Children.Add(
				hackyViews[1],
				Constraint.RelativeToParent(parent => {
					double left = 0.0;
					left = (hackyViews[1] as ICLForms).Padding.Left;
					left += (hackyViews[1] as ICLForms).RelativePadding.Left * parent.Width * 0.1111;
					return 0.0 + left;
				}),
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					double left = 0.0, right = 0.0;
					left = (hackyViews[1] as ICLForms).Padding.Left;
					left += (hackyViews[1] as ICLForms).RelativePadding.Left * parent.Width * 0.1111;
					right = (hackyViews[1] as ICLForms).Padding.Right;
					right += (hackyViews[1] as ICLForms).RelativePadding.Right * parent.Width * 0.1111;
					return parent.Width * 0.33333 - (left + right);
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
			mainLayout.Children.Add(
				hackyViews[2],
				Constraint.RelativeToParent(parent => {
					double left = 0.0;
					left = (hackyViews[2] as ICLForms).Padding.Left;
					left += (hackyViews[2] as ICLForms).RelativePadding.Left * parent.Width * 0.1111;
					return parent.Width * 0.33333 * 2.0  +left;
				}),
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					double left = 0.0, right = 0.0;
					left = (hackyViews[2] as ICLForms).Padding.Left;
					left += (hackyViews[2] as ICLForms).RelativePadding.Left * parent.Width * 0.1111;
					right = (hackyViews[2] as ICLForms).Padding.Right;
					right += (hackyViews[2] as ICLForms).RelativePadding.Right * parent.Width * 0.1111;
					return parent.Width * 0.33333 - (left + right);
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
			mainLayout.Children.Add(
				hackyViews[3],
				Constraint.RelativeToParent(parent => {
					return 0.0;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Height - parent.Width * 0.1111;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.33333;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
			mainLayout.Children.Add(
				hackyViews[4],
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.33333;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Height - parent.Width * 0.1111;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.33333;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
			mainLayout.Children.Add(
				hackyViews[5],
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.33333 * 2.0;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Height - parent.Width * 0.1111;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.33333;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
			mainLayout.Children.Add(
				hackyViews[6],
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111 * 2.0 + parent.Width * 0.1111 * 0.32f * Strings.Name.Length;
				}),
				Constraint.RelativeToParent(parent => {
					return (parent.Height - parent.Width) * 0.5 + parent.Width * 0.1111 * 4.0;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111 * 5;
				}),
				Constraint.RelativeToParent(parent => {
					return parent.Width * 0.1111;
				})
			);
		}

		private static readonly string[] cellColours = {
			"Cell",
			"Red",
			"Yellow",
			"Green",
			"Cyan",
			"Blue",
			"Pink",
			"Brown",
			"LabelMicro",
			"LabelSmall",
			"LabelMedium",
			"LabelLong",
			"LabelLarge",
		};
	}

	public interface ICLForms {
		Thickness Padding { get; set; }
		Thickness RelativePadding { get; set; }
		float TextScale { get; set; }
	}
	public class CLFormsLabel : Label, ICLForms {
		public Thickness Padding { get; set; }
		public Thickness RelativePadding { get; set; }
		public float TextScale { get; set; }
		public CLFormsLabel() {
			this.Padding = 0.0;
			this.RelativePadding = 0.0;
			this.TextScale = 1.0f;
			string fontFamily = null;
			double namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
			switch(Device.RuntimePlatform) {
			default:
				break;
			case Device.Android:
				fontFamily = "fonts/PressStart2P.ttf#PressStart2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.8;
				break;
			case Device.iOS:
				fontFamily = "PressStart2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.84;
				break;
			}
			this.FontFamily = fontFamily;
			this.FontSize = namedSize;
			this.BackgroundColor = Color.Transparent;
			this.TextColor = Color.FromRgb(0, 170, 0);
			this.HorizontalTextAlignment = TextAlignment.Center;
			this.VerticalTextAlignment = TextAlignment.Center;
		}
		protected override void OnPropertyChanged(string propertyName) {
			if (propertyName == Label.HorizontalTextAlignmentProperty.PropertyName) {
				if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Center) {
					this.Padding = 0.0;
					this.RelativePadding = 0.0;
				} else if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Start) {
//					this.Padding = new Thickness(13.37, 0.0, 0.0, 0.0);
					this.RelativePadding = new Thickness(0.38, 0.0, 0.0, 0.0);
				} else if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.End) {
//					this.Padding = new Thickness(0.0, 0.0, 13.37, 0.0);
					this.RelativePadding = new Thickness(0.0, 0.0, 0.38, 0.0);
				}
			}
			base.OnPropertyChanged(propertyName);
		}
	}
	public class CLFormsEntry : Entry, ICLForms {
		public Thickness Padding { get; set; }
		public Thickness RelativePadding { get; set; }
		public float TextScale { get; set; }
		public int LettersLimit = -1;
		public CLFormsEntry() {
			this.Padding = 0.0;
			this.RelativePadding = 0.0;
			this.TextScale = 1.0f;
			string fontFamily = null;
			double namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label));
			switch(Device.RuntimePlatform) {
			default:
				break;
			case Device.Android:
				fontFamily = "fonts/PressStart2P.ttf#PressStart2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.8;
				break;
			case Device.iOS:
				fontFamily = "PressStart2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.8;
				break;
			}
			this.FontFamily = fontFamily;
			this.FontSize = namedSize;
			this.BackgroundColor = Color.Transparent;
			this.TextColor = Color.FromRgb(0, 170, 0);
			this.HorizontalTextAlignment = TextAlignment.Start;
			this.TextChanged += (sender, ev) => {
				CLFormsEntry entry = (sender as CLFormsEntry);
				string text = ev.NewTextValue;
				int limit = this.LettersLimit;

				if (limit > 0 && text.Length > limit) {
					text = text.Remove(limit);
					entry.Text = text;
				}
			};
		}
/*		protected override void OnPropertyChanged(string propertyName) {
			if (propertyName == Entry.HorizontalTextAlignmentProperty.PropertyName) {
				if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Center) {
					this.Padding = 0.0;
				} else if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Start) {
					this.Padding = new Thickness(13.37, 0.0, 0.0, 0.0);
				} else if (this.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.End) {
					this.Padding = new Thickness(0.0, 0.0, 13.37, 0.0);
				}
			}
			base.OnPropertyChanged(propertyName);
		}*/
	}
}
