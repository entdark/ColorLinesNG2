using System;

namespace CLAnimations {
	public class CLAnim {
		private CLAnim next = null;
		private bool parallel = false;
		public Func<long, bool, bool> Execute;
		public CLAnim(long duration, Func<long, long, bool, bool> anim, bool parallel = false) {
			bool firstCall = true;
			long start = 0, end = 0;
			this.Execute = (thisTime, checkTime) => {
				if (firstCall) {
					start = thisTime;
					end = thisTime + duration;
					firstCall = false;
				}
				return anim(start, end, checkTime);
			};
			this.parallel = parallel;
		}
		private static CLAnim anims;
		public static void AddToQueue(CLAnim animAdd) {
			if (CLAnim.anims == null) {
				CLAnim.anims = animAdd;
				return;
			}
			CLAnim anim = CLAnim.anims;
			while (anim.next != null) anim = anim.next;
			anim.next = animAdd;
		}
		public static void AddToTop(CLAnim animAdd, bool nextParallel = false) {
			animAdd.next = CLAnim.anims;
			CLAnim.anims = animAdd;
			if (nextParallel && CLAnim.anims.next != null) {
				CLAnim.anims.next.parallel = true;
			}
		}
		public static bool CheckTimes(long thisTime) {
			return CLAnim.ExecQueue(thisTime, true);
		}
		public static bool ExecQueue(long thisTime) {
			return CLAnim.ExecQueue(thisTime, false);
		}
		private static bool ExecQueue(long thisTime, bool checkTime) {
			if (CLAnim.anims == null)
				return false;
			CLAnim anim = CLAnim.anims;
			while (anim != null) {
				if (CLAnim.anims == anim || anim.parallel) {
					if (anim.Execute(thisTime, checkTime)) {
						CLAnim.RemoveAnim(anim);
					}
				}
				anim = anim.next;
			}
			if (CLAnim.anims == null)
				return false;
			return true;
		}
		private static void RemoveAnim(CLAnim animRemove) {
			CLAnim anim = CLAnim.anims, animPrev = null;
			for(;anim!=null && anim != animRemove;animPrev=anim,anim=anim.next);
			if (anim == null)
				return;
			if (animPrev != null) {
				animPrev.next = anim.next;
			} else {
				CLAnim.anims = anim.next;
			}
		}
		//source: http://gizma.com/easing
		public static float Linear(float delta, float duration, float value = 1.0f) {
			return delta / duration * value;
		}
		public static float EaseInSin(float delta, float duration, float value = 1.0f) {
			return (float)Math.Cos(delta/duration * (Math.PI*0.5)) * -value + value;
		}
		public static float EaseOutSin(float delta, float duration, float value = 1.0f) {
			return (float)Math.Sin(delta/duration * (Math.PI*0.5)) * value;
		}
		public static float EaseInCubic(float delta, float duration, float value = 1.0f) {
			delta /= duration;
			return delta * delta * delta * value;
		}
		public static float EaseOutCubic(float delta, float duration, float value = 1.0f) {
			delta /= duration;
			delta -= 1.0f;
			return (delta * delta * delta + 1.0f) * value;
		}
		public static float EaseInOutCubic(float delta, float duration, float value = 1.0f) {
			delta /= duration*0.5f;
			if (delta < 1.0f) return delta * delta * delta * value * 0.5f;
			delta -= 2.0f;
			return (delta * delta * delta + 2.0f) * value * 0.5f;
		}
		public static float EaseInQuad(float delta, float duration, float value = 1.0f) {
			delta /= duration;
			return delta * delta * value;
		}
		public static float EaseOutQuad(float delta, float duration, float value = 1.0f) {
			delta /= duration;
			return delta * (delta - 2.0f) * -value;
		}
		public static float EaseInOutQuad(float delta, float duration, float value = 1.0f) {
			delta /= duration*0.5f;
			if (delta < 1.0f) return delta * delta * value * 0.5f;
			delta -= 1.0f;
			return (delta * (delta - 2.0f) - 1.0f) * -value * 0.5f;
		}
		public static float Jump(float delta, float duration, float value = 1.0f) {
			if (delta > duration) {
				return 1.0f * value;
			}
			if (delta > (duration * 0.5f)) {
				return 1.0f-EaseOutCubic(delta-duration*0.5f, duration * 0.5f, value);
			} else {
				return EaseOutCubic(delta, duration * 0.5f, value);
			}
		}
	}
}