using Assets.Scripts.Singleton;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// A singleton manager responsible for playing sound effects throughout the game.
/// It uses an object pooling system for AudioSources to avoid frequent instantiation, 
/// supporting both standard and 3D positional sounds.
/// </summary>
public class SoundEffectManager : Singleton<SoundEffectManager>
{
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private int initialPoolSize = 3;
    bool mute = true;

    private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();
    private Transform poolParent;

    /// <summary>
    /// Initializes the AudioSource pool and temporarily mutes audio output to prevent start-up glitches.
    /// </summary>
    private async void Start()
    {
        poolParent = new GameObject("AudioSourcePool").transform;
        poolParent.SetParent(this.transform);

        for (int i = 0; i < this.initialPoolSize; i++)
        {
            CreateNewAudioSource();
        }

        // Unmute after a short delay to prevent any initial sound glitches
        await Awaitable.WaitForSecondsAsync(1f, destroyCancellationToken);
        mute = false;
    }

    /// <summary>
    /// Plays an audio clip using an available AudioSource from the pool.
    /// </summary>
    /// <param name="audioClip">The AudioClip to be played.</param>
    /// <param name="spawmTransform">The Transform representing the spawn location for the sound.</param>
    /// <param name="volume">The volume level at which to play the sound.</param>
    /// <param name="duration">Optional override duration. If greater than 0, the source returns to the pool after this duration; otherwise, it uses the clip's length.</param>
    /// <param name="is3DSound">Whether the audio should be played as a 3D spatial sound.</param>
    /// <param name="parentSpawnTransform">Whether to parent the AudioSource to the spawn transform. If false, it remains parented to the pool.</param>
    public void PlaySoundFXClip(AudioClip audioClip, Transform spawmTransform, float volume, float duration = 0.0f, bool is3DSound = false, bool parentSpawnTransform = false)
    {
        if (mute) return;

        AudioSource audioSource = GetAvailableAudioSource();
        audioSource.transform.position = spawmTransform.position;
        audioSource.gameObject.SetActive(true);

        if (parentSpawnTransform)
            audioSource.transform.SetParent(spawmTransform);
        else
            audioSource.transform.SetParent(poolParent);

        audioSource.clip = audioClip;
        audioSource.volume = volume;
        audioSource.spatialBlend = is3DSound ? 1.0f : 0.0f;

        audioSource.Play();

        float returnDelay = duration > 0.0f ? duration : audioSource.clip.length;
        _ = ReturnToPoolAsync(audioSource, returnDelay);
    }

    #region Audio Pooling
    /// <summary>
    /// Instantiates a new AudioSource from the prefab, adds it to the pool, and returns it.
    /// </summary>
    /// <returns>The newly created AudioSource.</returns>
    private AudioSource CreateNewAudioSource()
    {
        AudioSource audioSource = Instantiate(soundFXObject, poolParent);
        audioSource.gameObject.SetActive(false);
        audioSourcePool.Add(audioSource);
        return audioSource;
    }

    /// <summary>
    /// Retrieves an inactive AudioSource from the pool. If all are active, a new one is created.
    /// </summary>
    /// <returns>An available AudioSource ready for use.</returns>
    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (!audioSource.gameObject.activeInHierarchy)
                return audioSource;
        }
        return CreateNewAudioSource();
    }

    /// <summary>
    /// Asynchronously waits for a specified delay, then stops the given AudioSource and returns it to the pool.
    /// </summary>
    /// <param name="audioSource">The AudioSource to return.</param>
    /// <param name="delay">The time in seconds to wait before recycling.</param>
    /// <returns>An awaitable representing the async operation.</returns>
    private async Awaitable ReturnToPoolAsync(AudioSource audioSource, float delay)
    {
        await Awaitable.WaitForSecondsAsync(delay + 0.1f, destroyCancellationToken);

        audioSource.Stop();
        audioSource.clip = null;
        audioSource.transform.SetParent(poolParent);
        audioSource.gameObject.SetActive(false);
    }
    #endregion
}
