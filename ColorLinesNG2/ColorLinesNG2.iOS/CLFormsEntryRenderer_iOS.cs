using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

using ColorLinesNG2;
using ColorLinesNG2.iOS;

using UIKit;
using System.ComponentModel;

[assembly: ExportRenderer(typeof(CLFormsEntry), typeof(CLFormsEntryRenderer_iOS))]
namespace ColorLinesNG2.iOS {
	public class CLFormsEntryRenderer_iOS : EntryRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> e) {
			base.OnElementChanged(e);

			if (this.Control == null) return;

			this.Control.BorderStyle = UITextBorderStyle.None;
			this.Control.Layer.BorderWidth = 0.0f;
			this.Control.Layer.BorderColor = Color.Transparent.ToCGColor();

			this.Control.ShouldChangeCharacters = (textField, range, replacement) => {
				if (textField.IsFirstResponder) {
					if (textField.TextInputMode == null
					|| textField.TextInputMode.PrimaryLanguage == null
					|| textField.TextInputMode.PrimaryLanguage == "emoji")
						return false;
				}
				return true;
			};

			if (this.Element == null) return;

			this.Control.TintColor = this.Element.TextColor.ToUIColor();
			this.SetFontSize();
		}
		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);
			if (ev.PropertyName == Entry.TextColorProperty.PropertyName) {
				if (this.Element != null)
					this.Control.TintColor = this.Element.TextColor.ToUIColor();
			}
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

