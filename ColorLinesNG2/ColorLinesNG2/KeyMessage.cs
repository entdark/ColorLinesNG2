using CLDataTypes;

namespace ColorLinesNG2 {
	public class KeyMessage {
		public const string Pressed = "KeyPressed";
		public const string Inactive = "KeysInactive";

		public CLKey Key { get; set; }
		public bool IsCtrlPressed { get; set; }

		public KeyMessage() {}
		public KeyMessage(CLKey key, bool isCtrlPressed) {
			this.Key = key;
			this.IsCtrlPressed = isCtrlPressed;
		}
	}
}