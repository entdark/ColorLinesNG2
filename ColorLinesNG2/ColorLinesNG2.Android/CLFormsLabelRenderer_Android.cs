using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using ColorLinesNG2;
using ColorLinesNG2.Droid;

using Android.Views;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CLFormsLabel), typeof(CLFormsLabelRenderer_Android))]
namespace ColorLinesNG2.Droid {
	public class CLFormsLabelRenderer_Android : LabelRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Label> ev) {
			base.OnElementChanged(ev);

			if (this.Control == null) return;

			this.UpdateGravity();
			this.UpdatePadding();

			this.Control.SetIncludeFontPadding(true);

			if (this.Element == null) return;

			float textSize = 11.7f / Resources.DisplayMetrics.Density * (float)Resources.DisplayMetrics.WidthPixels / 480.0f * (this.Element as ICLForms).TextScale;
			this.Control.TextSize = textSize;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);

			if (this.Control == null) return;
			if (ev.PropertyName == Label.HorizontalTextAlignmentProperty.PropertyName) {
				this.UpdateGravity();
			}
		}

		private void UpdateGravity() {
			this.Control.Gravity |= GravityFlags.CenterVertical;
			this.Control.Gravity &= ~GravityFlags.Top;
			this.Control.Gravity &= ~GravityFlags.Bottom;
		}
		private void UpdatePadding() {
			if (this.Element == null) {
				return;
			}
			if (this.Element.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Center) {
				this.Control.SetPadding(0, 0, 0, 0);
			} else if (this.Element.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.Start) {
				this.Control.SetPadding(0/*19*/, 0, 0, 0);
			} else if (this.Element.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.End) {
				this.Control.SetPadding(0, 0, 0/*19*/, 0);
			}
		}
	}
}