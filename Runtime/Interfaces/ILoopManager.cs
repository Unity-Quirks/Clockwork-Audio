using UnityEngine;

namespace Quirks.Audio
{
    public interface ILoopManager
    {
        /// <summary>Starts a looping audio effect.</summary>
        LoopHandle StartLoop(LoopPack loopPack, int clipIndex = 0, float fadeInTime = 0.5f, float startVolume = 1f, Vector3? position = null, Transform parent = null);

        /// <summary>Stops all active loops with fade out.</summary>
        void StopAllLoops(float fadeOutTime = 1f);

        /// <summary>Immediately stops all loops without fading.</summary>
        void StopAllLoopsImmediate();
    }
}