#if __ANDROID__ || __IOS__
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using OpenTK;
using OpenTK.Graphics.ES20;

using ColorLinesNG2;

using Xamarin.Forms;

namespace CLRenderer {
	public class CLReViewEntity {
		public CLReViewEntity Next;
		public bool Removed { get; private set; }

		private float x, y;
		private float width, height;
		private View view;

		private CLReViewEntity() {}
		public CLReViewEntity(View view) {
			this.view = view;
			this.Removed = false;
		}
		public CLReViewEntity(View view, float x, float y, float width, float height) {
			this.view = view;
			this.x = x;
			this.y = y;
			this.width = width;
			this.height = height;
			this.Removed = false;
		}
		public void Render(RelativeLayout mainLayout, ColorLinesNG glView) {
			float appOpenGLWidth = glView.Right - glView.Left,
				appOpenGLHeight = glView.Top - glView.Bottom;
			double appNativeWidth = mainLayout.Width,
				appNativeHeight = mainLayout.Height;
			float xAbsOpenGL = (this.x - glView.Left) / appOpenGLWidth,
				yAbsOpenGL = (glView.Top - this.y) / appOpenGLHeight;
			double xAbsNative = xAbsOpenGL * appNativeWidth,
				yAbsNative = yAbsOpenGL * appNativeHeight;
			float x2AbsOpenGL = ((this.x+this.width) - glView.Left) / appOpenGLWidth,
				y2AbsOpenGL = (glView.Top - (this.y-this.height)) / appOpenGLHeight;
			double x2AbsNative = x2AbsOpenGL * appNativeWidth,
				y2AbsNative = y2AbsOpenGL * appNativeHeight;
			double widthAbsNative = x2AbsNative - xAbsNative,
				heightAbsNative = y2AbsNative - yAbsNative;
			Device.BeginInvokeOnMainThread(() => {
				mainLayout.Children.Add(
					this.view,
					Constraint.RelativeToParent(parent => {
						double left = 0.0;
						if (this.view is ICLForms) {
							left = (this.view as ICLForms).Padding.Left;
							left += (this.view as ICLForms).RelativePadding.Left * heightAbsNative;
						}
						return xAbsNative + left;
					}),
					Constraint.RelativeToParent(parent => {
						double top = 0.0;
						if (this.view is ICLForms) {
							top = (this.view as ICLForms).Padding.Top;
							top += (this.view as ICLForms).RelativePadding.Top * heightAbsNative;
						}
						return yAbsNative + top;
					}),
					Constraint.RelativeToParent(parent => {
						double left = 0.0, right = 0.0;
						if (this.view is ICLForms) {
							left = (this.view as ICLForms).Padding.Left;
							left += (this.view as ICLForms).RelativePadding.Left * heightAbsNative;
							right = (this.view as ICLForms).Padding.Right;
							right += (this.view as ICLForms).RelativePadding.Right * heightAbsNative;
						}
						return widthAbsNative - (left + right);
					}),
					Constraint.RelativeToParent(parent => {
						double bottom = 0.0, top = 0.0;
						if (this.view is ICLForms) {
							bottom = (this.view as ICLForms).Padding.Bottom;
							bottom += (this.view as ICLForms).RelativePadding.Bottom * heightAbsNative;
							top = (this.view as ICLForms).Padding.Top;
							top += (this.view as ICLForms).RelativePadding.Top * heightAbsNative;
						}
						return heightAbsNative - (bottom + top);
					})
				);
			});
/*			if (this.view is CLFormsEntry) {
				Task.Run(async () => {
					await Task.Delay(500);
					Device.BeginInvokeOnMainThread(() => {
						if (this.view != null && !this.view.IsFocused) {
							(this.view as CLFormsEntry).Focus();
						}
					});
				});
			}*/
		}
		public void Release(RelativeLayout mainLayout) {
			Device.BeginInvokeOnMainThread(() => {
				this.Removed = mainLayout.Children.Remove(this.view);
			});
		}
	}
	public class CLReEntity {
		public CLReEntity Next;

