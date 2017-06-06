using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;

using Xamarin.Forms;
using OpenTK.Graphics.ES20;

using CLRenderer;
using OpenTK;

#if __ANDROID__
using Android.Graphics;
using Android.Opengl;
#elif __IOS__
using CoreGraphics;
using UIKit;
#endif

namespace ColorLinesNG2 {
	public class ColorLinesNG {
		public float Bottom { get; private set; }
		public float Top { get; private set; }
		public float Left { get; private set; }
		public float Right { get; private set; }

		public OpenGLView GameView { get; private set; }
		public bool Sleeping { get; set; }
		private bool wentSleeping = false;

		private CLReQueue reQueue;
		private CLField field;

		private int []textureIds;

		private int []programs;
		private int simpleProgram, textureProgram;

		private int viewportWidth, viewportHeight;

		private Stopwatch time;

		public ColorLinesNG(RelativeLayout mainLayout, List<string> textures, View []hackyViews = null) {
			this.Bottom = -1.0f;
			this.Top = 1.0f;
			this.Left = -1.0f;
			this.Right = 1.0f;
			this.time = new Stopwatch();
			this.GameView = new OpenGLView() {
				HasRenderLoop = true
			};
			this.GameView.OnDisplay = (re) => {
				if (this.Sleeping) {
					if (!this.wentSleeping) {
						this.GameView.HasRenderLoop = false;
						this.wentSleeping = true;
						Debug.WriteLine("Flushed");
						GL.Flush();
					}
					return;
				} else if (this.wentSleeping) {
					this.wentSleeping = false;
				}
				if (this.field == null) {
					this.InitGL((int)re.Width, (int)re.Height, mainLayout, textures, hackyViews);
				}
				this.Render();
			};
			this.Sleeping = false;
		}

		private void Render() {
			this.reQueue.Clear();
			this.field.Draw(this.time.ElapsedMilliseconds);

			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			this.reQueue.Render(this);
		}

		private void InitGL(int width, int height, RelativeLayout mainLayout, List<string> textures, View []hackyViews = null) {
			this.viewportWidth = width;
			this.viewportHeight = height;
#region SIMPLE_PROGRAM
			string vertexShaderSrc = @"
							  attribute vec4 position;

							  void main()
							  {
								 gl_Position = position;
							  }";

			string fragmentShaderSrc = @"
									 precision mediump float;
									 uniform vec4 ourColor;

									 void main()
									 {
									   gl_FragColor = ourColor;
									 }";

			int vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSrc);
			int fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSrc);
			this.simpleProgram = GL.CreateProgram();
			if (this.simpleProgram == 0)
				throw new InvalidOperationException("Unable to create program");

			GL.AttachShader(this.simpleProgram, vertexShader);
			GL.AttachShader(this.simpleProgram, fragmentShader);
			
			LinkProgram(this.simpleProgram);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);
#endregion
#region TEXTURE_PROGRAM
			vertexShaderSrc = @"
							  attribute vec4 position;
							  attribute vec4 color;
							  attribute vec2 texCoord;
							  varying vec4 ourColor;
							  varying vec2 ourTexCoord;

							  void main()
							  {
								 gl_Position = position;
								 ourTexCoord = texCoord;
								 ourColor = color;
							  }";

			fragmentShaderSrc = @"
									 varying lowp vec4 ourColor;
									 varying lowp vec2 ourTexCoord;
									 uniform sampler2D ourTexture;

									 void main()
									 {
									   gl_FragColor = texture2D(ourTexture, ourTexCoord) * ourColor;
									 }";

			vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSrc);
			fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSrc);
			this.textureProgram = GL.CreateProgram();
			if (this.textureProgram == 0)
				throw new InvalidOperationException("Unable to create program");

			GL.AttachShader(this.textureProgram, vertexShader);
			GL.AttachShader(this.textureProgram, fragmentShader);
			
			LinkProgram(this.textureProgram);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);
