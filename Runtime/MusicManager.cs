using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Quirks.Audio
{
    public class MusicManager : MonoBehaviour, IMusicManager
    {
        // Music Settings
        [Header("Music Settings")]
        [SerializeField] GameObject sourcePrefab = null;
        [SerializeField] AudioMixerGroup mixerGroup;

        [Space]
        [SerializeField] MusicPack defaultPack;
        [SerializeField] public bool playOnAwake = true;

        [Space]
        public bool loopCurrentMusic = true;
        [Range(0f, 1f)] public float maxVolume = 1f;
        public float defaultBlendDuration = 1f;

        // Internal state
        List<AudioSource> sourcePool = new List<AudioSource>();
        int currentSourceIndex = -1;
        AudioSource currentSource => (currentSourceIndex < 0 || sourcePool.Count <= 0 || sourcePool.Count < currentSourceIndex) ? null : sourcePool[currentSourceIndex];

        Coroutine loopCoroutine = null;
        MusicPack currentMusicPack;
        int currentMusicClipIndex = 0;

        double nextLoopStartTime = 0;
        public float MusicTimeRemaining => nextLoopStartTime != 0f ? (float)(nextLoopStartTime - AudioSettings.dspTime) + (!loopCurrentMusic ? currentMusicPack.ReverbTail : 0f) : 0f;

        void Awake()
        {
            if (playOnAwake && defaultPack != null)
                PlayMusic(defaultPack);
        }

        public void PlayMusic(MusicPack musicPack, int clipIndex = 0, float startTime = 0f, float blendOutTime = 1f, float blendInTime = 1f)
        {
            if (musicPack == null)
                return;

            if (musicPack.AudioClips.Count <= 0)
                return;

            // Check if we are currently already playing a audio Track
            // Fade the audio track to the endVolume for our next track to play.
            if (currentSourceIndex != -1)
            {
                AudioSource current = currentSource;

                float endVolume = (blendOutTime == -1f) ? current.volume : 0f;
                float fadeTime = (blendOutTime == -1f) ? currentMusicPack.ReverbTail : blendOutTime;
                StartCoroutine(FadeVolume(current, current.volume, endVolume, fadeTime));
            }

            // Get the next available Music Source
            currentSourceIndex = GetNextMusicLayerIndex();
            AudioSource nextSource = currentSource;

            // Set Music Data
            currentMusicPack = musicPack;
            currentMusicClipIndex = clipIndex;

            // Stop previous audio track Loop coroutine if it exists
            if (loopCoroutine != null)
                StopCoroutine(loopCoroutine);

            // Start current audio track Loop coroutine
            loopCoroutine = StartCoroutine(Loop(startTime));

            // Fade in the audio track
            StartCoroutine(FadeVolume(nextSource, 0f, maxVolume, blendInTime));

            // Set all the necessary Source Data
            nextSource.clip = currentMusicPack.AudioClips[currentMusicClipIndex];

            // Apply random pitch if enabled
            if (currentMusicPack.useRandomPitch)
            {
                nextSource.pitch = Random.Range(currentMusicPack.pitchRange.x, currentMusicPack.pitchRange.y);
            }
            else
            {
                nextSource.pitch = 1f;
            }

            // Rename so we can clearly see the Music Source and what clip it is playing.
            nextSource.name = "[Music] " + nextSource.clip.name;
            nextSource.time = startTime;

            // Start playing the new Track
            nextSource.Play();
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            AudioSource currentSource = this.currentSource;
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
                if (source == currentSource)
                {
                    sourcePool.ForEach(s => s.Stop());
                    StopCoroutine(loopCoroutine);
                    currentSourceIndex = -1;
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
                AudioSource currentSource = this.currentSource;
                StartCoroutine(FadeVolume(currentSource, currentSource.volume, currentSource.volume, currentMusicPack.ReverbTail));
                yield break;
            }

            PlayMusic(currentMusicPack);
        }

        // Gets the index for the next available music audio source.
        // If non found, we create one.
        int GetNextMusicLayerIndex()
        {
            AudioSource next = sourcePool.Find(layer => !layer.isPlaying);
            if (next == null || sourcePool.Count <= 0)
            {
                next = GameObject.Instantiate(sourcePrefab, transform).GetComponent<AudioSource>();
                sourcePool.Add(next);
            }
            next.gameObject.SetActive(true);
            return sourcePool.IndexOf(next);
        }
    }
}