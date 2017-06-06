using System.ComponentModel;
using System;

using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

using ColorLinesNG2;
using ColorLinesNG2.Droid;

using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.Text;
using Java.Lang;

[assembly: ExportRenderer(typeof(CLFormsEntry), typeof(CLFormsEntryRenderer_Android))]
namespace ColorLinesNG2.Droid {
	public class CLFormsEntryRenderer_Android : EntryRenderer {
		private IInputFilter emojiInputFilter;
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> ev) {
			base.OnElementChanged(ev);

			if (this.Control == null) return;

			this.UpdateGravity();
			this.UpdatePadding();

			this.Control.SetIncludeFontPadding(true);

			//make the cursor colour inherit from the text colour
			IntPtr IntPtrtextViewClass = JNIEnv.FindClass(typeof(TextView));
			IntPtr mCursorDrawableResProperty = JNIEnv.GetFieldID(IntPtrtextViewClass, "mCursorDrawableRes", "I");
			JNIEnv.SetField(Control.Handle, mCursorDrawableResProperty, 0);

			var background = new GradientDrawable();
			background.SetStroke(0, global::Xamarin.Forms.Color.Transparent.ToAndroid());

			this.Control.SetBackground(background);

			if (this.emojiInputFilter == null)
				this.emojiInputFilter = new EmojiInputFilter();
			this.Control.SetFilters(new IInputFilter[] { this.emojiInputFilter });

			if (this.Element == null) return;

			float textSize = 12.0f / Resources.DisplayMetrics.Density * (float)Resources.DisplayMetrics.WidthPixels / 480.0f * (this.Element as ICLForms).TextScale;
			this.Control.TextSize = textSize;
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);

			if (this.Control == null) return;
			if (ev.PropertyName == Entry.HorizontalTextAlignmentProperty.PropertyName) {
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
				this.Control.SetPadding(2, 0, 0, 0);
			} else if (this.Element.HorizontalTextAlignment == global::Xamarin.Forms.TextAlignment.End) {
				this.Control.SetPadding(0, 0, 2, 0);
			}
		}

		private class EmojiInputFilter : Java.Lang.Object, IInputFilter {
			private Java.Lang.String emptyString = new Java.Lang.String("");
			public ICharSequence FilterFormatted(ICharSequence source, int start, int end, ISpanned dest, int dstart, int dend) {
				for (int i = start; i < end; i++) {
					int type = Character.GetType(source.CharAt(i));
					if (type == Character.Surrogate || type == Character.OtherSymbol) {
						return emptyString;
					}
				}
				return null;
			}
			protected override void Dispose(bool disposing) {
				this.emptyString.Dispose();
				base.Dispose(disposing);
			}
		}
	}
}