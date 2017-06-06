using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using ColorLinesNG2;
using ColorLinesNG2.iOS;

using UIKit;
using CoreGraphics;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CLFormsLabel), typeof(CLFormsLabelRenderer_iOS))]
namespace ColorLinesNG2.iOS {
	public class CLFormsLabelRenderer_iOS : LabelRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Label> e) {
			base.OnElementChanged(e);

			if (this.Control == null) return;
			
			this.Control.Layer.BorderWidth = 0.0f;
			this.Control.Layer.BorderColor = Color.Transparent.ToCGColor();

			if (this.Element == null) return;

			this.SetFontSize();
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);
			//a lot of properties can reset the font size so we do that magic here all the time
			this.SetFontSize();
		}

		private void SetFontSize() {
			if (this.Control == null) return;
			if (this.Element == null) return;

			float textSize = 10.08f * (float)UIScreen.MainScreen.Bounds.Size.Width / 414.0f * (this.Element as ICLForms).TextScale;
			this.Control.Font = this.Control.Font.WithSize(textSize);
		}
	}
}