#endregion
			this.programs = new int[] {
				this.simpleProgram,
				this.textureProgram
			};

			GL.Viewport(0, 0, this.viewportWidth, this.viewportHeight);

			this.SetScreenOpenGLCoords(this.viewportWidth, this.viewportHeight);

			this.textureIds = new int[textures.Count];

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);

			// create texture ids
			GL.GenTextures(textures.Count, this.textureIds);

			for (int i = 0; i < textures.Count; i++) {
				LoadTexture(textures[i], this.textureIds[i]);
			}
			
			this.field = new CLField(this.textureIds);
			this.field.InitVisuals(this.Left, this.Right, this.Bottom, this.Top, this.time.ElapsedMilliseconds, hackyViews);
			this.reQueue = new CLReQueue(mainLayout, this.programs);

			this.time.Start();
		}

		private void SetScreenOpenGLCoords(int w, int h) {
			if (w > h) {
				Top = 1.0f;
				Bottom = -Top;
				Right = (float)w / h;
				Left = -Right;
			} else {
				Top = (float)h / w;
				Bottom = -Top;
				Right = 1.0f;
				Left = -Right;
			}
		}

		private static int LoadShader(ShaderType type, string shader_source) {
			int shader = GL.CreateShader(type);
			if (shader == 0) {
				throw new InvalidOperationException("Unable to create shader");
			}

			int length = 0;
			GL.ShaderSource(shader, 1, new string[] { shader_source }, (int[])null);
			GL.CompileShader(shader);

			int compiled = 0;
			GL.GetShader(shader, ShaderParameter.CompileStatus, out compiled);
			if (compiled == 0) {
				length = 0;
				GL.GetShader(shader, ShaderParameter.InfoLogLength, out length);
				if (length > 0) {
					StringBuilder log = new StringBuilder(length);
					GL.GetShaderInfoLog(shader, length, out length, log);

					throw new InvalidOperationException("GL2 : Couldn't compile shader: " + log.ToString());
				}

				GL.DeleteShader(shader);
				throw new InvalidOperationException("Unable to compile shader of type : " + type.ToString());
			}

			return shader;
		}

		private static void LinkProgram(int program) {
			GL.LinkProgram(program);

			int linked;
			GL.GetProgram(program, ProgramParameter.LinkStatus, out linked);
			if (linked == 0) {
				// link failed
				int length = 0;
				GL.GetProgram(program, ProgramParameter.InfoLogLength, out length);
				if (length > 0) {
					var log = new StringBuilder(length);
					GL.GetProgramInfoLog(program, length, out length, log);
//					Log.Debug("GL2", "Couldn't link program: " + log.ToString());
				}

				GL.DeleteProgram(program);
				throw new InvalidOperationException("Unable to link program");
			}
		}

		private void RenderTest() {
/*			var vertices = new float[] {
					0.0f, 1.0f, 0.0f,
					-1.0f, 0.0f, 0.0f,
					1.0f, -1.0f, 0.0f
				};
			var vertices2 = new float[] {
					-1.0f, 0.5f, 0.0f,
					-0.5f, -1.0f, 0.0f,
					0.5f, 1.0f, 0.0f,
					0.5f, -0.5f, 0.0f
				};
			var vertices3 = new float[] {
					-0.5f, -0.5f, 0.0f,
					0.5f, -0.5f, 0.0f,
					-0.5f, 0.5f, 0.0f,
					0.5f, 0.5f, 0.0f
				};
			float[] defaultTextureCoords = {
					0, 1,
					1, 1,
					0, 0,
					1, 0,
				};
			float[] colours = {
					1.0f, 1.0f, 1.0f, 0.5f,
					1.0f, 1.0f, 1.0f, 0.5f,
					1.0f, 1.0f, 1.0f, 0.5f,
					1.0f, 1.0f, 1.0f, 0.5f,
				};*/

//			GL.UseProgram(simpleProgram);
/*			int vertexColor = GL.GetUniformLocation(simpleProgram, "ourColor");
			GL.Uniform4(vertexColor, 1.0f, 0.0f, 0.0f, 1.0f);

			// pin the data, so that GC doesn't move them, while used
			// by native code
			unsafe
			{
				fixed (float* pvertices = vertices2)
				{
					GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
					GL.EnableVertexAttribArray(0);
					GL.DrawArrays(BeginMode.TriangleFan, 0, vertices2.Length/3);
//					GL.Finish();
				}
			}
			vertexColor = GL.GetUniformLocation(simpleProgram, "ourColor");
			GL.Uniform4(vertexColor, 0.0f, 1.0f, 0.0f, 1.0f);

			// pin the data, so that GC doesn't move them, while used
			// by native code
			unsafe
			{
				fixed (float* pvertices = vertices)
				{
					GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
					GL.EnableVertexAttribArray(0);
					GL.DrawArrays(BeginMode.Triangles, 0, 3);
				}
			}*/

/*			GL.UseProgram(this.textureProgram);

			var mPositionHandle = GL.GetAttribLocation(this.textureProgram, "position");
			var color = GL.GetAttribLocation(this.textureProgram, "color");
			var mTextureCoordinatesHandle = GL.GetAttribLocation(this.textureProgram, "texCoord");
			var texture_sampler_handle = GL.GetUniformLocation(this.textureProgram, "ourTexture");

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, this.textureIds[0]);
//			GL.Uniform1(texture_sampler_handle, 0);

			GL.EnableVertexAttribArray(mPositionHandle);
			GL.EnableVertexAttribArray(color);
			GL.EnableVertexAttribArray(mTextureCoordinatesHandle);
			unsafe
			{
				fixed (float* pvertices = vertices2)
				{
					GL.VertexAttribPointer(mPositionHandle, 3, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
				}
				fixed (float* pcolours = colours)
				{
					GL.VertexAttribPointer(color, 4, VertexAttribPointerType.Float, false, 0, new IntPtr(pcolours));
				}
				fixed (float* ptexcoords = defaultTextureCoords)
				{
					GL.VertexAttribPointer(mTextureCoordinatesHandle, 2, VertexAttribPointerType.Float, false, 0, new IntPtr(ptexcoords));
				}
			}

			GL.DrawArrays(BeginMode.TriangleStrip, 0, vertices2.Length/3);
			
				GL.DisableVertexAttribArray(mPositionHandle);
				GL.DisableVertexAttribArray(mTextureCoordinatesHandle);

			GL.UseProgram(this.textureProgram);

			mPositionHandle = GL.GetAttribLocation(this.textureProgram, "position");
			mTextureCoordinatesHandle = GL.GetAttribLocation(this.textureProgram, "texCoord");
			texture_sampler_handle = GL.GetUniformLocation(this.textureProgram, "ourTexture");

			GL.ActiveTexture(TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2D, this.textureIds[1]);
//			GL.Uniform1(texture_sampler_handle, 0);

			GL.EnableVertexAttribArray(mPositionHandle);
			GL.EnableVertexAttribArray(mTextureCoordinatesHandle);
			unsafe
			{
				fixed (float* pvertices = vertices3)
				{
					GL.VertexAttribPointer(mPositionHandle, 3, VertexAttribPointerType.Float, false, 0, new IntPtr(pvertices));
				}
				fixed (float* ptexcoords = defaultTextureCoords)
				{
					GL.VertexAttribPointer(mTextureCoordinatesHandle, 2, VertexAttribPointerType.Float, false, 0, new IntPtr(ptexcoords));
				}
			}

			GL.DrawArrays(BeginMode.TriangleStrip, 0, vertices3.Length/3);
			
				GL.DisableVertexAttribArray(mPositionHandle);
				GL.DisableVertexAttribArray(mTextureCoordinatesHandle);*/
		}

		private static void UniformMatrix4(int location, Matrix4 value) {
			GL.UniformMatrix4(location, 1, false, ref value.Row0.X);
		}
		
		private static void LoadTexture(string texture, int texId) {
			var imageData = LoadEmbeddedResource(texture);
#if __ANDROID__
			Bitmap b = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
#elif __IOS__
			UIImage image = ImageFromByteArray(imageData);
			int width = (int)image.CGImage.Width;
			int height = (int)image.CGImage.Height;

			CGColorSpace colorSpace = CGColorSpace.CreateDeviceRGB();
			byte[] imageData2 = new byte[height * width * 4];
			CGContext context = new CGBitmapContext(imageData2, width, height, 8, 4 * width, colorSpace,
				CGBitmapFlags.PremultipliedLast | CGBitmapFlags.ByteOrder32Big);

			colorSpace.Dispose();
			context.ClearRect(new CGRect(0, 0, width, height));
			context.DrawImage(new CGRect(0, 0, width, height), image.CGImage);
#endif

			GL.BindTexture(TextureTarget.Texture2D, texId);

			// setup texture parameters
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)All.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)All.ClampToEdge);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)All.ClampToEdge);
			
