namespace ColorLinesNG2 {
	public interface IAudioManager {
		string SoundPath { get; set; }
		float BackgroundMusicVolume { get; set; }
		float SoundsVolume { get; set; }
		bool MusicEnabled { get; set; }
		bool SoundsEnabled { get; set; }
		
		void DeactivateAudioSession();
		void ReactivateAudioSession();

		void PlayBackgroundMusic(string filename);
		void StopBackgroundMusic();
		void FlushBackgroundMusic();
		void RestartBackgroundMusic(bool restart = false);

		void PlaySound(string filename);

		void PrecacheSounds(string []filenames);
	}
}