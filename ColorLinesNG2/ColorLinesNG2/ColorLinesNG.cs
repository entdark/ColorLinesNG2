using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;

using CLDataTypes;
using CLRenderer;

using SkiaSharp;
using SkiaSharp.Views.Forms;

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
		public SKGLView GameView { get; private set; }

		private CLReQueue reQueue;
		private CLField field;

		private int []textureIds;
		private static SKImage []images;
		private Stopwatch time;

		public ColorLinesNG(RelativeLayout mainLayout, View []hackyViews = null) {
			this.Bottom = -1.0f;
			this.Top = 1.0f;
			this.Left = -1.0f;
			this.Right = 1.0f;
			this.time = new Stopwatch();
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
					this.Init(mainLayout, hackyViews);
				}
				this.Render(ev.Surface.Canvas);
			};
		}

		public bool OnBackButtonPressed() {
			if (this.field == null)
				return true;
			return this.field.OnBackButtonPressed();
		}

		private static readonly SKColor bgColor = new SKColor(0, 0, 0, 255);
		private void Render(SKCanvas canvas) {
			this.reQueue.Clear();
			this.field.Draw(this.time.ElapsedMilliseconds);

			canvas.Clear(bgColor);

			this.reQueue.Render(this, canvas);
		}

		private void Init(RelativeLayout mainLayout, View []hackyViews = null) {
			float textureScale = this.Width * 0.00087f;

			this.SetScreenVirtualCoords(this.Width, this.Height);

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
			//when the app is restarted (Android only for now)
			//then we cannot reload embedded resources
			//so we store them static and won't reload afterward
			if (ColorLinesNG.images == null) {
				ColorLinesNG.images = new SKImage[size];
			}
			int []specialBgTextureIds = new int[2];
			for (i = 0; i < size; i++) {
				int j = 0, sum = 0;
				foreach (var texturesArray in embeddedTextures) {
					int k = 0;
					foreach (var texture in texturesArray) {
						if (i == sum) {
							textureIdsDouble[j][k] = i;
							if (embeddedTextures[j][k].Name.Equals("CLNG_Stars.png")) {
								specialBgTextureIds[0] = i;
							} else if (embeddedTextures[j][k].Name.Equals("CLNG_Nebula.png")) {
								specialBgTextureIds[1] = i;
							}
						} else if (i < sum) {
							break;
						}
						k++;
						sum++;
					}
					j++;
				}
			}
			this.field = new CLField(textureIdsDouble);
			this.field.InitVisuals(this.Left, this.Right, this.Bottom, this.Top, this.time.ElapsedMilliseconds, hackyViews);
			this.reQueue = new CLReQueue(mainLayout, ColorLinesNG.images, specialBgTextureIds);

			this.time.Start();
		}

		private void SetScreenVirtualCoords(int w, int h) {
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

		public Task<SKImage> LoadTexture(int textureId) {
			float textureScale = this.Width * 0.00087f;
			int size = ColorLinesNG.images.Length;
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