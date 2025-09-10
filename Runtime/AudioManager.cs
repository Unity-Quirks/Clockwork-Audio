using UnityEngine;

namespace Quirks.Audio
{
    public class AudioManager : MonoBehaviour, IAudioManager
    {
        static AudioManager instance; // Singleton instance of AudioManager.
        public static AudioManager Instance => instance;

        [Header("Audio Systems")]
        public MusicManager musicManager;
        public EffectManager effectManager;
        public LoopManager loopManager;

        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                transform.SetParent(null);
                DontDestroyOnLoad(this.gameObject);
            }
            else if (instance != this)
            {
                DestroyImmediate(this);
                return;
            }

            AudioManagerRegistry.Register(this);
        }

        #region Music

        public void PlayMusic(MusicPack musicPack, int clipIndex = 0, float startTime = 0f, float blendOutTime = 1f, float blendInTime = 1f)
        {
            if (musicManager == null)
                return;

            musicManager.PlayMusic(musicPack, clipIndex, startTime, blendOutTime, blendInTime);
        }

        public void StopMusic(float fadeOutDuration = 1f)
        {
            if (musicManager == null)
                return;

            musicManager.StopMusic(fadeOutDuration);
        }

        #endregion

        #region Effect Manager

        /// <summary>
        /// Plays the specified audio effect from an EffectPack at a given index.
        /// </summary>
        public void PlayEffect(EffectPack effectPack, int index = 0, Vector3? position = null, Transform parent = null)
        {
            if (effectManager == null)
                return;

            effectManager.PlayEffect(effectPack, index, position, parent);
        }

        /// <summary>
        /// Plays a random audio effect from an EffectPack.
        /// </summary>
        public void PlayRandomEffect(EffectPack effectPack, Vector3? position = null, Transform parent = null)
        {
            if (effectManager == null)
                return;

            effectManager.PlayRandomEffect(effectPack, position, parent);
        }

        /// <summary>Plays an effect at a specific world position with 3D audio.</summary>
        public void PlayEffectAtPosition(EffectPack effectPack, Vector3 position, int index = 0) => PlayEffect(effectPack, index, position);

        public void PlayRandomEffectAtPosition(EffectPack effectPack, Vector3 position) => PlayRandomEffect(effectPack, position);

        public void PlayEffectOnTransform(EffectPack effectPack, Transform parent, int index = 0) => PlayEffect(effectPack, index, null, parent);

        public void PlayRandomEffectOnTransform(EffectPack effectPack, Transform parent) => PlayRandomEffect(effectPack, null, parent);

        #endregion

        #region Loop Manager

        /// <summary>
        /// Starts a looping audio effect.
        /// </summary>
        /// <param name="loopPack">The loop pack to play</param>
        /// <param name="clipIndex">Index of the clip to start with</param>
        /// <param name="fadeInTime">Time to fade in</param>
        /// <param name="startVolume">Starting volume</param>
        /// <param name="position">Optional world position for 3D audio</param>
        /// <param name="parent">Optional parent transform</param>
        /// <returns>LoopHandle for controlling the loop</returns>
        public LoopHandle StartLoop(LoopPack loopPack, int clipIndex = 0, float fadeInTime = 0.5f, float startVolume = 1f, Vector3? position = null, Transform parent = null)
        {
            if (loopManager == null)
                return null;

            return loopManager.StartLoop(loopPack, clipIndex, fadeInTime, startVolume, position, parent);
        }

        /// <summary>Stops all active loops with fade out.</summary>
        /// <param name="fadeOutTime">Time to fade out all loops</param>
        public void StopAllLoops(float fadeOutTime = 1f)
        {
            if (loopManager == null)
                return;

            loopManager.StopAllLoops(fadeOutTime);
        }

        /// <summary>Immediately stops all loops without fading.</summary>
        public void StopAllLoopsImmediate()
        {
            if (loopManager == null)
                return;

            loopManager.StopAllLoopsImmediate();
        }

        #endregion

        void OnDestroy()
        {
            AudioManagerRegistry.Unregister(this);

            if (instance == this)
                instance = null;
        }

#if UNITY_EDITOR

        void OnValidate()
        {
            if (musicManager == null)
            {
                musicManager = FindFirstObjectByType<MusicManager>();

                if (musicManager == null)
                    musicManager = gameObject.AddComponent<MusicManager>();
            }

            if (effectManager == null)
            {
                effectManager = FindFirstObjectByType<EffectManager>();

                if (effectManager == null)
                    effectManager = gameObject.AddComponent<EffectManager>();
            }

            if (loopManager == null)
            {
                loopManager = FindFirstObjectByType<LoopManager>();

                if (loopManager == null)
                    loopManager = gameObject.AddComponent<LoopManager>();
            }
        }

        [UnityEditor.MenuItem("GameObject/Audio/Audio Manager", false, 0)]
        static void CreateAudioManager(UnityEditor.MenuCommand menuCommand)
        {
            GameObject go = new GameObject("Audio Manager", typeof(AudioManager));
            UnityEditor.Selection.activeGameObject = go;
        }

#endif
    }

}