		public float []Verticies { get; private set; }
		public Color Fill { get; private set; }
		public float Angle { get; private set; }
		public bool Grayscale { get; private set; }

		private float []textureCoords;
		private int textureId;

		private BeginMode type;
		public BeginMode Type {
			get {
				return this.type;
			}
			set {
				if (value != BeginMode.TriangleStrip || value != BeginMode.TriangleFan)
					value = BeginMode.TriangleStrip;
			}
		}

		public CLReEntity(float []verticies, Color fill, int textureId = -1, float[] textureCoords = null, BeginMode type = BeginMode.TriangleStrip, bool grayscale = false, float angle = 0.0f) {
			this.Verticies = verticies;
			this.Fill = fill;
			this.type = type;
			this.textureId = textureId;
			this.Next = null;
			if (textureCoords == null)
				this.textureCoords = CLReEntity.defaultTextureCoords;
			else
				this.textureCoords = textureCoords;
			this.Grayscale = grayscale;
			this.Angle = angle;
		}
		private static byte []ColorToBytes(Color c, uint size) {
			byte []b = new byte[size*4];
			for (uint i = 0; i < size*4; i++) {
				if ((i&3) == 0) b[i] = (byte)(c.R * 255);
				else if ((i&3) == 1) b[i] = (byte)(c.G * 255);
				else if ((i&3) == 2) b[i] = (byte)(c.B * 255);
				else if ((i&3) == 3) b[i] = (byte)(c.A * 255);
			}
			return b;
		}
		private static float []ColorToFloats(Color c, uint size) {
			float []f = new float[size*4];
			for (uint i = 0; i < size*4; i++) {
				if ((i&3) == 0) f[i] = (float)c.R;
				else if ((i&3) == 1) f[i] = (float)c.G;
				else if ((i&3) == 2) f[i] = (float)c.B;
				else if ((i&3) == 3) f[i] = (float)c.A;
			}
			return f;
		}
		private static void Uniform4Color(int program, Color c) {
			GL.Uniform4(program, (float)c.R, (float)c.G, (float)c.B, (float)c.A);
		}
		private static float []VirtualToOpenGLCoords(float []verticies, ColorLinesNG glView, float angle) {
			int length = verticies.Length;
			float []ret = new float[length];
			for (int i = 0; i < length; i+=2) {
				float appVirtualWidth = glView.Right - glView.Left,
				appVirtualHeight = glView.Top - glView.Bottom;
				const float appOpenGLWidth = 2.0f,
					appOpenGLHeight = 2.0f;
				float xAbsVirtual = (verticies[i] - glView.Left) / appVirtualWidth,
					yAbsVirtual = (verticies[i+1] - glView.Bottom) / appVirtualHeight;
				float xAbsOpenGL = xAbsVirtual * appOpenGLWidth,
					yAbsOpenGL = yAbsVirtual * appOpenGLHeight;
				ret[i] = xAbsOpenGL-1.0f;
				ret[i+1] = yAbsOpenGL-1.0f;
			}
			//apply to squares only 4*2
			if (angle != 0.0f && length == 8) {
				angle *= toRadCoef;
				float s = (float)System.Math.Sin(-angle);
				float c = (float)System.Math.Cos(-angle);
				float wh = (ret[2]-ret[0])*0.5f;
				float hh = (ret[5]-ret[3])*0.5f;
				float x = ret[0]+wh;
				float y = ret[1]+hh;
				for (int i = 0; i < 4; i++) {
					ret[i*2] = x+(c*1.0f*r[i][0]*wh+-s*glView.Top*r[i][1]*hh);
					ret[i*2+1] = y+(s*glView.Ratio*r[i][0]*wh+c*1.0f*r[i][1]*hh);
				}
			}
			return ret;
		}
		public void Render(int []programs, ColorLinesNG glView) {
			if (this.textureId > 0) {
				int program;
				if (!this.Grayscale)
					program = programs[1];
				else
					program = programs[2];
				GL.UseProgram(program);

				var position = GL.GetAttribLocation(program, "position");
				var color = GL.GetAttribLocation(program, "color");
				var texCoord = GL.GetAttribLocation(program, "texCoord");
				var ourTexture = GL.GetUniformLocation(program, "ourTexture");
				
				GL.ActiveTexture(TextureUnit.Texture0);
				GL.BindTexture(TextureTarget.Texture2D, this.textureId);

				GL.EnableVertexAttribArray(position);
				GL.EnableVertexAttribArray(color);
				GL.EnableVertexAttribArray(texCoord);
				unsafe {
					fixed (float *pvertices = CLReEntity.VirtualToOpenGLCoords(this.Verticies, glView, this.Angle)) {
						GL.VertexAttribPointer(position, 2, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
					}
					fixed (float *pcolours = ColorToFloats(this.Fill, 4)) {
						GL.VertexAttribPointer(color, 4, VertexAttribPointerType.Float, false, 0, new IntPtr(pcolours));
					}
					fixed (float *ptexcoords = this.textureCoords) {
						GL.VertexAttribPointer(texCoord, 2, VertexAttribPointerType.Float, false, 0, new IntPtr(ptexcoords));
					}
				}
				GL.DrawArrays(BeginMode.TriangleStrip, 0, /*this.Verticies.Length/2*/4);

				GL.DisableVertexAttribArray(position);
				GL.DisableVertexAttribArray(color);
				GL.DisableVertexAttribArray(texCoord);
			} else {
				GL.UseProgram(programs[0]);

				var position = GL.GetAttribLocation(programs[0], "position");
				var ourColor = GL.GetUniformLocation(programs[0], "ourColor");

				Uniform4Color(ourColor, this.Fill);

				GL.EnableVertexAttribArray(position);
				unsafe {
					fixed (float *pvertices = CLReEntity.VirtualToOpenGLCoords(this.Verticies, glView, 0.0f)) {
						GL.VertexAttribPointer(position, 2, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
					}
				}
				if (this.Type == BeginMode.TriangleStrip)
					GL.DrawArrays(BeginMode.TriangleStrip, 0, 4);
				else
					GL.DrawArrays(BeginMode.TriangleFan, 0, (this.Verticies.Length)/2);

				GL.DisableVertexAttribArray(position);
			}
		}
		private static float []defaultTextureCoords = new float []{
			0.0f, 1.0f, //top left
			1.0f, 1.0f, //top right
			0.0f, 0.0f, //bottom left
			1.0f, 0.0f, //bottom right
		};
		//signs
		private static float [][]r = new float [][]{
			new float []{ -1.0f, -1.0f },
			new float []{ 1.0f, -1.0f },
			new float []{ -1.0f, 1.0f },
			new float []{ 1.0f, 1.0f },
		};
		private static float toRadCoef = (float)(System.Math.PI / 180.0);
	}
	public static class CLReDraw {
		public static readonly Color WhiteColor = Color.FromRgba(1.0, 1.0, 1.0, 1.0);
		//TODO: change amount of circle verticies depending on screen size/pixel density
		private const uint circleVerticies = 32;

		private static float[] PosResToVerticies(float x, float y, float width, float height) {
			return new float [8]{ x, y-height, x+width, y-height, x, y, x+width, y };
		}
		public static void Circle(float xCentre, float yCentre, float radius, Color colour) {
			float []verticies = new float[(circleVerticies+2)*2]; //+2 verticies for centre and the same end point as the start one
			verticies[0] = xCentre;
			verticies[1] = yCentre;
			for(int i = 0; i <= circleVerticies; i++){
				double angle = 2 * Math.PI * (double)i / circleVerticies;
				verticies[i*2+2] = xCentre + (float)Math.Cos(angle) * radius;
				verticies[i*2+3] = yCentre + (float)Math.Sin(angle) * radius;
			}
			CLReQueue.AddToQueue(new CLReEntity(verticies, colour, type: BeginMode.TriangleFan));
		}
		public static void Rect(float x, float y, float width, float height, Color colour) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), colour));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, float []textureCoords) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), CLReDraw.WhiteColor, textureId, textureCoords));
		}
		public static void Rect(float x, float y, float width, float height, int textureId) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), CLReDraw.WhiteColor, textureId));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, Color colour) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), colour, textureId));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, bool grayscale) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), CLReDraw.WhiteColor, textureId, grayscale: grayscale));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, Color colour, float angle) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), colour, textureId, angle: angle));
		}
		public static void View(View view, float x, float y, float width, float height) {
			CLReQueue.AddToQueue(new CLReViewEntity(view, x, y, width, height));
		}
		public static void ReleaseView(View view) {
			CLReQueue.AddToReleaseQueue(new CLReViewEntity(view));
		}
	}
	public class CLReQueue {
		private static CLReEntity rentities;
		private static CLReViewEntity ventities;
		private static CLReViewEntity ventitiesToRelease;

		public static void AddToQueue(CLReEntity rentity) {
			if (rentities == null) {
				rentities = rentity;
				return;
			}
			CLReEntity r = rentities;
			while (r.Next != null) r = r.Next;
			r.Next = rentity;
		}
		public static void AddToQueue(CLReViewEntity ventity) {
			if (ventities == null) {
				ventities = ventity;
				return;
			}
			CLReViewEntity v = ventities;
			while (v.Next != null) v = v.Next;
			v.Next = ventity;
		}
		public static void AddToReleaseQueue(CLReViewEntity ventity) {
			if (ventitiesToRelease == null) {
				ventitiesToRelease = ventity;
				return;
			}
			CLReViewEntity vre = ventitiesToRelease;
			while (vre.Next != null) vre = vre.Next;
			vre.Next = ventity;
		}

		private RelativeLayout mainLayout;
		private int []programs;
		private CLReQueue() {}
		public CLReQueue(RelativeLayout mainLayout, int []programs) {
			this.mainLayout = mainLayout;
			this.programs = programs;
		}
		public void Render(ColorLinesNG glView) {
			CLReEntity r = rentities;
			for (; r != null; r = r.Next) r.Render(this.programs, glView);
			CLReViewEntity v = ventities;
			for (; v != null; v = v.Next) v.Render(this.mainLayout, glView);
			CLReViewEntity vre = ventitiesToRelease;
			for (; vre != null; vre = vre.Next) vre.Release(this.mainLayout);
		}
		public void Clear() {
			ClearEntities();
			ClearViewEntities();
			ClearViewToReleaseEntities();
		}
		private static void ClearEntities() {
			CLReEntity next;
			while (rentities != null) {
				next = rentities.Next;
				rentities.Next = null;
				rentities = next;
			}
		}
		private static void ClearViewEntities() {
			CLReViewEntity next;
			while (ventities != null) {
				next = ventities.Next;
				ventities.Next = null;
				ventities = next;
			}
		}
		private static List<CLReViewEntity> notRemoved = new List<CLReViewEntity>();
		private static Stopwatch resetViewToReleaseTimer = new Stopwatch();
		private static int lastCount = 0;
		private static void ClearViewToReleaseEntities() {
			CLReViewEntity next;
			while (ventitiesToRelease != null) {
				if (!ventitiesToRelease.Removed) {
					notRemoved.Add(ventitiesToRelease);
				}
				next = ventitiesToRelease.Next;
				ventitiesToRelease.Next = null;
				ventitiesToRelease = next;
			}
			if (notRemoved.Count <= 0) {
				if (resetViewToReleaseTimer.IsRunning)
					resetViewToReleaseTimer.Stop();
				return;
			}
			if (lastCount != notRemoved.Count) {
				lastCount = notRemoved.Count;
				resetViewToReleaseTimer.Restart();
			}
			if (resetViewToReleaseTimer.ElapsedMilliseconds > 10000) {
				notRemoved.Clear();
				lastCount = 0;
				return;
			}
			next = ventitiesToRelease = notRemoved[0];
			for (int i = 1; i < notRemoved.Count; i++) {
				next = next.Next = notRemoved[i];
			}
			next.Next = null;
			notRemoved.Clear();
		}
	}
}
#endif