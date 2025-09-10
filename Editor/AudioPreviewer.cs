#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;
using System;

namespace Clockwork.Audio.Editor
{
    public static class AudioPreviewer
    {
        // Events for preview state
        public static event Action OnPreviewStarted;
        public static event Action OnPreviewStopped;
        public static event Action<float> OnDecibelUpdate;

        static EditorAudioPlayer currentPlayer;

        class EditorAudioPlayer
        {
            GameObject gameObject;
            AudioSource audioSource;
            float startTime;
            float clipLength;
            bool isManualStop;
            float[] audioData;
            int sampleRate;

            public EditorAudioPlayer(GameObject go, AudioClip clip, float delay)
            {
                this.gameObject = go;
                this.audioSource = go.GetComponent<AudioSource>();
                this.clipLength = delay;
                this.startTime = (float)EditorApplication.timeSinceStartup;
                this.isManualStop = false;

                // Prepare audio data for decibel calculation
                if (clip != null)
                {
                    audioData = new float[clip.samples * clip.channels];
                    clip.GetData(audioData, 0);
                    sampleRate = clip.frequency;
                }

                EditorApplication.update += Update;
                OnPreviewStarted?.Invoke();
            }

            void Update()
            {
                if (gameObject == null || audioSource == null)
                {
                    Cleanup();
                    return;
                }

                float currentTime = (float)EditorApplication.timeSinceStartup;
                float elapsedTime = currentTime - startTime;

                // Calculate and send decibel level
                if (audioSource.isPlaying && audioData != null)
                {
                    float decibelLevel = CalculateCurrentDecibelLevel(elapsedTime);
                    OnDecibelUpdate?.Invoke(decibelLevel);
                }

                // Check if we should stop (either finished or manual stop)
                if (elapsedTime >= clipLength || isManualStop || !audioSource.isPlaying)
                {
                    Cleanup();
                }
            }

            float CalculateCurrentDecibelLevel(float timePosition)
            {
                if (audioData == null || audioData.Length == 0)
                    return -80f;

                // Calculate the sample position based on time
                int samplePosition = Mathf.FloorToInt(timePosition * sampleRate);

                // Get a small window of samples around the current position for RMS calculation
                int windowSize = 1024; // Small window for real-time calculation
                int startSample = Mathf.Max(0, samplePosition - windowSize / 2);
                int endSample = Mathf.Min(audioData.Length, startSample + windowSize);

                if (startSample >= endSample)
                    return -80f;

                // Calculate RMS (Root Mean Square) for the window
                float sum = 0f;
                int actualSamples = 0;

                for (int i = startSample; i < endSample; i++)
                {
                    sum += audioData[i] * audioData[i];
                    actualSamples++;
                }

                if (actualSamples == 0)
                    return -80f;

                float rms = Mathf.Sqrt(sum / actualSamples);

                // Convert RMS to decibels
                // Using 20 * log10(rms) formula, with a minimum threshold
                if (rms < 0.0001f) // Very quiet threshold
                    return -80f;

                float decibels = 20f * Mathf.Log10(rms);

                // Clamp to reasonable range
                return Mathf.Clamp(decibels, -80f, 0f);
            }

            public void Stop()
            {
                isManualStop = true;
                if (audioSource != null && audioSource.isPlaying)
                    audioSource.Stop();
            }

            void Cleanup()
            {
                EditorApplication.update -= Update;

                if (gameObject != null)
                    UnityEngine.Object.DestroyImmediate(gameObject);

                OnPreviewStopped?.Invoke();

                if (currentPlayer == this)
                    currentPlayer = null;
            }
        }

        public static void PlayClipAtPoint(AudioClip clip, Vector3 position, float volume, float pitch)
        {
            // Stop any existing preview first
            StopCurrentPreview();

            GameObject tempGO = new GameObject("One Shot Audio (Preview)");
            tempGO.transform.position = position;
            AudioSource audioSource = tempGO.AddComponent<AudioSource>();
            audioSource.clip = clip;
            audioSource.spatialBlend = 1f;
            audioSource.volume = volume;
            audioSource.pitch = pitch;
            audioSource.Play();

            if (!Application.isPlaying)
            {
                // Create our custom player for Edit Mode
                float delay = clip.length / Mathf.Max(0.01f, Mathf.Abs(pitch));
                currentPlayer = new EditorAudioPlayer(tempGO, clip, delay);
            }
            else
            {
                // Normal behavior in Play Mode
                UnityEngine.Object.Destroy(tempGO, clip.length / Mathf.Max(0.01f, Time.timeScale));
            }
        }

        public static void StopCurrentPreview()
        {
            if (currentPlayer != null)
            {
                currentPlayer.Stop();
                currentPlayer = null;
            }
        }

        public static bool IsPreviewPlaying()
        {
            return currentPlayer != null;
        }
    }
}

#endif