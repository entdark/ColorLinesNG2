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

			View []hackyViews = null;
			if (Settings.Taught) {
				hackyViews = this.InitHackyViews();
			}
			this.Game = new ColorLinesNG(mainLayout, hackyViews);

			bool desktop = Device.Idiom == TargetIdiom.Desktop;
			if (!desktop) {
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
			} else {
				backgroundLayout.Children.Add(
					this.Game.GameView,
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
			}
			if (Settings.Taught) {
				this.AddHackyViews(mainLayout, hackyViews);
			}

			if (!desktop) {
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
			} else {
				backgroundLayout.Children.Add(
					mainLayout,
					Constraint.RelativeToParent(parent => {
						double width;
						if (parent.Width < App.MinDesktopWidth) {
							width = App.MinDesktopWidth;
						} else {
							double height;
							if ((parent.Height * App.MinDesktopRatio) > parent.Width) {
								height = parent.Width / App.MinDesktopRatio;
							} else {
								height = parent.Height;
							}
							width = height * App.MinDesktopRatio;
						}
						return parent.Width * 0.5 - width * 0.5;
					}),
					Constraint.RelativeToParent(parent => {
						if ((parent.Height * App.MinDesktopRatio) > parent.Width) {
							return parent.Height * 0.5 - parent.Width / App.MinDesktopRatio * 0.5;
						} else {
							return 0.0;
						}
					}),
					Constraint.RelativeToParent(parent => {
						double width;
						if (parent.Width < App.MinDesktopWidth) {
							width = App.MinDesktopWidth;
						} else {
							double height;
							if ((parent.Height * App.MinDesktopRatio) > parent.Width) {
								height = parent.Width / App.MinDesktopRatio;
							} else {
								height = parent.Height;
							}
							width = height * App.MinDesktopRatio;
						}
						return width;
					}),
					Constraint.RelativeToParent(parent => {
						if ((parent.Height * App.MinDesktopRatio) > parent.Width) {
							return parent.Width / App.MinDesktopRatio;
						} else {
							return parent.Height;
						}
					})
				);
			}

			this.Content = backgroundLayout;
		}

		protected override bool OnBackButtonPressed() {
			return !this.Game.OnBackButtonPressed() ? base.OnBackButtonPressed() : true;
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
			var menuLabel = new CLFormsLabel() {
				Text = App.Strings.Menu,
				TextColor = colour
			};
			var startLabel = new CLFormsLabel() {
				Text = App.Strings.Restart,
				TextColor = colour
			};
			var resultsLabel = new CLFormsLabel() {
				Text = App.Strings.Results,
				TextColor = colour
			};
			var nameEntry = new CLFormsEntry() {
				Text = "",
				Keyboard = Keyboard.Text,
//				IsVisible = false,
				LettersLimit = 11
			};
			return new View []{ tapFieldBoxView, bestScoreLabel, userScoreLabel, menuLabel, startLabel, resultsLabel, nameEntry };
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
					return parent.Width * 0.1111 * 2.0 + parent.Width * 0.1111 * 0.32f * App.Strings.Name.Length;
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
			case Device.WinPhone:
			case Device.Windows:
				fontFamily = "Assets/Fonts/PressStart2P.ttf#Press Start 2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.5;
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
			case Device.WinPhone:
			case Device.Windows:
				fontFamily = "Assets/Fonts/PressStart2P.ttf#Press Start 2P";
				namedSize = Device.GetNamedSize(NamedSize.Micro, typeof(Label)) * 0.5;
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
