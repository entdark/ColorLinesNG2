using Xamarin.Forms;

using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Windows.UI;
using Windows.ApplicationModel.Core;

namespace ColorLinesNG2.UWP {
	public sealed partial class MainPage : IAudioManagerControl {
		public Canvas Control { get; set; }

		public MainPage() {
			this.InitializeComponent();

			if (Device.Idiom == TargetIdiom.Desktop) {
				var size = new Windows.Foundation.Size(ColorLinesNG2.App.MinDesktopWidth, ColorLinesNG2.App.MinDesktopHeight);
				ApplicationView.PreferredLaunchViewSize = size;
				ApplicationView.PreferredLaunchWindowingMode = ApplicationViewWindowingMode.PreferredLaunchViewSize;
				ApplicationView.GetForCurrentView().SetPreferredMinSize(size);
//				bool isFullScreenLast = false;
				this.SizeChanged += (sender, ev) => {
					var newSize = ev.NewSize;
					bool update = false;
					double width = newSize.Width, height = newSize.Height;
					if (width < size.Width) {
						update = true;
						width = size.Width;
					}
					if (height < size.Height) {
						update = true;
						height = size.Height;
					}
					var view = ApplicationView.GetForCurrentView();
					if (update)
						view.TryResizeView(new Windows.Foundation.Size(width, height));
					bool isFullScreen = view.AdjacentToLeftDisplayEdge && view.AdjacentToRightDisplayEdge;
//					if (isFullScreen != isFullScreenLast)
//						this.SetUpTitleBar(isFullScreen);
//					isFullScreenLast = isFullScreen;
				};
				this.SetUpTitleBar(false);
			}

			this.LoadApplication(new ColorLinesNG2.App());

			this.Control = this.Content as Canvas;
			if (this.Control == null)
				this.Control = new Canvas();
		}

		private void SetUpTitleBar(bool isFullScreen) {
//			if (isFullScreen)
//				ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
			isFullScreen = false;
			var coreTitleBar = CoreApplication.GetCurrentView().TitleBar;
			coreTitleBar.ExtendViewIntoTitleBar = isFullScreen;
			var titleBar = ApplicationView.GetForCurrentView().TitleBar;
			titleBar.BackgroundColor =
			titleBar.ButtonBackgroundColor = isFullScreen ? Colors.Transparent : Colors.Black;
			titleBar.ForegroundColor =
			titleBar.ButtonForegroundColor = new Windows.UI.Color() {
				R = 204,
				G = 204,
				B = 204,
				A = 255
			};
			titleBar.InactiveForegroundColor =
			titleBar.ButtonInactiveForegroundColor = new Windows.UI.Color() {
				R = 136,
				G = 136,
				B = 136,
				A = 255
			};
			titleBar.InactiveBackgroundColor =
			titleBar.ButtonInactiveBackgroundColor =
			titleBar.ButtonHoverBackgroundColor = isFullScreen ? Colors.Transparent :
			new Windows.UI.Color() {
				R = 26,
				G = 26,
				B = 26,
				A = 255
			};
			titleBar.ButtonPressedBackgroundColor = isFullScreen ? Colors.Transparent :
			new Windows.UI.Color() {
				R = 57,
				G = 57,
				B = 57,
				A = 255
			};
			titleBar.ButtonPressedForegroundColor =
			titleBar.ButtonHoverForegroundColor = Colors.White;
		}
	}
}
