using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xamarin.Forms;

using Android.Media;
using Android.OS;

[assembly: Dependency(typeof(ColorLinesNG2.Droid.AudioManager))]
namespace ColorLinesNG2.Droid {
	public class AudioManager : IAudioManager {
		public string SoundPath { get; set; } = "Sounds";

		private float backgroundMusicVolume = 1.0f;
		public float BackgroundMusicVolume {
			get {
				return this.backgroundMusicVolume;
			}
			set {
				this.backgroundMusicVolume = value;
				this.backgroundMusic?.SetVolume(this.backgroundMusicVolume, this.backgroundMusicVolume);
			}
		}
		
		private float soundsVolume = 1.0f;
		public float SoundsVolume {
			get { return this.soundsVolume; }
			set {
				this.soundsVolume = value;
				if (this.sounds.Any()) {
					foreach (var id in this.sounds.Values.ToList())
						this.soundPool.SetVolume(id, this.soundsVolume, this.soundsVolume);
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
				}
			}
		}

		private bool soundsEnabled = true;
		public bool SoundsEnabled {
			get { return this.soundsEnabled; }
			set {
				if (this.soundsEnabled != value) {
					this.soundsEnabled = value;
					if (!value && this.sounds.Any()) {
						foreach (var id in this.sounds.Values.ToList()) {
							this.soundPool.Stop(id);
						}
					}
				}
			}
		}

		private readonly Dictionary<string, int> sounds = new Dictionary<string, int>();
		private MediaPlayer backgroundMusic = null;
		private readonly SoundPool soundPool = null;
		private string backgroundSong = null;
		private bool backgroundMusicLoading = false;

		public AudioManager() {
			if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop) {
				var attributes = new AudioAttributes.Builder()
					.SetUsage(AudioUsageKind.Game)
					.SetContentType(AudioContentType.Music)
					.Build();
				this.soundPool = new SoundPool.Builder()
					.SetAudioAttributes(attributes)
					.SetMaxStreams(10)
					.Build();
			} else {
				this.soundPool = new SoundPool(10, Android.Media.Stream.Music, 0);
			}
		}
		
		public void DeactivateAudioSession() {
			this.soundPool.AutoPause();
			this.backgroundMusic?.Pause();
		}

		public void ReactivateAudioSession() {
			this.soundPool.AutoResume();
			this.RestartBackgroundMusic();
		}

		public void PlayBackgroundMusic(string filename) {
			if (!this.musicEnabled || this.backgroundMusicLoading)
				return;

			this.backgroundMusicLoading = true;

			this.StopBackgroundMusic();
			this.backgroundMusic?.Dispose();

			this.backgroundSong = filename;
			this.backgroundMusic = new MediaPlayer() {
				Looping = true
			};
			using (var file = Forms.Context.Assets.OpenFd(Path.Combine(SoundPath, filename))) {
				this.backgroundMusic.SetDataSource(file.FileDescriptor, file.StartOffset, file.Length);
			}
			this.backgroundMusic.SetVolume(BackgroundMusicVolume, BackgroundMusicVolume);
			this.backgroundMusic.Prepare();
			this.backgroundMusic.Start();

			this.backgroundMusicLoading = false;
		}

		public void StopBackgroundMusic() {
			if (this.backgroundMusic != null && this.backgroundMusic.IsPlaying) {
				this.backgroundMusic.Stop();
				this.backgroundMusic.Prepare();
			}
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
				this.PlayBackgroundMusic(this.backgroundSong);
			} else {
				this.backgroundMusic?.Start();
			}
		}

		public void PlaySound(string filename) {
			if (!this.soundsEnabled)
				return;
			this.PrecacheSound(filename);
			this.soundPool.Play(this.sounds[filename], this.soundsVolume, this.soundsVolume, 0, 0, 1.0f);
		}

		private int NewSound(string filename, int priority = 0) {
			if (!this.sounds.ContainsKey(filename)) {
				using (var file = Forms.Context.Assets.OpenFd(Path.Combine(this.SoundPath, filename))) {
					return this.soundPool.LoadAsync(file, priority).Result;
				}
			}
			return 0;
		}

		private void PrecacheSound(string filename) {
			if (!this.sounds.ContainsKey(filename)) {
				int id = this.NewSound(filename);
				if (id > 0)
					this.sounds[filename] = id;
			}
		}
		public void PrecacheSounds(string []filenames) {
			foreach (var filename in filenames) {
				this.PrecacheSound(filename);
			}
		}
	}
}