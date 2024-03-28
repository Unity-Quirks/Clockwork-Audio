using UnityEngine;
using UnityEngine.Audio;

using System.Collections.Generic;
using System.Collections;

namespace Quirks.Audio
{
    public class AudioManager : MonoBehaviour
    {
        static AudioManager instance; // Singleton instance of AudioManager.
        public static AudioManager Instance => instance;

        [Header("Music Settings")]
        public GameObject musicSourcePrefab = null; // Music Source Prefab which will be used in the source pool.
        public AudioMixerGroup musicMixerGroup;
        [Space]
        public MusicPack defaultMusicPack;
        List<AudioSource> musicSourcePool = new List<AudioSource>();
        int currentMusicSourceIndex = -1;
        AudioSource currentMusicSource => (currentMusicSourceIndex < 0 || musicSourcePool.Count <= 0 || musicSourcePool.Count < currentMusicSourceIndex) ? null : musicSourcePool[currentMusicSourceIndex];

        [Space]
        public bool loopCurrentMusic = true; // Should the current song loop?
        Coroutine loopCoroutine = null; 
        MusicPack currentMusicPack; // Current playing Music Pack
        int currentMusicClipIndex = 0; // Current Music Clip Index

        double nextLoopStartTime = 0;
        public float musicTimeRemaining => nextLoopStartTime != 0f ? (float)(nextLoopStartTime - AudioSettings.dspTime) + (!loopCurrentMusic ? currentMusicPack.ReverbTail : 0f) : 0f;

        [Space]
        public bool playOnAwake = true;
        [Range(0f, 1f)] public float maxVolume = 1f;
        public float defaultMusicBlendDuration = 1f;

        [Header("Effect Settings")]
        [Tooltip("Prefab used for creating audio sources for effects.")]
        public GameObject effectSourcePrefab = null;

        List<AudioSource> effectSourcePool = new List<AudioSource>(); // Pool of audio sources for playing effects.
        int currentEffectSourceIndex = -1; // index of the currently used audio source.

        // Returns the current effect source, or null if unavailable.
        AudioSource currentEffectSource => (currentEffectSourceIndex < 0 || effectSourcePool.Count <= 0 || effectSourcePool.Count < currentEffectSourceIndex) ? null : effectSourcePool[currentEffectSourceIndex];

        private void Awake()
        {
            instance = instance ?? this;
            if(instance != this) 
            {
                DestroyImmediate(this);
                return;
            }
            DontDestroyOnLoad(this.gameObject);

            if (playOnAwake && defaultMusicPack != null) PlayMusic(defaultMusicPack);
        }

        #region Music Manager

        /// <summary>
        /// Plays the specified audio track from an MusicPack at a given index.
        /// </summary>
        public void PlayMusic(MusicPack musicPack, int clipIndex = 0, float startTime = 0f, float blendOutTime = 1f, float blendInTime = 1f)
        {
            if (musicPack == null) return;
            if (musicPack.AudioClips.Count <= 0) return;

            // Check if we are currently already playing a audio Track
            // Fade the audio track to the endVolume for our next track to play.
            if (currentMusicSourceIndex != -1)
            {
                AudioSource current = currentMusicSource;

                float endVolume = (blendOutTime == -1f) ? current.volume : 0f;
                float fadeTime = (blendOutTime == -1f) ? currentMusicPack.ReverbTail : blendOutTime;
                StartCoroutine(FadeVolume(current, current.volume, endVolume, fadeTime));
            }

            // Get the next available Music Source
            currentMusicSourceIndex = GetNextMusicLayerIndex();
            AudioSource nextSource = currentMusicSource;

            // Set Music Data
            currentMusicPack = musicPack;
            currentMusicClipIndex = clipIndex;

            // Stop previous audio track Loop coroutine if it exists
            if (loopCoroutine != null) StopCoroutine(loopCoroutine);

            // Start current audio track Loop corouitine
            loopCoroutine = StartCoroutine(Loop(startTime));

            // Fade in the audio track
            StartCoroutine(FadeVolume(nextSource, 0f, maxVolume, blendInTime));

            // Set all the necessary Source Data
            nextSource.clip = currentMusicPack.AudioClips[currentMusicClipIndex];

            // Rename so we can clearly see the Music Source and what clip it is playing.
            nextSource.name = "[Music] " + nextSource.clip.name;
            nextSource.time = startTime;

            // Start playing the new Track
            nextSource.Play();
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            AudioSource currentSource = currentMusicSource;
            StartCoroutine(FadeVolume(currentSource, currentSource.volume, 0f, fadeOutDuration));
        }

