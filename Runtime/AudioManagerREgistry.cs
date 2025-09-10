using UnityEngine;

namespace Quirks.Audio
{
    /// <summary>
    /// Global registry for audio managers. 
    /// Provides a static reference point for AudioPacks to find the correct AudioManager implementation to use.
    /// </summary>
    public static class AudioManagerRegistry
    {
        static IAudioManager currentManager;

        /// <summary>Gets the currently registered audio manager.</summary>
        public static IAudioManager Current
        {
            get
            {
                if (currentManager == null)
                {
                    // Try to find the default AudioManager in the scene
                    IAudioManager defaultManager = (IAudioManager)Object.FindFirstObjectByType<AudioManager>();
                    if (defaultManager != null)
                    {
                        Register(defaultManager);
                    }
                    else
                    {
                        Debug.LogWarning("No AudioManager found in scene and none registered. Audio playback will not work.");
                    }
                }

                return currentManager;
            }
        }

        /// <summary>Checks if an audio manager is currently registered and available.</summary>
        public static bool IsAvailable => Current != null;

        /// <summary>Registers an audio manager as the current global instance.</summary>
        /// <param name="manager">The audio manager to register</param>
        public static void Register(IAudioManager manager)
        {
            if (manager == null)
            {
                Debug.LogError("Cannot register null audio manager.");
                return;
            }

            if (currentManager != null && currentManager != manager)
            {
                Debug.LogWarning($"Replacing existing AudioManager ({currentManager.GetType().Name}) with new one ({manager.GetType().Name})");
            }

            currentManager = manager;
        }

        /// <summary>Unregisters the current audio manager if it matches the provided manager.</summary>
        /// <param name="manager">The manager to unregister</param>
        public static void Unregister(IAudioManager manager)
        {
            if (currentManager == manager)
                currentManager = null;
        }

        /// <summary>Forces a refresh of the current manager (useful for scene changes).</summary>
        public static void Refresh()
        {
            currentManager = null;
            _ = Current;
        }
    }
}