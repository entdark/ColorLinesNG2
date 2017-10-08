using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

using SkiaSharp;

using ColorLinesNG2;

using Xamarin.Forms;

namespace CLRenderer {
	public class CLReViewEntity {
		public CLReViewEntity Next;
		public bool Removed { get; private set; }

		private float x, y;
		private float width, height;
		private View view;

		private CLReViewEntity() { }
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
		private float appSkiaWidth, appSkiaHeight;
		private double appNativeWidth, appNativeHeight;
		private float xAbsSkia, yAbsSkia;
		private double xAbsNative, yAbsNative;
		private float x2AbsSkia, y2AbsSkia;
		private double x2AbsNative, y2AbsNative;
		private double widthAbsNative, heightAbsNative;
		private void RecountCoordinates(RelativeLayout mainLayout, ColorLinesNG skiaView) {
			appSkiaWidth = skiaView.Right - skiaView.Left;
				appSkiaHeight = skiaView.Top - skiaView.Bottom;
			appNativeWidth = mainLayout.Width;
				appNativeHeight = mainLayout.Height;
			xAbsSkia = (this.x - skiaView.Left) / appSkiaWidth;
				yAbsSkia = (skiaView.Top - this.y) / appSkiaHeight;
			xAbsNative = xAbsSkia * appNativeWidth;
				yAbsNative = yAbsSkia * appNativeHeight;
			x2AbsSkia = ((this.x + this.width) - skiaView.Left) / appSkiaWidth;
				y2AbsSkia = (skiaView.Top - (this.y - this.height)) / appSkiaHeight;
			x2AbsNative = x2AbsSkia * appNativeWidth;
				y2AbsNative = y2AbsSkia * appNativeHeight;
			widthAbsNative = x2AbsNative - xAbsNative;
				heightAbsNative = y2AbsNative - yAbsNative;
		}
		public void Render(RelativeLayout mainLayout, ColorLinesNG skiaView) {
			if (Device.Idiom != TargetIdiom.Desktop)
				this.RecountCoordinates(mainLayout, skiaView);
			Device.BeginInvokeOnMainThread(() => {
				mainLayout.Children.Add(
					this.view,
					Constraint.RelativeToParent(parent => {
						if (Device.Idiom == TargetIdiom.Desktop)
							this.RecountCoordinates(mainLayout, skiaView);
						double left = 0.0;
						if (this.view is ICLForms) {
							left = (this.view as ICLForms).Padding.Left;
							left += (this.view as ICLForms).RelativePadding.Left * heightAbsNative;
						}
						return xAbsNative + left;
					}),
					Constraint.RelativeToParent(parent => {
						if (Device.Idiom == TargetIdiom.Desktop)
							this.RecountCoordinates(mainLayout, skiaView);
						double top = 0.0;
						if (this.view is ICLForms) {
							top = (this.view as ICLForms).Padding.Top;
							top += (this.view as ICLForms).RelativePadding.Top * heightAbsNative;
						}
						return yAbsNative + top;
					}),
					Constraint.RelativeToParent(parent => {
						if (Device.Idiom == TargetIdiom.Desktop)
							this.RecountCoordinates(mainLayout, skiaView);
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
						if (Device.Idiom == TargetIdiom.Desktop)
							this.RecountCoordinates(mainLayout, skiaView);
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
		public Color? Fill { get; private set; }
		public float Angle { get; private set; }
		public bool Grayscale { get; private set; }
		public int TextureId { get; private set; }

		private float []textureCoords;

		private CLReEntity() {}
		public CLReEntity(float []verticies, int textureId = -1, Color? fill = null, float []textureCoords = null, bool grayscale = false, float angle = 0.0f) {
			this.Verticies = verticies;
			this.TextureId = textureId;
			this.Fill = fill;
			this.textureCoords = textureCoords;
			this.Grayscale = grayscale;
			this.Angle = angle;
		}
		private static SKColor ColorToSKColor(Color c) {
			return new SKColor((byte)(c.R*255), (byte)(c.G*255), (byte)(c.B*255), (byte)(c.A*255));
		}
		private static SKRect VirtualToSkiaCoords(float []verticies, ColorLinesNG skiaView, float []textureCoords = null) {
			float []ret = new float[8];
			for (int i = 0; i < 8; i += 6) {
				float appVirtualWidth = skiaView.Right - skiaView.Left,
					appVirtualHeight = skiaView.Top - skiaView.Bottom;
				int appSkiaWidth = skiaView.Width,
					appSkiaHeight = skiaView.Height;
				float xAbsVirtual = (verticies[i] - skiaView.Left) / appVirtualWidth,
					yAbsVirtual = (skiaView.Top - verticies[i+1]) / appVirtualHeight;
				float xAbsSkia = xAbsVirtual * appSkiaWidth,
					yAbsSkia = yAbsVirtual * appSkiaHeight;
				ret[i] = skiaView.X + xAbsSkia;
				ret[i+1] = skiaView.Y + yAbsSkia;
			}
			//TODO: make proper clipping bounds
			if (textureCoords != null) {
				float cx = ret[0], cy = ret[1], w = ret[6]-cx, h = cy-ret[7];
				for (int i = 0; i < 8; i += 6) {
					ret[i] = cx + (0.0f+textureCoords[i])*w;
					ret[i+1] = cy - (1.0f-textureCoords[i+1])*h;
				}
			}
			return new SKRect(ret[0], ret[7], ret[6], ret[1]);
		}
		private static void CanvasDrawImage(SKCanvas canvas, SKImage image, SKRect dest, SKRect? src = null, SKPaint paint = null) {
			if (src == null)
				canvas.DrawImage(image, dest, paint);
			else
				canvas.DrawImage(image, (SKRect)src, dest, paint);
		}

		private static HashSet<int> loadingTextures = new HashSet<int>();
		private static bool loadAllTextures =
			Device.RuntimePlatform == Device.Android ||
			Device.RuntimePlatform == Device.iOS ||
			Device.Idiom == TargetIdiom.Desktop;
		public void Render(SKImage []images, ColorLinesNG skiaView, SKCanvas canvas) {
			var destRect = CLReEntity.VirtualToSkiaCoords(this.Verticies, skiaView);
			SKRect? srcRect = null;
//			if (this.textureCoords != null)
//				srcRect = CLReEntity.VirtualToSkiaCoords(this.Verticies, skiaView, this.textureCoords);

			if (this.Angle != 0.0f) {
				canvas.Save();
				canvas.RotateDegrees(this.Angle, destRect.MidX, destRect.MidY);
			}
			if (this.TextureId >= 0) {
				if (images[this.TextureId] == null) {
					if (loadingTextures.Count <= 0 && loadAllTextures) {
						if (!loadingTextures.Contains(this.TextureId)) {
							for (int i = 0; i < images.Length; i++) {
								if (images[i] == null)
									loadingTextures.Add(i);
							}
							Task.Run(async () => {
								for (int i = 0; i < images.Length; i++) {
									if (images[i] == null) {
										images[i] = await skiaView.LoadTexture(i);
										loadingTextures.Remove(i);
									}
								}
							});
						}
					} else if (!loadAllTextures) {
						if (!loadingTextures.Contains(this.TextureId)) {
							loadingTextures.Add(this.TextureId);
							Task.Run(async () => {
								images[this.TextureId] = await skiaView.LoadTexture(this.TextureId);
								loadingTextures.Remove(this.TextureId);
							});
						}
					}
					if (this.Angle != 0.0f) {
						canvas.Restore();
					}
					return;
				}
				if (this.Grayscale) {
					CLReEntity.CanvasDrawImage(canvas, images[this.TextureId], destRect, srcRect, paintGrayscale);
				} else if (this.Fill != null) {
					texturePaint.ColorFilter = SKColorFilter.CreateBlendMode(CLReEntity.ColorToSKColor((Color)this.Fill), SKBlendMode.Modulate);
					CLReEntity.CanvasDrawImage(canvas, images[this.TextureId], destRect, srcRect, texturePaint);
				} else {
					CLReEntity.CanvasDrawImage(canvas, images[this.TextureId], destRect);
				}
			} else {
				rectPaint.Color = CLReEntity.ColorToSKColor((Color)this.Fill);
				canvas.DrawRect(destRect, rectPaint);
			}
			if (this.Angle != 0.0f) {
				canvas.Restore();
			}
		}

		private static SKPaint paintGrayscale = new SKPaint() {
			ColorFilter = SKColorFilter.CreateColorMatrix(new float[20]{
				0.299f, 0.587f, 0.114f, 0.0f, 0.0f,
				0.299f, 0.587f, 0.114f, 0.0f, 0.0f,
				0.299f, 0.587f, 0.114f, 0.0f, 0.0f,
				0.0f, 0.0f, 0.0f, 1.0f, 0.0f,
			})
		};
		private static SKPaint rectPaint = new SKPaint();
		private static SKPaint texturePaint = new SKPaint();
	}
	public static class CLReDraw {
		public static readonly Color WhiteColor = Color.FromRgba(1.0, 1.0, 1.0, 1.0);

		private static float []PosResToVerticies(float x, float y, float width, float height) {
			return new float [8]{ x, y-height, x+width, y-height, x, y, x+width, y };
		}
		public static void Circle(float xCentre, float yCentre, float radius, Color colour) {
			//TODO
			throw new NotImplementedException();
		}
		public static void Rect(float x, float y, float width, float height, Color colour) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), -1, colour));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, float []textureCoords) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId, textureCoords: textureCoords));
		}
		public static void Rect(float x, float y, float width, float height, int textureId) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, Color colour) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId, colour));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, bool grayscale) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId, grayscale: grayscale));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, Color colour, float angle) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId, colour, angle: angle));
		}
		public static void Rect(float x, float y, float width, float height, int textureId, float angle) {
			CLReQueue.AddToQueue(new CLReEntity(CLReDraw.PosResToVerticies(x, y, width, height), textureId, angle: angle));
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
		private CLReQueue() {}
		private SKImage []images;
		private int []specialBgTextureIds;
		public CLReQueue(RelativeLayout mainLayout, SKImage []images, int []specialBgTextureIds) {
			this.mainLayout = mainLayout;
			this.images = images;
			this.specialBgTextureIds = specialBgTextureIds;
		}
		public void Render(ColorLinesNG skiaView, SKCanvas canvas) {
			CLReEntity r = rentities;
			int renderSpecialBgTexture = 0;
			for (; r != null; r = r.Next) {
				r.Render(this.images, skiaView, canvas);
				if (Device.Idiom != TargetIdiom.Desktop
					|| renderSpecialBgTexture != 0
					|| this.specialBgTextureIds == null
					|| this.specialBgTextureIds.Length < 2)
					continue;
				if (r.TextureId == this.specialBgTextureIds[0]) {
					renderSpecialBgTexture = 1;
				} else if (r.TextureId == this.specialBgTextureIds[1]) {
					renderSpecialBgTexture = 2;
				}
			}
			if (renderSpecialBgTexture == 1) {
				CLReQueue.RenderFadingCircle(canvas, skiaView);
			} else if (renderSpecialBgTexture == 2) {
				CLReQueue.RenderFades(canvas, skiaView, false);
				CLReQueue.RenderFades(canvas, skiaView, true);
				CLReQueue.RenderFadingCircle(canvas, skiaView);
			}
			CLReViewEntity v = ventities;
			for (; v != null; v = v.Next) v.Render(this.mainLayout, skiaView);
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

		private const float wc = 1.337f;
		private static readonly SKPaint fadePaint = new SKPaint();
		private static readonly SKColor []fadingGradient3 = new SKColor[3]{
			new SKColor(0, 0, 0, 255),
			new SKColor(0, 0, 0, 166),
			new SKColor(0, 0, 0, 0)
		};
		private static readonly float []fadingPos3 = new float[3]{
			0.72f,
			0.89f,
			1.0f
		};
		private static void RenderFades(SKCanvas canvas, ColorLinesNG skiaView, bool right) {
			float width = skiaView.Width * wc;
			float xoffset, pxs, pxe;
			if (!right) {
				xoffset = skiaView.X - width;
				pxs = xoffset;
				pxe = skiaView.X;
			} else {
				xoffset = skiaView.X + skiaView.Width;
				pxs = xoffset + width;
				pxe = xoffset;
			}
			var fadeRect = SKRect.Create(xoffset, skiaView.Y, width, skiaView.Height);
			using (var shader = SKShader.CreateLinearGradient(
				new SKPoint(pxs, fadeRect.MidY),
				new SKPoint(pxe, fadeRect.MidY),
				fadingGradient3,
				fadingPos3,
				SKShaderTileMode.Clamp
			)) {
				fadePaint.Shader = shader;
				canvas.DrawRect(fadeRect, fadePaint);
			}
		}
		private static readonly SKPaint cutPaint = new SKPaint() {
			Color = new SKColor(0, 0, 0, 255),
			Style = SKPaintStyle.Fill
		};
		private static void RenderCuts(SKCanvas canvas, ColorLinesNG skiaView) {
			float x = skiaView.X - skiaView.Width * wc;
			float midy = skiaView.Y + skiaView.Height * 0.5f;
			float y = midy - skiaView.Width * (0.5f + wc);
			float width = skiaView.X + skiaView.Width * (1.0f + wc) - x;
			float height = skiaView.Y - y;
			var topRect = SKRect.Create(x, y, width, height);
			y = skiaView.Y + skiaView.Height;
			var bottomRect = SKRect.Create(x, y, width, height);
			canvas.DrawRect(topRect, cutPaint);
			canvas.DrawRect(bottomRect, cutPaint);
		}
		private static readonly SKColor []fadingGradient2 = new SKColor[2]{
			new SKColor(0, 0, 0, 0),
			new SKColor(0, 0, 0, 255)
		};
		private static readonly float []fadingPos2 = new float[2]{
			0.57f,
			0.66f
		};
		private static void RenderFadingCircle(SKCanvas canvas, ColorLinesNG skiaView) {
			float midx = skiaView.X + skiaView.Width * 0.5f;
			float midy = skiaView.Y + skiaView.Height * 0.5f;
			float x = skiaView.X - skiaView.Width * wc;
			float y = midy - skiaView.Width * (0.5f + wc);
			float width = skiaView.X + skiaView.Width * (1.0f + wc) - x;
			float height = midy + skiaView.Width * (1.0f + wc) - y;
			var fadeRect = SKRect.Create(x, y, width, height);
			using (var shader = SKShader.CreateRadialGradient(
				new SKPoint(midx, midy),
				width * 0.5f,
				fadingGradient2,
				fadingPos2,
				SKShaderTileMode.Clamp
			)) {
				fadePaint.Shader = shader;
				canvas.DrawRect(fadeRect, fadePaint);
			}
		}
	}
}
 