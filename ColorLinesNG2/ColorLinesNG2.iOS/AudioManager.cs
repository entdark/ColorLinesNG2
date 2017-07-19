using System.Collections.Generic;
using System.IO;
using System.Linq;

using Xamarin.Forms;

using AVFoundation;
using Foundation;

[assembly: Dependency(typeof(ColorLinesNG2.iOS.AudioManager))]
namespace ColorLinesNG2.iOS {
	public class AudioManager : IAudioManager {
		public string SoundPath { get; set; } = "Sounds";

		private float backgroundMusicVolume = 1.0f;
		public float BackgroundMusicVolume {
			get {
				return this.backgroundMusicVolume;
			}
			set {
				this.backgroundMusicVolume = value;
				if (this.backgroundMusic != null)
					this.backgroundMusic.Volume = backgroundMusicVolume;
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
						foreach (var so in this.sounds.Values.ToList()) {
							so.Stop();
							so.PrepareToPlay();
						}
					}
				}
			}
		}

		private readonly Dictionary<string, AVAudioPlayer> sounds = new Dictionary<string, AVAudioPlayer>();
		private AVAudioPlayer backgroundMusic = null;
		private string backgroundSong = null;
		private bool backgroundMusicLoading = false;

		public AudioManager() {
			var session = AVAudioSession.SharedInstance();
			session.SetCategory(AVAudioSessionCategory.Ambient);
			session.SetActive(true);
		}
		public void DeactivateAudioSession() {
			this.backgroundMusic?.Pause();
			var session = AVAudioSession.SharedInstance();
			session.SetActive(false);
		}

		public void ReactivateAudioSession() {
			var session = AVAudioSession.SharedInstance();
			session.SetActive(true);
			this.RestartBackgroundMusic();
		}

		public void PlayBackgroundMusic(string filename) {
			if (!this.musicEnabled || this.backgroundMusicLoading)
				return;

			this.backgroundMusicLoading = true;

			this.StopBackgroundMusic();
			this.backgroundMusic?.Dispose();
			this.backgroundSong = filename;
			this.backgroundMusic = this.NewSound(this.backgroundSong, this.backgroundMusicVolume, true);
			this.backgroundMusic.Play();

			this.backgroundMusicLoading = false;
		}

		public void StopBackgroundMusic() {
			this.backgroundMusic?.Stop();
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
				this.backgroundMusic?.Play();
			}
		}

		public void PlaySound(string filename) {
			if (!this.soundsEnabled)
				return;
			this.PrecacheSound(filename);
			this.sounds[filename].Play();
		}

		private AVAudioPlayer NewSound(string filename, float defaultVolume, bool isLooping = false) {
			using (var songUrl = new NSUrl(Path.Combine(this.SoundPath, filename))) {
				NSError err;
				var fileType = filename.Split('.').Last();
				var sound = new AVAudioPlayer(songUrl, fileType, out err) {
					Volume = defaultVolume,
					NumberOfLoops = isLooping ? -1 : 0
				};
				sound.PrepareToPlay();

				sound.FinishedPlaying += (sender, ev) => {
					var se = sender as AVAudioPlayer;
					if (se != this.backgroundMusic)
						se.PrepareToPlay();
				};
				
				err?.Dispose();
				return sound;
			}
		}

		private void PrecacheSound(string filename) {
			if (!this.sounds.ContainsKey(filename)) {
				this.sounds[filename] = this.NewSound(filename, this.soundsVolume);
			}
		}
		public void PrecacheSounds(string []filenames) {
			foreach (var filename in filenames) {
				this.PrecacheSound(filename);
			}
		}
	}
}