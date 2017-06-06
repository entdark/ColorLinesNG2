using System;

using CLDataTypes;

namespace CLTutorialRoutine {
	public class CLTutorial {
		public bool Done { get; private set; }
		public bool Applied { get; private set; }

		private string line;
		private Action action;
		private CLCell []addBalls;

		private CLTutorial() {}
		public CLTutorial(string line, Action action, CLCell []addBalls = null) {
			this.line = line;
			this.Applied = false;
			this.Done = false;
			this.action = action;
			this.addBalls = addBalls;
		}

		private static CLTutorial current = null;
		public static string Apply(CLTutorial tutorial, bool unDone = false) {
			if (tutorial != null && !tutorial.Applied) {
				tutorial.Applied = true;
				tutorial.action();
			}
			if (CLTutorial.current != tutorial && tutorial != null && (!tutorial.Done || unDone)) {
				if (CLTutorial.current != null)
					CLTutorial.current.Done = true;
				CLCell []balls = CLTutorial.current?.addBalls;
				CLTutorial.current = tutorial;
				if (unDone) {
					CLTutorial.current.Done = false;
					if (CLTutorial.current.addBalls == null)
						CLTutorial.current.addBalls = balls;
				}
			}
			if (tutorial == null) {
				CLTutorial.current = null;
				return null;
			}
			return CLTutorial.current.line;
		}
		public static CLCell []GetAddBalls() {
			CLCell []balls = null;
			if (CLTutorial.current != null && CLTutorial.current.addBalls != null) {
				balls = CLTutorial.current.addBalls;
				CLTutorial.current.addBalls = null;
				return balls;
			}
			return null;
		}
	}
}