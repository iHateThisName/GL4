using Assets.Scripts.Singleton;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : Singleton<SoundEffectManager> {

    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private int initialPoolSize = 3;
    bool mute = true;

    private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();
    private Transform poolParent;

    private IEnumerator Start() {
        poolParent = new GameObject("AudioSourcePool").transform;
        poolParent.SetParent(this.transform); // Attach the pool parent to the SoundEffectManager for better organization in the hierarchy

        for (int i = 0; i < this.initialPoolSize; i++) {
            CreateNewAudioSource();
        }

        // Unmute after a short delay to prevent any initial sound glitches
        yield return new WaitForSeconds(1f);
        mute = false;
    }

    /// <summary>
    /// Plays the specified sound effect clip at the given position with the specified volume.
    /// </summary>
    /// <param name="audioClip">The audio clip to be played as the sound effect.</param>
    /// <param name="spawmTransform">The transform that determines the position where the sound effect will be played.</param>
    /// <param name="volume">The volume level of the sound effect, ranging from 0.0 (silent) to 1.0 (full volume).</param>
    /// <param name="duration">The duration, in seconds, for which the sound effect should play. If set to 0.0, the duration will default to
    /// the length of the audio clip.</param>
    /// <param name="parentSpawnTransform">An optional transform to which the audio source will be parented. If null, the audio source will be parented to
    /// the default pool parent.</param>
    public void PlaySoundFXClip(AudioClip audioClip, Transform spawmTransform, float volume, float duration = 0.0f, bool parentSpawnTransform = false) {
        if (mute) {
            return;
        }

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.transform.position = spawmTransform.position;
        audioSource.gameObject.SetActive(true);

        if (parentSpawnTransform) {
            audioSource.transform.SetParent(spawmTransform);
        } else {
            audioSource.transform.SetParent(poolParent);
        }

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.Play();

        float returnDelay = duration > 0.0f ? duration : audioSource.clip.length;

        StartCoroutine(ReturnToPool(audioSource, returnDelay));
    }

    #region Audio Pooling
    /// <summary>
    /// Creates a new AudioSource instance from the sound effects object and adds it to the audio source pool for later
    /// use.
    /// </summary>
    /// <returns>An AudioSource that has been instantiated and added to the pool. The returned AudioSource is initially inactive.</returns>
    private AudioSource CreateNewAudioSource() {
        AudioSource audioSource = Instantiate(soundFXObject, poolParent);
        audioSource.gameObject.SetActive(false);
        audioSourcePool.Add(audioSource);
        return audioSource;
    }

    /// <summary>
    /// Retrieves an available audio source from the pool that is not currently active in the hierarchy.
    /// </summary>
    /// <remarks>This method checks the audio source pool for any audio sources that are not active in the
    /// hierarchy. If all audio sources are active, a new audio source is instantiated.</remarks>
    /// <returns>An instance of <see cref="AudioSource"/> that is available for use. If no inactive audio sources are found, a
    /// new audio source is created.</returns>
    private AudioSource GetAvailableAudioSource() {
        foreach (AudioSource audioSource in audioSourcePool) {
            if (!audioSource.gameObject.activeInHierarchy) {
                return audioSource;
            }
        }
        return CreateNewAudioSource();
    }

    /// <summary>
    /// Returns the specified AudioSource to the object pool after a delay, ensuring that the audio clip has finished
    /// playing.
    /// </summary>
    /// <remarks>A small buffer is added to the delay to ensure the audio clip has finished playing before the
    /// AudioSource is stopped and deactivated. This method is intended for use with non-looping audio clips.</remarks>
    /// <param name="audioSource">The AudioSource instance to be returned to the pool. This parameter must not be null.</param>
    /// <param name="delay">The time, in seconds, to wait before returning the AudioSource to the pool. Must be non-negative.</param>
    /// <returns>An enumerator that yields until the specified delay has elapsed, allowing the coroutine to wait before
    /// deactivating the AudioSource.</returns>
    private IEnumerator ReturnToPool(AudioSource audioSource, float delay) {
        yield return new WaitForSeconds(delay + 0.1f); // Add a small buffer to ensure the clip has finished playing, its safe since the audio is not looping.

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.transform.SetParent(poolParent);
        audioSource.gameObject.SetActive(false);
    }
    #endregion
}