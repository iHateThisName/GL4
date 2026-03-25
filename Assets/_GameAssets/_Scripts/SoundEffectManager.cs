using Assets.Scripts.Singleton;
using System.Collections.Generic;
using UnityEngine;

public class SoundEffectManager : Singleton<SoundEffectManager>
{
    [SerializeField] private AudioSource soundFXObject;
    [SerializeField] private int initialPoolSize = 3;
    bool mute = true;

    private readonly List<AudioSource> audioSourcePool = new List<AudioSource>();
    private Transform poolParent;

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

    public void PlaySoundFXClip(AudioClip audioClip, Transform spawmTransform, float volume, float duration = 0.0f, bool parentSpawnTransform = false)
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
        audioSource.Play();

        float returnDelay = duration > 0.0f ? duration : audioSource.clip.length;
        _ = ReturnToPoolAsync(audioSource, returnDelay);
    }

    #region Audio Pooling
    private AudioSource CreateNewAudioSource()
    {
        AudioSource audioSource = Instantiate(soundFXObject, poolParent);
        audioSource.gameObject.SetActive(false);
        audioSourcePool.Add(audioSource);
        return audioSource;
    }

    private AudioSource GetAvailableAudioSource()
    {
        foreach (AudioSource audioSource in audioSourcePool)
        {
            if (!audioSource.gameObject.activeInHierarchy)
                return audioSource;
        }
        return CreateNewAudioSource();
    }

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
