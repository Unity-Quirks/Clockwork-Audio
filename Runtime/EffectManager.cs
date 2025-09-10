using System.Collections.Generic;
using UnityEngine;

namespace Quirks.Audio
{
    /// <summary>Standalone effect management system. Handles one-shot sound effects with 3D audio support.</summary>
    public class EffectManager : MonoBehaviour, IEffectManager
    {
        [Header("Effect Settings")]
        [Tooltip("Prefab used for creating audio sources for effects.")]
        public GameObject sourcePrefab = null;

        [Header("Positional Audio Settings")]
        [Tooltip("Default 3D audio settings for positional sounds")]
        public AudioRolloffMode defaultRolloffMode = AudioRolloffMode.Logarithmic;
        [Range(0f, 500f)] public float defaultMinDistance = 4f;
        [Range(0f, 500f)] public float defaultMaxDistance = 64f;
        [Range(0f, 1f)] public float defaultSpatialBlend = 1f; // 0 = 2D, 1 = 3D

        // Internal state
        List<AudioSource> sourcePool = new List<AudioSource>();
        int currentSourceIndex = -1;
        AudioSource currentSource => (currentSourceIndex < 0 || sourcePool.Count <= 0 || sourcePool.Count < currentSourceIndex) ? null : sourcePool[currentSourceIndex];

        public void PlayEffect(EffectPack effectPack, int index = 0, Vector3? position = null, Transform parent = null)
        {
            if (effectPack.ClipCount <= 0 || index > effectPack.MaxIndex)
                return;

            if (currentSourceIndex == -1 || currentSource.isPlaying)
                currentSourceIndex = GetNextLayerIndex();

            AudioSource playSource = currentSource;
            SetupSource(playSource, effectPack, position, parent);

            AudioClip playClip = effectPack.AudioClips[index];
            playSource.clip = playClip;
            playSource.Play();
        }

        public void PlayRandomEffect(EffectPack effectPack, Vector3? position = null, Transform parent = null)
        {
            if (effectPack.ClipCount <= 0)
                return;

            if (currentSourceIndex == -1 || currentSource.isPlaying)
                currentSourceIndex = GetNextLayerIndex();

            AudioSource playSource = currentSource;
            SetupSource(playSource, effectPack, position, parent);

            AudioClip playClip = effectPack.GetRandomClip();
            playSource.clip = playClip;
            playSource.Play();
        }

        public void PlayEffectAtPosition(EffectPack effectPack, Vector3 position, int index = 0) => PlayEffect(effectPack, index, position);

        public void PlayRandomEffectAtPosition(EffectPack effectPack, Vector3 position) => PlayRandomEffect(effectPack, position);

        public void PlayEffectOnTransform(EffectPack effectPack, Transform parent, int index = 0) => PlayEffect(effectPack, index, null, parent);

        public void PlayRandomEffectOnTransform(EffectPack effectPack, Transform parent) => PlayRandomEffect(effectPack, null, parent);

        void SetupSource(AudioSource source, EffectPack effectPack, Vector3? position, Transform parent)
        {
            // Reset transform parent first
            source.transform.SetParent(transform);

            // Set mixer group
            if (effectPack.mixerGroup != null)
                source.outputAudioMixerGroup = effectPack.mixerGroup;

            // Set volume
            source.volume = effectPack.playVolume;

            // Apply random pitch if enabled
            if (effectPack.useRandomPitch)
            {
                source.pitch = Random.Range(effectPack.pitchRange.x, effectPack.pitchRange.y);
            }
            else
            {
                source.pitch = 1f;
            }

            if (position.HasValue || parent != null)
            {
                // Configure 3D audio settings
                source.spatialBlend = effectPack.spatialBlend > 0 ? effectPack.spatialBlend : defaultSpatialBlend;
                source.rolloffMode = effectPack.rolloffMode;
                source.minDistance = effectPack.minDistance > 0 ? effectPack.minDistance : defaultMinDistance;
                source.maxDistance = effectPack.maxDistance > 0 ? effectPack.maxDistance : defaultMaxDistance;
                source.dopplerLevel = effectPack.dopplerLevel;

                if (parent != null)
                {
                    // Attach to parent transform
                    source.transform.SetParent(parent);

                    // Use position for position offset
                    if (position.HasValue)
                        source.transform.localPosition = position.Value;
                    else
                        source.transform.localPosition = Vector3.zero;
                }
                else if (position.HasValue)
                {
                    // Set position (world position)
                    source.transform.position = position.Value;
                }
            }
            else
            {
                // 2D Audio
                source.spatialBlend = 0f;
            }
        }

        int GetNextLayerIndex()
        {
            AudioSource next = sourcePool.Find(layer => !layer.isPlaying);
            if (next == null || sourcePool.Count <= 0)
            {
                next = Instantiate(sourcePrefab, transform).GetComponent<AudioSource>();
                next.name = "[Effect] Source";
                sourcePool.Add(next);
            }

            next.gameObject.SetActive(true);
            return sourcePool.IndexOf(next);
        }
    }
}