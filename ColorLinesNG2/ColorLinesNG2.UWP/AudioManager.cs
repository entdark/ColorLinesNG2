using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Xamarin.Forms;

[assembly: Dependency(typeof(ColorLinesNG2.UWP.AudioManager))]
namespace ColorLinesNG2.UWP {
	public class AudioManager : IAudioManager {
		public string SoundPath { get; set; } = "Sounds";

		private float backgroundMusicVolume = 1.0f;
		public float BackgroundMusicVolume {
			get {
				return this.backgroundMusicVolume;
			}
			set {
				this.backgroundMusicVolume = value;
				if (this.backgroundMusic != null) {
					Device.BeginInvokeOnMainThread(() => {
						this.backgroundMusic.Volume = this.backgroundMusicVolume;
					});
				}
			}
		}

		private float soundsVolume = 1.0f;
		public float SoundsVolume {
			get { return this.soundsVolume; }
			set {
				this.soundsVolume = value;
				if (this.sounds.Any()) {
					foreach (var so in this.sounds.Values.ToList())
						so.Volume = this.soundsVolume;
				}
			}
		}

		private bool musicEnabled = true;
		public bool MusicEnabled {
			get { return this.musicEnabled; }
			set {
				if (this.musicEnabled != value) {
					this.musicEnabled = value;
					if (!value)
						this.StopBackgroundMusic();
					else
						this.RestartBackgroundMusic(true);
				} else if (value) {
					this.RestartBackgroundMusic(true);
				}
			}
		}

		private bool soundsEnabled = true;
		public bool SoundsEnabled {
			get { return this.soundsEnabled; }
			set {
				if (this.soundsEnabled != value) {
					this.soundsEnabled = value;
					if (!value && this.sounds.Any())
						foreach (var so in this.sounds.Values.ToList()) {
							so.Stop();
					}
				}
			}
		}
		
		private readonly Canvas control;
		private readonly Dictionary<string, MediaElement> sounds = new Dictionary<string, MediaElement>();
		private MediaElement backgroundMusic = null;
		private string backgroundSong = null;
		
		public AudioManager() {
			var audioManagerControl = ((Windows.UI.Xaml.Controls.Frame)Window.Current.Content).Content as IAudioManagerControl;
			this.control = audioManagerControl != null ? audioManagerControl.Control : new Canvas();
		}
		
		public void DeactivateAudioSession() {
			Device.BeginInvokeOnMainThread(() => {
				this.backgroundMusic?.Pause();
			});
		}

		public void ReactivateAudioSession() {
			this.RestartBackgroundMusic();
		}

		public void PlayBackgroundMusic(string filename) {
			this.PlayBackgroundMusic(filename, false);
		}
		private void PlayBackgroundMusic(string filename, bool autoPlay) {
			if (!this.musicEnabled)
				return;

			if (this.backgroundMusic != null) {
				this.StopBackgroundMusic();
				this.control.Children.Remove(backgroundMusic);
			}

			this.backgroundSong = filename;
			Task.Run(async () => {
				var newSound = await this.NewSound(this.backgroundSong);
				Device.BeginInvokeOnMainThread(() => {
					this.backgroundMusic = new MediaElement {
						Volume = this.backgroundMusicVolume,
						IsLooping = true,
						AutoPlay = autoPlay,
						Visibility = Visibility.Collapsed
					};
					this.backgroundMusic.SetSource(newSound.Item1, newSound.Item2);
					this.control.Children.Add(this.backgroundMusic);
					this.backgroundMusic.Play();
				});
			});
		}

		public void StopBackgroundMusic() {
			Device.BeginInvokeOnMainThread(() => {
				this.backgroundMusic?.Stop();
			});
		}

		public void FlushBackgroundMusic() {
			this.backgroundSong = null;
			this.StopBackgroundMusic();
		}

		public void RestartBackgroundMusic(bool restart = false) {
			if (!this.musicEnabled)
				return;
			if (this.backgroundSong == null)
				return;
			if (restart) {
				this.PlayBackgroundMusic(this.backgroundSong, true);
			} else {
				Device.BeginInvokeOnMainThread(() => {
					this.backgroundMusic?.Play();
				});
			}
		}

		public void PlaySound(string filename) {
			if (!this.soundsEnabled)
				return;
			Task.Run(async () => {
				await this.PrecacheSound(filename);
				Device.BeginInvokeOnMainThread(() => {
					this.sounds[filename].Play();
				});
			});
		}

		private async Task<Tuple<IRandomAccessStream, string>> NewSound(string filename) {
			try {
				StorageFolder folder = await Package.Current.InstalledLocation.GetFolderAsync("Assets");
				folder = await folder.GetFolderAsync(this.SoundPath);
				StorageFile file = await folder.GetFileAsync(filename);
				var stream = await file.OpenAsync(FileAccessMode.Read);
				return new Tuple<IRandomAccessStream, string>(stream, file.ContentType);
			} catch (Exception exception) {
				System.Diagnostics.Debug.WriteLine(exception);
				return null;
			}
		}

		private async Task PrecacheSound(string filename) {
			if (!this.sounds.ContainsKey(filename)) {
				var newSound = await this.NewSound(filename);
				Device.BeginInvokeOnMainThread(() => {
					var sound = new MediaElement {
						Volume = this.soundsVolume,
						IsLooping = false,
						AutoPlay = false,
						Visibility = Visibility.Collapsed
					};
					sound.SetSource(newSound.Item1, newSound.Item2);
					this.sounds[filename] = sound;
					this.control.Children.Add(sound);
				});
			}
		}
		public void PrecacheSounds(string []filenames) {
			Task.Run(async () => {
				foreach (var filename in filenames) {
					await this.PrecacheSound(filename);
				}
			});
		}
	}
}