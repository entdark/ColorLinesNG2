using Xamarin.Forms;

using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;

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
					if (update)
						ApplicationView.GetForCurrentView().TryResizeView(new Windows.Foundation.Size(width, height));
				};
			}

			LoadApplication(new ColorLinesNG2.App());

			this.Control = this.Content as Canvas;
			if (this.Control == null)
				this.Control = new Canvas();
		}
	}
}