        // Fade Coroutine
        IEnumerator FadeVolume(AudioSource source, float start, float end, float duration)
        {
            duration = Mathf.Max(duration, 0f);
            float fadeDuration = 0f;

            // Volume Fade
            while (fadeDuration < duration)
            {
                yield return new WaitForEndOfFrame();
                fadeDuration += Time.unscaledDeltaTime;
                source.volume = Mathf.SmoothStep(start, end, fadeDuration / duration);
            }

            // Ensure volume is at the desired level
            source.volume = end;

            // If our volume has reached its destination
            // Stop the Source and prepare for next use
            if (source.volume == 0f || end == start)
            {
                if (source == currentMusicSource)
                {
                    musicSourcePool.ForEach(s => s.Stop());
                    StopCoroutine(loopCoroutine);
                    currentMusicSourceIndex = -1;
                    nextLoopStartTime = 0f;
                }

                source.volume = 0f;
                source.Stop();
                source.gameObject.SetActive(false);
            }
        }

        // Loop Coroutine
        IEnumerator Loop(float startTime)
        {
            float fullLength = currentMusicPack.AudioClips[currentMusicClipIndex].length;
            float waitTime = fullLength / currentMusicPack.ReverbTail - startTime;

            nextLoopStartTime = AudioSettings.dspTime + waitTime;
            yield return new WaitForSecondsRealtime(waitTime);

            if (!loopCurrentMusic)
            {
                AudioSource currentSource = currentMusicSource;
                StartCoroutine(FadeVolume(currentSource, currentSource.volume, currentSource.volume, currentMusicPack.ReverbTail));
                yield break;
            }

            PlayMusic(currentMusicPack);
        }

        // Gets the index for the next available music audio source.
        // If non found, we create one.
        int GetNextMusicLayerIndex()
        {
            AudioSource next = musicSourcePool.Find(layer => !layer.isPlaying);
            if (next == null || musicSourcePool.Count <= 0)
            {
                next = Instantiate(musicSourcePrefab, transform).GetComponent<AudioSource>();
                musicSourcePool.Add(next);
            }
            next.gameObject.SetActive(true);
            return musicSourcePool.IndexOf(next);
        }

        #endregion

        #region Effect Manager

        /// <summary>
        /// Plays the specified audio effect from an EffectPack at a given index.
        /// </summary>
        public void PlayEffect(EffectPack effectPack, int index = 0)
        {
            if (effectPack.ClipCount <= 0 || index > effectPack.MaxIndex)
                return;

            if(currentEffectSourceIndex  == -1 || currentEffectSource.isPlaying)
                currentEffectSourceIndex = GetNextEffectLayerIndex();

            AudioSource playSource = currentEffectSource;
            if(effectPack.mixerGroup != null)
                playSource.outputAudioMixerGroup = effectPack.mixerGroup;

            AudioClip playClip = effectPack.AudioClips[index];

            playSource.volume = effectPack.playVolume;

            playSource.clip = playClip;
            playSource.Play();
        }

        /// <summary>
        /// Plays a random audi effect from an EffectPack.
        /// </summary>
        public void PlayRandomEffect(EffectPack effectPack)
        {
            if (effectPack.ClipCount <= 0)
                return;

            if (currentEffectSourceIndex == -1 || currentEffectSource.isPlaying)
                currentEffectSourceIndex = GetNextEffectLayerIndex();

            AudioSource playSource = currentEffectSource;
            if (effectPack.mixerGroup != null)
                playSource.outputAudioMixerGroup = effectPack.mixerGroup;

            AudioClip playClip = effectPack.GetRandomClip();

            playSource.volume = effectPack.playVolume;

            playSource.clip = playClip;
            playSource.Play();
        }

        // Gets the index for the next available effect audio source.
        // If non found, we create one.
        int GetNextEffectLayerIndex()
        {
            AudioSource next = effectSourcePool.Find(layer => !layer.isPlaying);
            if(next == null || effectSourcePool.Count <= 0)
            {
                next = Instantiate(effectSourcePrefab, transform).GetComponent<AudioSource>();

                // Rename so we can clearly see effect sources in the Hierarchy
                next.name = "[Effect] Source";

                effectSourcePool.Add(next);
            }
            next.gameObject.SetActive(true);
            return effectSourcePool.IndexOf(next);
        }

        #endregion


#if UNITY_EDITOR

        [UnityEditor.MenuItem("GameObject/Audio/Audio Manager", false, 0)]
        static void CreateAudioManager(UnityEditor.MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Audio Manager", typeof(AudioManager));
        }

#endif
    }

}

