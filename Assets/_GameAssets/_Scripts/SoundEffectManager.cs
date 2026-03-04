using Assets.Scripts.Singleton;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class SoundEffectManager : Singleton<SoundEffectManager>
{

    [SerializeField] private AudioSource soundFXObject;
    bool mute = true;

    private IEnumerator Start()
    {
        yield return new WaitForSecondsRealtime(1f);
        mute = false;
    }
    public void PlaySoundFXClip(AudioClip audioClip, Transform spawmTransform, float volume, float duration = 0.0f)
    {
        if (mute)
        {
            return;
        }
        //spawn in GameObject
        AudioSource audioSource = Instantiate(soundFXObject, spawmTransform.position, Quaternion.identity);
        //assign audioClip
        audioSource.clip = audioClip;
        //assign volume
        //audioSource.volume = SoundManager.instance.SFXVolumeWithMasterVolumeApplied();
        //play sound
        audioSource.Play();
        //get length of clip
        float clipLength = audioSource.clip.length;

        //destroy the clip after playing
        if (duration > 0.0f)
        {
            Destroy(audioSource.gameObject, duration);
        }
        else
        {
            Destroy(audioSource.gameObject, clipLength);
        }
    }
}

//if (SFX == null) return true;
//SoundEffectManager.Instance.PlaySoundFXClip(this.SFX, transform, 1f);