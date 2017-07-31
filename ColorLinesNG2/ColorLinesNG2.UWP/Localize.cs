using System.Globalization;

using Xamarin.Forms;

[assembly: Dependency(typeof(ColorLinesNG2.UWP.Localize))]
namespace ColorLinesNG2.UWP {
	public class Localize : ColorLinesNG2.ILocalize {
		public void SetLocale(CultureInfo ci) {
		}
		public CultureInfo GetCurrentCultureInfo() {
			return CultureInfo.CurrentUICulture;
		}
	}
}