using System;
using System.Collections.Generic;
using System.Reflection;

using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;

namespace ColorLinesNG2.UWP {
	/// <summary>
	/// Provides application-specific behavior to supplement the default Application class.
	/// </summary>
	sealed partial class App : Application {
		/// <summary>
		/// Initializes the singleton application object.  This is the first line of authored code
		/// executed, and as such is the logical equivalent of main() or WinMain().
		/// </summary>
		public App() {
			AppCenter.Start(APIKeys.AppCenterUWP, typeof(Analytics), typeof(Crashes));

			this.InitializeComponent();
			this.Suspending += OnSuspending;
		}

		/// <summary>
		/// Invoked when the application is launched normally by the end user.  Other entry points
		/// will be used such as when the application is launched to open a specific file.
		/// </summary>
		/// <param name="e">Details about the launch request and process.</param>
		protected override void OnLaunched(LaunchActivatedEventArgs ev) {

#if DEBUG
			if (System.Diagnostics.Debugger.IsAttached)
			{
				this.DebugSettings.EnableFrameRateCounter = true;
			}
#endif

			Frame rootFrame = Window.Current.Content as Frame;

			// Do not repeat app initialization when the Window already has content,
			// just ensure that the window is active
			if (rootFrame == null) {
				// Create a Frame to act as the navigation context and navigate to the first page
				rootFrame = new Frame();

				rootFrame.NavigationFailed += OnNavigationFailed;

				var assembliesToInclude = this.ReferenceAssemblies();
				Xamarin.Forms.Forms.Init(ev, assembliesToInclude);
				
				if (ev.PreviousExecutionState == ApplicationExecutionState.Terminated) {
					//TODO: Load state from previously suspended application
				}

				// Place the frame in the current Window
				Window.Current.Content = rootFrame;
			}

			if (rootFrame.Content == null) {
				// When the navigation stack isn't restored navigate to the first page,
				// configuring the new page by passing required information as a navigation
				// parameter
				rootFrame.Navigate(typeof(MainPage), ev.Arguments);
			}
			// Ensure the current window is active
			Window.Current.Activate();
		}

		/// <summary>
		/// Invoked when Navigation to a certain page fails
		/// </summary>
		/// <param name="sender">The Frame which failed navigation</param>
		/// <param name="e">Details about the navigation failure</param>
		void OnNavigationFailed(object sender, NavigationFailedEventArgs e) {
			throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
		}

		/// <summary>
		/// Invoked when application execution is being suspended.  Application state is saved
		/// without knowing whether the application will be terminated or resumed with the contents
		/// of memory still intact.
		/// </summary>
		/// <param name="sender">The source of the suspend request.</param>
		/// <param name="e">Details about the suspend request.</param>
		private void OnSuspending(object sender, SuspendingEventArgs e) {
			var deferral = e.SuspendingOperation.GetDeferral();
			//TODO: Save application state and stop any background activity
			deferral.Complete();
		}

		private List<Assembly> ReferenceAssemblies() {
			List<Assembly> assembliesToInclude = new List<Assembly>();
			assembliesToInclude.Add(typeof(Xamarin.Forms.Label).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Entry).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.RelativeLayout).GetTypeInfo().Assembly);
/*			assembliesToInclude.Add(typeof(Xamarin.Forms.StackLayout).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.View).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Color).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Keyboard).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Constraint).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.ContentPage).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Application).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Device).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.TargetIdiom).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Thickness).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.BoxView).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.NamedSize).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.TextAlignment).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.TapGestureRecognizer).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Easing).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Point).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Forms).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.BindableProperty).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Command).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Command<>).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.BindableObject).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.RoutingEffect).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.DependencyService).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Entry).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Entry).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(Xamarin.Forms.Entry).GetTypeInfo().Assembly);*/
			assembliesToInclude.Add(typeof(Plugin.Settings.CrossSettings).GetTypeInfo().Assembly);
/*			assembliesToInclude.Add(typeof(Plugin.Settings.Abstractions.ISettings).GetTypeInfo().Assembly);*/
			assembliesToInclude.Add(typeof(SkiaSharp.SKPaint).GetTypeInfo().Assembly);
/*			assembliesToInclude.Add(typeof(SkiaSharp.SKColor).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKBitmap).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKCanvas).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKSurface).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKColorFilter).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKManagedStream).GetTypeInfo().Assembly);
			assembliesToInclude.Add(typeof(SkiaSharp.SKBlendMode).GetTypeInfo().Assembly);*/
			assembliesToInclude.Add(typeof(SkiaSharp.Views.Forms.SKGLView).GetTypeInfo().Assembly);
			return assembliesToInclude;
		}
	}
}
