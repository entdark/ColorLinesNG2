using System.Globalization;

namespace ColorLinesNG2 {
	public interface ILocalize {
		CultureInfo GetCurrentCultureInfo();
		void SetLocale(CultureInfo ci);
	}
}