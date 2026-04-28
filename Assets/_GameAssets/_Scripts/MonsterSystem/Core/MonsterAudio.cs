using UnityEngine;

namespace MonsterSystem
{
    public static class MonsterAudio
    {
        public static void Play(AudioSource source, AudioClip clip, bool loop = false, float volume = 1f)
        {
            if (source == null || clip == null) return;
            
            source.Stop();
            source.clip = clip;
            source.loop = loop;
            source.volume = volume;
            source.Play();
        }

        public static void PlayOneShot(AudioSource source, AudioClip clip, float volume = 1f)
        {
            if (source == null || clip == null) return;
            source.PlayOneShot(clip, volume);
        }

        public static void Stop(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
        }
    }
}
