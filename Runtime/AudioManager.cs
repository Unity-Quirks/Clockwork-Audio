using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.Audio;

namespace Quartzified.Audio
{
    public class AudioManager : MonoBehaviour
    {
        static AudioManager instance;
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
        int currentMusicIndex = 0; // Current Music Index
        int currentMusicClipIndex = 0; // Current Music Clip Index

        double nextLoopStartTime = 0;
        public float musicTimeRemaining => nextLoopStartTime != 0f ? (float)(nextLoopStartTime - AudioSettings.dspTime) + (!loopCurrentMusic ? currentMusicPack.ReverbTail : 0f) : 0f;

        [Space]
        public bool playOnAwake = true;
        [Range(0f, 1f)] public float maxVolume = 1f;
        public float defaultMusicBlendDuration = 1f;
        public float defaultMusicClipBlendDuration = 1f;

        [Header("Effect Settings")]
        public GameObject effectSourcePrefab = null; // Effect Source Prefab which will be used in the source pool.

        List<AudioSource> effectSourcePool = new List<AudioSource>();
        int currentEffectSourceIndex = -1;
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

        public void PlayMusic(MusicPack musicPack)
        {
            currentMusicPack = musicPack;

            if (currentMusicPack == null) return;

        }

        #region Effect Manager

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

            playSource.clip = playClip;
            playSource.Play();
        }

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

            playSource.clip = playClip;
            playSource.Play();
        }

        int GetNextEffectLayerIndex()
        {
            AudioSource next = effectSourcePool.Find(layer => !layer.isPlaying);
            if(next == null || effectSourcePool.Count <= 0)
            {
                next = Instantiate(effectSourcePrefab, transform).GetComponent<AudioSource>();
                effectSourcePool.Add(next);
            }
            next.gameObject.SetActive(true);
            return effectSourcePool.IndexOf(next);
        }

        #endregion


#if UNITY_EDITOR
        [MenuItem("GameObject/Audio/Audio Manager", false, 0)]
        static void CreateAudioManager(MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Audio Manager", typeof(AudioManager));
        }
#endif
    }

}

