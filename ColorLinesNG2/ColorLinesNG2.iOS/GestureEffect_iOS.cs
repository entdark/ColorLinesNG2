//https://forums.xamarin.com/discussion/comment/253375/#Comment_253375
using System;
using System.ComponentModel;

using ColorLinesNG2.iOS;

using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using UIKit;

[assembly: ResolutionGroupName("ColorLinesNG2")]
[assembly: ExportEffect(typeof(GesturePositionEffect), nameof(GesturePositionEffect))]
namespace ColorLinesNG2.iOS {
	internal class GesturePositionEffect : PlatformEffect {
		private readonly UITapGestureRecognizer tapDetector;
		private Command<Point> tapWithPositionCommand;

		public GesturePositionEffect() {
			tapDetector = CreateTapRecognizer(() => tapWithPositionCommand); ;
		}

		private UITapGestureRecognizer CreateTapRecognizer(Func<Command<Point>> getCommand) {
			return new UITapGestureRecognizer(() => {
				var handler = getCommand();
				if (handler != null) {
					var control = Control ?? Container;
					var tapPoint = tapDetector.LocationInView(control);
					float relativeX = (float)(tapPoint.X / control.Bounds.Size.Width);
					float relativeY = (float)(tapPoint.Y / control.Bounds.Size.Height);
					var point = new Point(relativeX, relativeY);

					if (handler.CanExecute(point) == true)
						handler.Execute(point);
				}
			}) {
				Enabled = false,
				ShouldRecognizeSimultaneously = (recognizer, gestureRecognizer) => true,
			};
		}

		protected override void OnElementPropertyChanged(PropertyChangedEventArgs args) {
			tapWithPositionCommand = Gesture.GetCommand(Element);
		}

		protected override void OnAttached() {
			var control = Control ?? Container;

			control.AddGestureRecognizer(tapDetector);
			tapDetector.Enabled = true;

			OnElementPropertyChanged(new PropertyChangedEventArgs(String.Empty));
		}

		protected override void OnDetached() {
			var control = Control ?? Container;
			tapDetector.Enabled = false;
			control.RemoveGestureRecognizer(tapDetector);
		}
	}
}