using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using CLDataTypes;
using CLRenderer;

#if __ANDROID__
using OpenTK;
using OpenTK.Graphics.ES20;
using Android.Graphics;
using Android.Opengl;
#elif __IOS__
using OpenTK;
using OpenTK.Graphics.ES20;
using CoreGraphics;
using UIKit;
#else //UWP
using SkiaSharp;
using SkiaSharp.Views.Forms;
#endif

namespace ColorLinesNG2 {
	public class ColorLinesNG {
		public float Bottom { get; private set; }
		public float Top { get; private set; }
		public float Left { get; private set; }
		public float Right { get; private set; }
		public float Ratio { get; private set; }
		public int X { get; private set; }
		public int Y { get; private set; }
		public int Width { get; private set; }
		public int Height { get; private set; }

		public bool Sleeping { get; set; }
#if __ANDROID__ || __IOS__
		public OpenGLView GameView { get; private set; }

		private bool wentSleeping = false;
#else //UWP
		public SKGLView GameView { get; private set; }
#endif
		private CLReQueue reQueue;
		private CLField field;

		private int []textureIds;
#if __ANDROID__ || __IOS__
		private int []programs;
		private int simpleProgram, textureProgram, textureGrayscaleProgram;
#else //UWP
		private SKImage []images;
#endif
		private Stopwatch time;

		public ColorLinesNG(RelativeLayout mainLayout, View []hackyViews = null) {
			this.Bottom = -1.0f;
			this.Top = 1.0f;
			this.Left = -1.0f;
			this.Right = 1.0f;
			this.time = new Stopwatch();
#if __ANDROID__ || __IOS__
			this.GameView = new OpenGLView() {
				HasRenderLoop = true
			};
			this.GameView.OnDisplay = (re) => {
				if (this.Sleeping) {
					if (!this.wentSleeping) {
						this.GameView.HasRenderLoop = false;
						this.wentSleeping = true;
						Debug.WriteLine("Flushed");
//						GL.Flush();
					}
					return;
				} else if (this.wentSleeping) {
					this.wentSleeping = false;
				}
				if (this.field == null) {
					this.InitGL((int)re.Width, (int)re.Height, mainLayout, hackyViews);
				}
				this.Render();
			};
			this.Sleeping = false;
#else //UWP
			this.GameView = new SKGLView() {
				HasRenderLoop = true,
				EnableTouchEvents = false
			};
			this.GameView.PaintSurface += (sender, ev) => {
				if (Device.Idiom == TargetIdiom.Desktop) {
					int width = ev.RenderTarget.Width;
					int height = ev.RenderTarget.Height;
					if ((height * App.MinDesktopRatio) > width) {
						this.Width = width;
						this.Height = (int)(width / App.MinDesktopRatio);
					} else {
						this.Width = (int)(height * App.MinDesktopRatio);
						this.Height = height;
					}
					this.X = (int)(width * 0.5f - this.Width * 0.5f);
					this.Y = (int)(height * 0.5f - this.Height * 0.5f);
				} else {
					this.X = 0;
					this.Y = 0;
					this.Width = ev.RenderTarget.Width;
					this.Height = ev.RenderTarget.Height;
				}
				if (this.field == null) {
					this.InitGL(mainLayout, hackyViews);
				}
				this.Render(ev.Surface.Canvas);
			};
#endif
		}

		public bool OnBackButtonPressed() {
			return this.field.OnBackButtonPressed();
		}

#if __ANDROID__ || __IOS__
		private void Render() {
			this.reQueue.Clear();
			this.field.Draw(this.time.ElapsedMilliseconds);

			GL.ClearColor(0.0f, 0.0f, 0.0f, 1.0f);
			GL.Clear(ClearBufferMask.ColorBufferBit);

			this.reQueue.Render(this);
		}
#else //UWP
		private static readonly SKColor bgColor = new SKColor(0, 0, 0, 255);
		private void Render(SKCanvas canvas) {
			this.reQueue.Clear();
			this.field.Draw(this.time.ElapsedMilliseconds);

			canvas.Clear(bgColor);

			this.reQueue.Render(this, canvas);
		}
#endif

#if __ANDROID__ || __IOS__
		private void InitGL(int width, int height, RelativeLayout mainLayout, View []hackyViews = null) {
			this.Width = width;
			this.Height = height;
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
#region TEXTURE_GRAYSCALE_PROGRAM
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
									   lowp vec4 color;
									   lowp float grayscale;
									   color = texture2D(ourTexture, ourTexCoord) * ourColor;
									   grayscale = 0.299 * color.r + 0.587 * color.g + 0.114 * color.b;
									   gl_FragColor = vec4(grayscale, grayscale, grayscale, color.a);
									 }";

