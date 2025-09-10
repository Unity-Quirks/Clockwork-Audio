using System.Collections.Generic;
using UnityEngine;

namespace Quirks.Audio
{
    public class LoopManager : MonoBehaviour, ILoopManager
    {
        [Header("Loop Settings")]
        [Tooltip("Prefab used for creating audio sources for loops.")]
        public GameObject sourcePrefab = null;

        [Header("Positional Audio Settings")]
        [Tooltip("Default 3D audio settings for positional sounds")]
        public AudioRolloffMode defaultRolloffMode = AudioRolloffMode.Logarithmic;
        [Range(0f, 500f)] public float defaultMinDistance = 4f;
        [Range(0f, 500f)] public float defaultMaxDistance = 64f;
        [Range(0f, 1f)] public float defaultSpatialBlend = 1f; // 0 = 2D, 1 = 3D

        // Internal state
        List<AudioSource> sourcePool = new List<AudioSource>();
        List<LoopHandle> activeLoops = new List<LoopHandle>();

        public int ActiveLoopCount
        {
            get
            {
                CleanupDeadLoops();
                return activeLoops.Count;
            }
        }

        void Update()
        {
            // Periodically cleanup dead loops (every few seconds)
            if (Time.time % 5f < Time.deltaTime)
            {
                CleanupDeadLoops();
            }
        }

        public LoopHandle StartLoop(LoopPack loopPack, int clipIndex = 0, float fadeInTime = 0.5f, float startVolume = 1f, Vector3? position = null, Transform parent = null)
        {
            if (loopPack == null || loopPack.ClipCount <= 0 || clipIndex >= loopPack.ClipCount)
                return null;

            // Get available loop source
            AudioSource loopSource = GetNextSource();
            if (loopSource == null) return null;

            // Setup the audio source
            SetupLoopSource(loopSource, loopPack, position, parent);

            // Set the clip
            loopSource.clip = loopPack.AudioClips[clipIndex];
            loopSource.loop = true;

            // Apply random pitch if enabled
            if (loopPack.useRandomPitch)
            {
                loopSource.pitch = Random.Range(loopPack.pitchRange.x, loopPack.pitchRange.y);
            }
            else
            {
                loopSource.pitch = 1f;
            }

            // Create loop handle
            LoopHandle handle = new LoopHandle(loopSource, loopPack, this, clipIndex);
            activeLoops.Add(handle);

            // Start with zero volume
            loopSource.volume = 0f;
            loopSource.Play();

            // Fade in
            handle.FadeToVolume(startVolume / loopPack.playVolume, fadeInTime);

            // Set name for debugging
            loopSource.name = "[Loop] " + loopSource.clip.name;

            return handle;
        }

        public void StopAllLoops(float fadeOutTime = 1f)
        {
            for (int i = activeLoops.Count - 1; i >= 0; i--)
            {
                if (activeLoops[i] != null && activeLoops[i].IsActive)
                {
                    activeLoops[i].Stop(fadeOutTime);
                }
            }
        }

        public void StopAllLoopsImmediate()
        {
            for (int i = activeLoops.Count - 1; i >= 0; i--)
            {
                if (activeLoops[i] != null)
                {
                    activeLoops[i].StopImmediate();
                }
            }
            CleanupDeadLoops();
        }

        public LoopHandle[] GetActiveLoops()
        {
            CleanupDeadLoops();
            return activeLoops.ToArray();
        }

        void SetupLoopSource(AudioSource source, LoopPack loopPack, Vector3? position, Transform parent)
        {
            // Reset transform parent first
            source.transform.SetParent(transform);

            // Set mixer group
            if (loopPack.mixerGroup != null)
                source.outputAudioMixerGroup = loopPack.mixerGroup;

            // Setup positional audio
            if (position.HasValue || parent != null)
            {
                // Configure 3D audio settings
                source.spatialBlend = loopPack.spatialBlend > 0 ? loopPack.spatialBlend : defaultSpatialBlend;
                source.rolloffMode = loopPack.rolloffMode;
                source.minDistance = loopPack.minDistance > 0 ? loopPack.minDistance : defaultMinDistance;
                source.maxDistance = loopPack.maxDistance > 0 ? loopPack.maxDistance : defaultMaxDistance;
                source.dopplerLevel = loopPack.dopplerLevel;

                if (parent != null)
                {
                    // Attach to parent transform
                    source.transform.SetParent(parent);
                    source.transform.localPosition = Vector3.zero;
                }
                else if (position.HasValue)
                {
                    // Set position
                    source.transform.position = position.Value;
                }
            }
            else
            {
                // 2D audio
                source.spatialBlend = 0f;
            }
        }

        AudioSource GetNextSource()
        {
            AudioSource next = sourcePool.Find(source => !source.isPlaying);
            if (next == null || sourcePool.Count <= 0)
            {
                if (sourcePrefab == null)
                {
                    Debug.LogError("Loop Source Prefab is not assigned in LoopManager!");
                    return null;
                }

                next = Instantiate(sourcePrefab, transform).GetComponent<AudioSource>();
                next.name = "[Loop] Source";
                sourcePool.Add(next);
            }

            next.gameObject.SetActive(true);
            return next;
        }

        void CleanupDeadLoops()
        {
            for (int i = activeLoops.Count - 1; i >= 0; i--)
            {
                if (activeLoops[i] == null || !activeLoops[i].IsActive)
                {
                    if (activeLoops[i] != null)
                    {
                        activeLoops[i].Cleanup();
                    }
                    activeLoops.RemoveAt(i);
                }
            }
        }
    }
}