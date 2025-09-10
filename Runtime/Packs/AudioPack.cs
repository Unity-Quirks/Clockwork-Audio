using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Quirks.Audio
{
    public abstract partial class AudioPack : ScriptableObject
    {
        [Header("Audio Base")]
        [SerializeField] private List<AudioClip> audioClips = new List<AudioClip>(); // Stores a collection of audio clips.
        public List<AudioClip> AudioClips => audioClips; // Provides access to the audio clips.

        [Space]
        [Tooltip("Reference to an AudioMixerGroup for managing audio mixing.")]
        public AudioMixerGroup mixerGroup;

        [Space]
        [Tooltip("Play volume level for the audio pack.")]
        [Range(0f, 1f)] public float playVolume = 1f;

        [Header("Pitch Settings")]
        [Tooltip("Enable random pitch variation for this audio pack.")]
        public bool useRandomPitch = false;

        [Tooltip("Random pitch range. X = minimum pitch, Y = maximum pitch.")]
        public Vector2 pitchRange = new Vector2(0.8f, 1.2f);

        [Header("3D Audio Settings")]
        [Tooltip("How 3D the audio is. 0 = 2D, 1 = fully 3D")]
        [Range(0f, 1f)] public float spatialBlend = 1f;

        [Tooltip("How the audio volume decreases with distance")]
        public AudioRolloffMode rolloffMode = AudioRolloffMode.Logarithmic;

        [Tooltip("Distance at which audio starts to fade")]
        [Range(0f, 500f)] public float minDistance = 1f;

        [Tooltip("Distance at which audio is completely silent")]
        [Range(0f, 500f)] public float maxDistance = 50f;

        [Tooltip("Doppler effect strength")]
        [Range(0f, 5f)] public float dopplerLevel = 1f;

        /// <summary>Gets the minimum index for audio clips.</summary>
        public int MinIndex => 0;

        /// <summary>Gets the maximum index for audio clips.</summary>
        public int MaxIndex => AudioClips.Count - 1;

        /// <summary>Returns the total number of audio clips in the pack.</summary>
        public int ClipCount => AudioClips.Count;

        /// <summary>Returns the length og the audio clip at the specified index.</summary>
        public float GetClipLength(int clipIndex) => AudioClips[clipIndex].length;

        /// <summary>Retrieves a random audio clip from the pack.</summary>
        public AudioClip GetRandomClip() => AudioClips[Random.Range(0, ClipCount)];

        /// <summary>
        /// Returns the index of the audio clip.
        /// If the clip is not available in the list the return will be -1.
        /// </summary>
        public int GetIndexFromClip(AudioClip audioClip)
        {
            if (AudioClips.Contains(audioClip))
            {
                return AudioClips.IndexOf(audioClip);
            }

            return -1;
        }

        /// <summary>Gets a random pitch value within the specified range.</summary>
        public float GetRandomPitch() => useRandomPitch ? Random.Range(pitchRange.x, pitchRange.y) : 1f;

#if UNITY_EDITOR

        void OnValidate()
        {
            if (pitchRange.x > pitchRange.y)
            {
                float temp = pitchRange.x;
                pitchRange.x = pitchRange.y;
                pitchRange.y = temp;
            }

            if (maxDistance < minDistance)
                maxDistance = minDistance;
        }

#endif
    }

}
