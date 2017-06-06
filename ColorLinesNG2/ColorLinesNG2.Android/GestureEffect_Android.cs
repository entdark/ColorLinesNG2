//https://forums.xamarin.com/discussion/comment/253375/#Comment_253375
using System;
using System.ComponentModel;

using ColorLinesNG2.Droid;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using Android.Views;

[assembly: ResolutionGroupName("ColorLinesNG2")]
[assembly: ExportEffect(typeof(GesturePositionEffect), nameof(GesturePositionEffect))]
namespace ColorLinesNG2.Droid {
	public class GesturePositionEffect : PlatformEffect {
		private Command<Point> tapWithPositionCommand;
		private Action<MotionEvent> tapAction { get; set; }

		public GesturePositionEffect() {
			tapAction = motionEvent => {
				var tap = tapWithPositionCommand;
				if (tap != null) {
					var control = Control ?? Container;
					float x = motionEvent.GetX();
					float y = motionEvent.GetY();
					float reX = x / control.Width;
					float reY = y / control.Height;
					var point = new Point(reX, reY);
					if (tap.CanExecute(point))
						tap.Execute(point);
				}
			};
		}
		
		protected override void OnElementPropertyChanged(PropertyChangedEventArgs ev) {
			tapWithPositionCommand = Gesture.GetCommand(Element);
		}
		protected override void OnAttached() {
			var control = Control ?? Container;
			control.Touch += ControlOnTouch;
			OnElementPropertyChanged(new PropertyChangedEventArgs(string.Empty));
		}
		protected override void OnDetached() {
			var control = Control ?? Container;
			control.Touch -= ControlOnTouch;
		}

		private void ControlOnTouch(object sender, Android.Views.View.TouchEventArgs touchEventArgs) {
			if (touchEventArgs.Event.Action == MotionEventActions.Up) {
				tapAction?.Invoke(touchEventArgs.Event);
			}
		}
	}
}