#if __ANDROID__
			GLUtils.TexImage2D((int)All.Texture2D, 0, b, 0);
			b.Recycle();
#elif __IOS__
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, width, height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, imageData2);
#endif
		}
#if __IOS__
		private static UIKit.UIImage ImageFromByteArray(byte[] data) {
			if (data == null) {
				return null;
			}

			UIKit.UIImage image;
			try {
				image = new UIKit.UIImage(Foundation.NSData.FromArray(data));
			} catch (Exception e) {
				Console.WriteLine("Image load failed: " + e.Message);
				return null;
			}
			return image;
		}
#endif
		private static byte []LoadEmbeddedResource(string path) {
			string prefix;

#if __ANDROID__
			prefix = "ColorLinesNG2.Droid.";
#elif __IOS__
			prefix = "ColorLinesNG2.iOS.";
#endif

			var assembly = typeof(App).GetTypeInfo().Assembly;

/*			foreach (var res in assembly.GetManifestResourceNames()) {
				System.Diagnostics.Debug.WriteLine("found resource: " + res);
			}*/

			Stream stream = assembly.GetManifestResourceStream(prefix + path);
			byte[] data;

			using (MemoryStream ms = new MemoryStream()) {
				stream.CopyTo(ms);
				data = ms.ToArray();
			}
			return data;
		}
	}
}