			vertexShader = LoadShader(ShaderType.VertexShader, vertexShaderSrc);
			fragmentShader = LoadShader(ShaderType.FragmentShader, fragmentShaderSrc);
			this.textureGrayscaleProgram = GL.CreateProgram();
			if (this.textureGrayscaleProgram == 0)
				throw new InvalidOperationException("Unable to create program");

			GL.AttachShader(this.textureGrayscaleProgram, vertexShader);
			GL.AttachShader(this.textureGrayscaleProgram, fragmentShader);
			
			LinkProgram(this.textureGrayscaleProgram);

			GL.DeleteShader(vertexShader);
			GL.DeleteShader(fragmentShader);
#endregion
			this.programs = new int[] {
				this.simpleProgram,
				this.textureProgram,
				this.textureGrayscaleProgram
			};

			GL.Viewport(0, 0, this.viewportWidth, this.viewportHeight);

			this.SetScreenOpenGLCoords(this.Width, this.Height);

			GL.Enable(EnableCap.CullFace);
			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
#else //UWP
		private void InitGL(RelativeLayout mainLayout, View []hackyViews = null) {
			float textureScale = this.Width * 0.00087f;

			this.SetScreenOpenGLCoords(this.Width, this.Height);

			var textureIdsDouble = new int[embeddedTextures.Length][];
			int i = 0, size = 0;
			foreach (var texturesArray in embeddedTextures) {
				textureIdsDouble[i] = new int[texturesArray.Length];
				int j = 0;
				foreach (var texture in texturesArray) {
					embeddedTextures[i][j].Name = string.Format("CLNG_{0}.png", texture.Name);
					j++;
					size++;
				}
				i++;
			}
			this.textureIds = new int[size];
#if __ANDROID__ || __IOS__
			// create texture ids
			GL.GenTextures(size, this.textureIds);
#else //UWP
			this.images = new SKImage[size];
			int []specialBgTextureIds = new int[2];
#endif
			for (i = 0; i < size; i++) {
				int j = 0, sum = 0;
				foreach (var texturesArray in embeddedTextures) {
					int k = 0;
					foreach (var texture in texturesArray) {
						if (i == sum) {
#if __ANDROID__ || __IOS__
							LoadTexture(textures[j][k], this.textureIds[i]);
							textureIdsDouble[j][k] = this.textureIds[i];
#else //UWP
							if (/*!embeddedTextures[j][k].InitIgnore || */Device.Idiom == TargetIdiom.Desktop) {
								this.images[i] = LoadTexture(embeddedTextures[j][k], textureScale);
							}
							textureIdsDouble[j][k] = i;
							if (embeddedTextures[j][k].Name.Equals("CLNG_Stars.png")) {
								specialBgTextureIds[0] = i;
							} else if (embeddedTextures[j][k].Name.Equals("CLNG_Nebula.png")) {
								specialBgTextureIds[1] = i;
							}
#endif
						} else if (i < sum) {
							break;
						}
						k++;
						sum++;
					}
					j++;
				}
			}
#endif
			this.field = new CLField(textureIdsDouble);
			this.field.InitVisuals(this.Left, this.Right, this.Bottom, this.Top, this.time.ElapsedMilliseconds, hackyViews);
#if __ANDROID__ || __IOS__
			this.reQueue = new CLReQueue(mainLayout, this.programs);
#else //UWP
			this.reQueue = new CLReQueue(mainLayout, this.images, specialBgTextureIds);
#endif

			this.time.Start();
		}

		private void SetScreenOpenGLCoords(int w, int h) {
			if (w > h) {
				Top = 1.0f;
				Bottom = -Top;
				Right = (float)w / h;
				Left = -Right;
				Ratio = (float)h / w;
			} else {
				Top = (float)h / w;
				Bottom = -Top;
				Right = 1.0f;
				Left = -Right;
				Ratio = (float)w / h;
			}
		}
#if __ANDROID__ || __IOS__
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
		}*/
#endif
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

