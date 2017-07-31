using System.ComponentModel;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using ColorLinesNG2;
using ColorLinesNG2.UWP;

using Windows.UI.ViewManagement;

[assembly: ExportRenderer(typeof(CLFormsLabel), typeof(CLFormsLabelRenderer_UWP))]
namespace ColorLinesNG2.UWP {
	public class CLFormsLabelRenderer_UWP : LabelRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Label> ev) {
			base.OnElementChanged(ev);
			this.ResizeFont();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);
			if (ev.PropertyName == Label.WidthProperty.PropertyName)
				this.ResizeFont();
		}

		private void ResizeFont() {
			if (this.Control == null) return;
			if (this.Element == null) return;
			double width = Device.Idiom == TargetIdiom.Desktop ?
				((this.Element.Parent as View).Height > 0.0 ?
				(this.Element.Parent as View).Height * ColorLinesNG2.App.MinDesktopRatio :
				ApplicationView.GetForCurrentView().VisibleBounds.Height * ColorLinesNG2.App.MinDesktopRatio) :
				ApplicationView.GetForCurrentView().VisibleBounds.Width;
			double fontSize = 11.75025 * width / 480.0 * (this.Element as ICLForms).TextScale;
			this.Control.FontSize = fontSize;
		}
	}
}