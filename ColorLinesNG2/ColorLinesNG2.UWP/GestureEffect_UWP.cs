//https://forums.xamarin.com/discussion/comment/253375/#Comment_253375
using System.ComponentModel;

using ColorLinesNG2.UWP;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using Windows.UI.Xaml;

[assembly: ResolutionGroupName("ColorLinesNG2")]
[assembly: ExportEffect(typeof(GesturePositionEffect), nameof(GesturePositionEffect))]
namespace ColorLinesNG2.UWP {
	public class GesturePositionEffect : PlatformEffect {
		private Command<Point> tapWithPositionCommand;
		protected override void OnAttached() {
			var control = this.GetControl();
			if (control != null) {
				control.Tapped += OnTapped;
			}
			OnElementPropertyChanged(new PropertyChangedEventArgs(string.Empty));
		}

		protected override void OnDetached() {
			var control = this.GetControl();
			if (control != null) {
				control.Tapped -= OnTapped;
			}
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs ev) {
			tapWithPositionCommand = Gesture.GetCommand(Element);
		}

		private void OnTapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs ev) {
			var tap = tapWithPositionCommand;
			if (tap != null) {
				var control = this.GetControl();
				var tapPoint = ev.GetPosition(control);
				double reX = tapPoint.X / control.RenderSize.Width;
				double reY = tapPoint.Y / control.RenderSize.Height;
				Point point = new Point(reX, reY);
				if (tap.CanExecute(point))
					tap.Execute(point);
			}
		}

		private UIElement GetControl() {
			if (Control != null)
				return Control;
			else
				return (this.Container as Windows.UI.Xaml.FrameworkElement);
		}
	}
}