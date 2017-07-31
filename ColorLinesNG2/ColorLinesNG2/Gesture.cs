//https://forums.xamarin.com/discussion/comment/253375/#Comment_253375
using System.Linq;

using Xamarin.Forms;

namespace ColorLinesNG2 {
	public static class Gesture {
		public static readonly BindableProperty TappedProperty = BindableProperty.CreateAttached("Tapped", typeof(Command<Point>), typeof(Gesture), null, propertyChanged: CommandChanged);

		public static Command<Point> GetCommand(BindableObject view) {
			return (Command<Point>)view.GetValue(TappedProperty);
		}
		public static void SetTapped(BindableObject view, Command<Point> value) {
			view.SetValue(TappedProperty, value);
		}

		private static void CommandChanged(BindableObject bindable, object oldValue, object newValue) {
			var view = bindable as View;
			if (view != null) {
				var effect = GetOrCreateEffect(view);
			}
		}
		private static GestureEffect GetOrCreateEffect(View view) {
			var effect = (GestureEffect)view.Effects.FirstOrDefault(ev => ev is GestureEffect);
			if (effect == null) {
				effect = new GestureEffect();
				view.Effects.Add(effect);
			}
			return effect;
		}

		private class GestureEffect : RoutingEffect {
			public GestureEffect() : base("ColorLinesNG2.GesturePositionEffect") {
			}
		}
	}
}