		/*		private static void UniformMatrix4(int location, Matrix4 value) {
					GL.UniformMatrix4(location, 1, false, ref value.Row0.X);
				}*/
#if __ANDROID__ || __IOS__
		private static void LoadTexture(string texture, int texId) {
			byte []imageData;
			using (var stream = LoadEmbeddedResource(texture)) {
				using (MemoryStream ms = new MemoryStream()) {
					stream.CopyTo(ms);
					imageData = ms.ToArray();
				}
			}
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
#else // UWP
		public Task<SKImage> LoadTexture(int textureId) {
			float textureScale = this.Width * 0.00087f;
			int size = this.images.Length;
			int i = 0, sum = 0;
			foreach (var texturesArray in embeddedTextures) {
				int j = 0;
				foreach (var texture in texturesArray) {
					if (textureId == sum) {
						return Task.FromResult(LoadTexture(embeddedTextures[i][j], textureScale));
					} else if (textureId < sum) {
						break;
					}
					j++;
					sum++;
				}
				i++;
			}
			return null;
		}
		private static SKImage LoadTexture(TextureData texture, float textureScale) {
			using (var stream = LoadEmbeddedResource(texture.Name))
			using (var skStream = new SKManagedStream(stream))
			using (var skBitmap = SKBitmap.Decode(skStream)) {
				//decrease size up to 4.2 times!
				if (Device.Idiom != TargetIdiom.Desktop) {
					var info = skBitmap.Info;
					info.Width = (int)(texture.Width * textureScale);
					info.Height = (int)(texture.Height * textureScale);
					using (var skBitmapResized = skBitmap.Resize(info, SKBitmapResizeMethod.Triangle))
						return SKImage.FromBitmap(skBitmapResized);
				} else {
					return SKImage.FromBitmap(skBitmap);
				}
			}
		}
#endif
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
		private static Stream LoadEmbeddedResource(string path) {
			string prefix;
#if __ANDROID__
			prefix = "ColorLinesNG2.Droid.Resources.";
#elif __IOS__
			prefix = "ColorLinesNG2.iOS.Resources.";
#else //UWP
            prefix = "ColorLinesNG2.UWP.Resources.";
#endif
            var assembly = typeof(App).GetTypeInfo().Assembly;

/*			foreach (var res in assembly.GetManifestResourceNames()) {
				System.Diagnostics.Debug.WriteLine("found resource: " + res);
			}*/

			return assembly.GetManifestResourceStream(prefix + path);
		}

		private struct TextureData {
			public string Name { get; set; }
			public int Width { get; set; }
			public int Height { get; set; }
			public bool InitIgnore { get; set; }
		}

		private static readonly TextureData [][]embeddedTextures = new TextureData[(int)CLTextures.CLMax][]{
			new TextureData[(int)CLLabelSize.CLMax]{
				new TextureData() { Name="Cell", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="LabelMicro", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="LabelSmall", Width=384, Height=128, InitIgnore=false },
				new TextureData() { Name="LabelMedium", Width=640, Height=128, InitIgnore=false },
				new TextureData() { Name="LabelLong8", Width=1024, Height=128, InitIgnore=true },
				new TextureData() { Name="LabelLong", Width=1152, Height=128, InitIgnore=true },
				new TextureData() { Name="LabelLarge", Width=896, Height=896, InitIgnore=false },
			},
			new TextureData[(int)CLColour.CLMax-1]{
				new TextureData() { Name="Red", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Yellow", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Green", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Cyan", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Blue", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Pink", Width=128, Height=128, InitIgnore=false },
				new TextureData() { Name="Brown", Width=128, Height=128, InitIgnore=false },
			},
			new TextureData[(int)CLColour.CLMax-1]{
				new TextureData() { Name="Magenta", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Orange", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Turquoise", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="CyanDark", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Purple", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Black", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="White", Width=128, Height=128, InitIgnore=true },
			},
			new TextureData[(int)CLColour.CLMax-1]{
				new TextureData() { Name="Sakura", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Peach", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Lime", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Mint", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Blueberry", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Grape", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="Chocolate", Width=128, Height=128, InitIgnore=true },
			},
			new TextureData[(int)CLAchievements.CLMax]{
				new TextureData() { Name="Achievement10", Width=256, Height=256, InitIgnore=true },
				new TextureData() { Name="Achievement13", Width=256, Height=256, InitIgnore=true },
				new TextureData() { Name="Achievement500", Width=256, Height=256, InitIgnore=true },
				new TextureData() { Name="Achievement1000", Width=256, Height=256, InitIgnore=true },
			},
			new TextureData[(int)CLBackgroundTextures.CLMax]{
				new TextureData() { Name="Stars", Width=2304, Height=2304, InitIgnore=true },
				new TextureData() { Name="Nebula", Width=2304, Height=2304, InitIgnore=true },
				new TextureData() { Name="DefaultSmall", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="StarsSmall", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="NebulaSmall", Width=128, Height=128, InitIgnore=true },
				new TextureData() { Name="DefaultPreview", Width=384, Height=384, InitIgnore=true },
				new TextureData() { Name="StarsPreview", Width=384, Height=384, InitIgnore=true },
				new TextureData() { Name="NebulaPreview", Width=384, Height=384, InitIgnore=true },
			}
		};
	}
}