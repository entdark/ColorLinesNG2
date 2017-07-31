using System.ComponentModel;
using System.Text.RegularExpressions;

using Xamarin.Forms;
using Xamarin.Forms.Platform.UWP;

using ColorLinesNG2;
using ColorLinesNG2.UWP;

using Windows.Graphics.Display;
using Windows.UI.ViewManagement;

[assembly: ExportRenderer(typeof(CLFormsEntry), typeof(CLFormsEntryRenderer_UWP))]
namespace ColorLinesNG2.UWP {
	public class CLFormsEntryRenderer_UWP : EntryRenderer {
		protected override void OnElementChanged(ElementChangedEventArgs<Entry> ev) {
			base.OnElementChanged(ev);

			if (this.Control == null) return;

			bool desktop = Device.Idiom == TargetIdiom.Desktop;

			this.Control.TextWrapping = Windows.UI.Xaml.TextWrapping.Wrap;
			this.Control.BorderThickness = new Windows.UI.Xaml.Thickness(0.0);
			if (desktop) {
				this.RecountDesktopPadding();
			} else {
				double w = ApplicationView.GetForCurrentView().VisibleBounds.Width *
					DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
				double top;
				if (w >= 1439.9) //1440
					top = 16.0;
				else if (w >= 1079.9) //1080
					top = 19.0;
				else if (w >= 767.9) //768
					top = 14.5;
				else if (w >= 719.9) //720
					top = 16.5;
				else //480
					top = 13.37;
				this.Control.Padding = new Windows.UI.Xaml.Thickness(0.0, top, 0.0, 0.0);
			}
			this.Control.VerticalContentAlignment = Windows.UI.Xaml.VerticalAlignment.Center;

			string lastText = null;
			this.Control.TextChanged += (sender, ev2) => {
				string text = this.Control.Text;
				if (text == lastText)
					return;
				lastText = text;
				string pattern = @"[.😃😊😞😉😁😂😋😈😇😆😅😄😌😍😎😏😐😒😜😚😘😖😔😓😝😠😡😢😣😤😭😫😪😩😨😥😰😱😲😳😵😶😷☺☹👀👂👃👄👅👆👇👈👉👊👋👌👍👎👏👐🙈🙉🙊🙅🙆🙇🙋🙌🙍🙎🙏☝✊✋✌❤💓💔💕💖💗💘💙💚💛💜💝💞💟👤👦👧👨👩👪👫👮👯👰👱👲👳👴👵👶👷💁💂💃🎈🎀🎁🎂🎃🎄🎅🎆🎇🎉🎊🎌🎍🎎🎏🎋🎐🎑🎒🎓💋💌💍💎💏💐💑💒👸👹👺👻👼👽👾👿💀🎽🎾🎿🏀🏁🏂🏃🏄🏆🏈🏊⚽⚾💄💅💆💇💈💉💊🃏🎠🎡🎢🎣🎤🎥🎦🎧🎨🎩🎪🎫🎬🎭🎮🎯🎰🎱🎲🎳🀄🎴🎵🎶🎷🎸🎹🎺🎻🎼📷📹📺📻📼♠♣♥♦🍕🍔🍖🍗🍘🍙🍚🍛🍜🍝🍞🍟🍠🍡🍢🍣🍤🍥🍦🍧🍨🍩🍪🍫🍬🍭🍮🍯🍰🍱🍲🍳🍴🍵🍶🍷🍸🍹🍺🍻☕🍅🍆🍇🍈🍉🍊🍌🍍🍎🍏🍑🍒🍓📝📞📟📠📡📢📣📤📥📦📧📨📩📪📫📮📰📱📲📳📴📶🔥🔦🔧🔨🔩🔪🔫🔮🔯🔱👑👒👓👔👕👖👗👘👙👚👛👜👝👞👟👠👡👢👣💺💻💼💽💾💿📀📁📂📃📄📅📆📇📈📉📊📋📌📍📎📏📐📑📒📓📔📕📖📗📘📙📚📛📜☎✂✉✏✒🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚🕛✈🚀🚃🚄🚅🚇🚓🚒🚑🚏🚌🚉🚕🚗🚙🚚🚢🚤🚥🚧🚨⛔🅿⭕🚭🚬🚫🚪🚩🔰🚲🚶🚹🚺🚻🚼⚡⚠♿🛀🚾🚽🏠🏡🏢🏣🏥🏦🏬🏫🏪🏩🏨🏧🏭🏮🏯🏰♨⚓⛽⛺⛵⛳⛲⛪⛅🌀🌁🌂🌃🌄🌅🌆🌇🌈🌉🌊🌋🌌🌑🌓🌔🌕🌙🌛🌟🌠☀☁☔⛄✨✳✴❄❇⭐🐌🐍🐎🐑🐒🐔🐗🐘🐙🐚🐛🐜🐝🐞🐟🐠🐡🐢🐣🐤🐥🐦🐧🐨🐩🐫🐬🐭🐯🐰🐱🐲🐳🐴🐵🐶🐷🐸🐹🐺🐻🐼🐽🐾😸😹😺😻😼😽😾😿🙀🌰🌱🌴🌵🌷🌸🌹🌺🌻🌼🌽🌾🌿🍀🍁🍂🍃🍄♻⁉‼❓❔❕❗☑✅✔❌❎➕➖✖➗©®™🌏🗻🗼🗽🗾🗿➰➿⤴⤵⬛⬜〰〽💠💡💢💣💤💥💦💧💨💩💪💫💬💮💯💰💱💲💳💴💵💸💹🔙🔚🔛🔜🔝↔↕↖↗↘↙↩↪➡⬅⬆⬇🔲🔳🔴🔵🔶🔷🔸🔹▪▫▶◀◻◼◽◾🔟ℹ🔞Ⓜ⚪⚫🉐🉑㊗㊙🅰🅱🅾🆎🆑🆒🆓🆔🆕🆖🆗🆘🆙🆚🈁🈂🈚🈯🈲🈳🈴🈵🈶🈷🈸🈹🈺🔃🔊🔋🔌🔍🔎🔏🔐🔑🔒🔓🔔🔖🔗🔘🔠🔡🔢🔣🔤🔺🔻🔼🔽⌚⌛⏩⏪⏫⏬⏰⏳♈♉♊♋♌♍♎♏♐♑♒♓⛎]";

				string cleanedText = Regex.Replace(text, pattern, "");

				if (cleanedText == text)
					return;
				this.Control.Text = cleanedText;
			};

			this.ResizeFont();
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs ev) {
			base.OnElementPropertyChanged(sender, ev);
			if (ev.PropertyName == Label.WidthProperty.PropertyName) {
				if (Device.Idiom == TargetIdiom.Desktop)
					this.RecountDesktopPadding();
				this.ResizeFont();
			}
		}

		private void RecountDesktopPadding() {
			double height = (this.Element.Parent as View).Height > 0.0 ?
				(this.Element.Parent as View).Height :
				ApplicationView.GetForCurrentView().VisibleBounds.Height;
			this.Control.Padding = new Windows.UI.Xaml.Thickness(0.0, height*0.024096, 0.0, 0.0);
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