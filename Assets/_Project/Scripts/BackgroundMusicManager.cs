using UnityEngine;

namespace Mode3D
{
	/// <summary>
	/// Gestionnaire de musique d'ambiance en boucle
	/// </summary>
	public class BackgroundMusicManager : MonoBehaviour
	{
		[Header("Audio Settings")]
		[SerializeField] private AudioClip backgroundMusic;
		[SerializeField] [Range(0f, 1f)] private float volume = 0.3f;
		[SerializeField] private bool playOnAwake = true;

		private AudioSource audioSource;
		private static BackgroundMusicManager instance;

		void Awake()
		{
			// Singleton pattern - une seule instance de musique
			if (instance != null && instance != this)
			{
				Destroy(gameObject);
				return;
			}

			instance = this;
			DontDestroyOnLoad(gameObject);

			SetupAudioSource();
		}

		void Start()
		{
			if (playOnAwake && backgroundMusic != null)
			{
				PlayMusic();
			}
		}

		private void SetupAudioSource()
		{
			audioSource = gameObject.GetComponent<AudioSource>();
			if (audioSource == null)
			{
				audioSource = gameObject.AddComponent<AudioSource>();
			}

			audioSource.clip = backgroundMusic;
			audioSource.loop = true;
			audioSource.playOnAwake = false;
			audioSource.volume = volume;
			audioSource.spatialBlend = 0f; // 2D sound
		}

		public void PlayMusic()
		{
			if (audioSource != null && !audioSource.isPlaying)
			{
				audioSource.Play();
			}
		}

		public void StopMusic()
		{
			if (audioSource != null && audioSource.isPlaying)
			{
				audioSource.Stop();
			}
		}

		public void SetVolume(float newVolume)
		{
			volume = Mathf.Clamp01(newVolume);
			if (audioSource != null)
			{
				audioSource.volume = volume;
			}
		}

		public void ToggleMute()
		{
			if (audioSource != null)
			{
				audioSource.mute = !audioSource.mute;
			}
		}
	}
}
