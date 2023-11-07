using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Quartzified.Audio
{
    public abstract partial class AudioPack : ScriptableObject
    {
        [Header("Audio Base")]
        [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>();
        public List<AudioClip> AudioClips => audioClips;

        [Space]
        public AudioMixerGroup mixerGroup;

        [Space]
        public float volumeOverride = 0f;

        public int MinIndex => 0;
        public int MaxIndex => AudioClips.Count - 1;
        public int ClipCount => AudioClips.Count;

        public float GetClipLength(int clipIndex)
        {
            return AudioClips[clipIndex].length;
        }

        public AudioClip GetRandomClip()
        {
            return AudioClips[Random.Range(0, ClipCount)];
        }

        /// <summary>
        /// Returns the index of the audio clip.
        /// If the clip is not available in the list the return will be -1.
        /// </summary>
        /// <param name="audioClip"></param>
        /// <returns></returns>
        public int GetIndexFromClip(AudioClip audioClip)
        {
            if (AudioClips.Contains(audioClip))
            {
                return AudioClips.IndexOf(audioClip);
            }

            return -1;
        }
    }

}

