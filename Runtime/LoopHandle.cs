using UnityEngine;
using System.Collections;

namespace Quirks.Audio
{
    /// <summary>
    /// Handle for controlling a looping audio source. Provides methods to control volume, 
    /// fade in/out, change clips, and stop the loop.
    /// </summary>
    public class LoopHandle
    {
        AudioSource audioSource;
        LoopPack loopPack;

        MonoBehaviour coroutineRunner;
        Coroutine currentFadeCoroutine;

        int currentClipIndex;
        bool isActive;
        float targetVolume;

        public AudioSource AudioSource => audioSource;
        public LoopPack LoopPack => loopPack;
        public int CurrentClipIndex => currentClipIndex;
        public bool IsActive => isActive && audioSource != null && audioSource.isPlaying;
        public float CurrentVolume => audioSource != null ? audioSource.volume : 0f;
        public float TargetVolume => targetVolume;

        bool IsValid => isActive && audioSource != null;

        public LoopHandle(AudioSource source, LoopPack pack, MonoBehaviour runner, int clipIndex)
        {
            audioSource = source;
            loopPack = pack;
            coroutineRunner = runner;
            currentClipIndex = clipIndex;
            isActive = true;
            targetVolume = pack.playVolume;
        }

        /// <summary>
        /// Sets the volume immediately without fading.
        /// </summary>
        /// <param name="volume">Target volume (0-1)</param>
        public void SetVolume(float volume)
        {
            if (!IsValid)
                return;

            volume = Mathf.Clamp01(volume);
            targetVolume = volume * loopPack.playVolume;
            audioSource.volume = targetVolume;

            StopCurrentFade();
        }

        /// <summary>
        /// Fades the volume to a target value over time.
        /// </summary>
        /// <param name="targetVolumePercent">Target volume percentage (0-1)</param>
        /// <param name="fadeTime">Time to fade</param>
        public void FadeToVolume(float targetVolumePercent, float fadeTime)
        {
            if (!IsValid)
                return;

            targetVolumePercent = Mathf.Clamp01(targetVolumePercent);
            targetVolume = targetVolumePercent * loopPack.playVolume;

            StopCurrentFade();
            currentFadeCoroutine = coroutineRunner.StartCoroutine(FadeVolumeCoroutine(audioSource.volume, targetVolume, fadeTime));
        }

        /// <summary>
        /// Smoothly changes to a different clip in the same loop pack.
        /// </summary>
        /// <param name="newClipIndex">Index of the new clip</param>
        /// <param name="crossfadeTime">Time to crossfade (uses pack default if not specified)</param>
        public void ChangeClip(int newClipIndex, float? crossfadeTime = null)
        {
            if (!IsValid || newClipIndex < 0 || newClipIndex >= loopPack.ClipCount)
                return;

            if (newClipIndex == currentClipIndex)
                return;

            float fadeTime = crossfadeTime ?? loopPack.crossfadeTime;
            currentClipIndex = newClipIndex;

            StopCurrentFade();
            currentFadeCoroutine = coroutineRunner.StartCoroutine(CrossfadeToClipCoroutine(newClipIndex, fadeTime));
        }

        /// <summary>
        /// Changes to a random clip from the loop pack.
        /// </summary>
        /// <param name="crossfadeTime">Time to crossfade</param>
        public void ChangeToRandomClip(float? crossfadeTime = null)
        {
            if (!IsValid)
                return;

            int randomIndex;
            do
            {
                randomIndex = Random.Range(0, loopPack.ClipCount);
            }
            while (randomIndex == currentClipIndex && loopPack.ClipCount > 1);

            ChangeClip(randomIndex, crossfadeTime);
        }

        /// <summary>
        /// Stops the loop with a fade out.
        /// </summary>
        /// <param name="fadeOutTime">Time to fade out (uses pack default if not specified)</param>
        public void Stop(float? fadeOutTime = null)
        {
            if (!IsValid)
                return;

            float fadeTime = fadeOutTime ?? loopPack.defaultFadeOutTime;
            targetVolume = 0f;

            StopCurrentFade();
            currentFadeCoroutine = coroutineRunner.StartCoroutine(FadeOutAndStopCoroutine(fadeTime));
        }

        /// <summary>
        /// Stops the loop immediately without fading.
        /// </summary>
        public void StopImmediate()
        {
            if (!IsValid)
                return;

            StopCurrentFade();
            audioSource.Stop();
            audioSource.gameObject.SetActive(false);
            isActive = false;
        }

        /// <summary>
        /// Pauses the loop (can be resumed with Resume()).
        /// </summary>
        public void Pause()
        {
            if (!IsValid)
                return;

            audioSource.Pause();
        }

        /// <summary>
        /// Resumes a paused loop.
        /// </summary>
        public void Resume()
        {
            if (!IsValid)
                return;

            audioSource.UnPause();
        }

        /// <summary>
        /// Updates the 3D position of the loop (for positional audio).
        /// </summary>
        /// <param name="newPosition">New world position</param>
        public void UpdatePosition(Vector3 newPosition)
        {
            if (!IsValid)
                return;

            audioSource.transform.position = newPosition;
        }

        void StopCurrentFade()
        {
            if (currentFadeCoroutine != null)
            {
                coroutineRunner.StopCoroutine(currentFadeCoroutine);
                currentFadeCoroutine = null;
            }
        }

        IEnumerator FadeVolumeCoroutine(float fromVolume, float toVolume, float duration)
        {
            float elapsed = 0f;

            while (elapsed < duration && IsValid)
            {
                elapsed += Time.unscaledDeltaTime;
                float progress = elapsed / duration;
                audioSource.volume = Mathf.SmoothStep(fromVolume, toVolume, progress);
                yield return null;
            }

            if (IsValid)
                audioSource.volume = toVolume;

            currentFadeCoroutine = null;
        }

        IEnumerator CrossfadeToClipCoroutine(int newClipIndex, float crossfadeTime)
        {
            AudioClip newClip = loopPack.AudioClips[newClipIndex];
            float originalVolume = audioSource.volume;
            float halfTime = crossfadeTime * 0.5f;

            // Fade out current clip
            yield return FadeVolumeCoroutine(originalVolume, 0f, halfTime);

            if (!IsValid)
                yield break;

            // Switch clip
            audioSource.clip = newClip;
            audioSource.time = 0f;

            // Apply random pitch if enabled
            if (loopPack.useRandomPitch)
                audioSource.pitch = Random.Range(loopPack.pitchRange.x, loopPack.pitchRange.y);

            if (!audioSource.isPlaying)
                audioSource.Play();

            // Fade in new clip
            yield return FadeVolumeCoroutine(0f, originalVolume, halfTime);

            currentFadeCoroutine = null;
        }

        IEnumerator FadeOutAndStopCoroutine(float fadeTime)
        {
            float startVolume = audioSource.volume;

            yield return FadeVolumeCoroutine(startVolume, 0f, fadeTime);

            if (IsValid)
            {
                audioSource.Stop();
                audioSource.gameObject.SetActive(false);
                isActive = false;
            }

            currentFadeCoroutine = null;
        }

        // Cleanup method called by AudioManager
        internal void Cleanup()
        {
            StopCurrentFade();
            isActive = false;
        }
    }
}