using UnityEngine;

namespace Quirks.Audio
{
    [CreateAssetMenu(menuName = "Quirks/Audio/Loop Pack", fileName = "New Loop Pack")]
    public class LoopPack : AudioPack
    {
        [Header("Loop Settings")]
        [Tooltip("Default fade in time when starting the loop")]
        [Range(0f, 5f)] public float defaultFadeInTime = 0.5f;

        [Tooltip("Default fade out time when stopping the loop")]
        [Range(0f, 5f)] public float defaultFadeOutTime = 0.5f;

        [Tooltip("Should the loop automatically restart if it reaches the end (for non-seamless loops)")]
        public bool autoRestart = true;

        [Tooltip("Cross-fade time when switching between clips in the same loop pack")]
        [Range(0f, 2f)] public float crossfadeTime = 0.2f;

        /// <summary>
        /// Starts playing the first clip in the loop pack.
        /// </summary>
        /// <param name="fadeInTime">Time to fade in. Uses default if not specified.</param>
        /// <param name="startVolume">Initial volume to fade in to. Uses pack volume if not specified.</param>
        /// <returns>Loop handle for controlling the loop</returns>
        public LoopHandle Play(float? fadeInTime = null, float? startVolume = null, Vector3? position = null, Transform parent = null)
        {
            return AudioManagerRegistry.Current.StartLoop(this, 0, fadeInTime ?? defaultFadeInTime, startVolume ?? playVolume, position, parent);
        }

        /// <summary>
        /// Starts playing a specific clip in the loop pack.
        /// </summary>
        /// <param name="clipIndex">Index of the clip to play</param>
        /// <param name="fadeInTime">Time to fade in. Uses default if not specified.</param>
        /// <param name="startVolume">Initial volume to fade in to. Uses pack volume if not specified.</param>
        /// <returns>Loop handle for controlling the loop</returns>
        public LoopHandle Play(int clipIndex, float? fadeInTime = null, float? startVolume = null, Vector3? position = null, Transform parent = null)
        {
            return AudioManagerRegistry.Current.StartLoop(this, clipIndex, fadeInTime ?? defaultFadeInTime, startVolume ?? playVolume, position, parent);
        }

        /// <summary>
        /// Starts playing a random clip from the loop pack.
        /// </summary>
        /// <param name="fadeInTime">Time to fade in. Uses default if not specified.</param>
        /// <param name="startVolume">Initial volume to fade in to. Uses pack volume if not specified.</param>
        /// <returns>Loop handle for controlling the loop</returns>
        public LoopHandle PlayRandom(float? fadeInTime = null, float? startVolume = null, Vector3? position = null, Transform parent = null)
        {
            int randomIndex = Random.Range(0, ClipCount);
            return AudioManagerRegistry.Current.StartLoop(this, randomIndex, fadeInTime ?? defaultFadeInTime, startVolume ?? playVolume, position, parent);
        }

        /// <summary>
        /// Plays the loop at a specific world position.
        /// </summary>
        /// <param name="position">World position to play at</param>
        /// <param name="clipIndex">Clip index to play (0 by default)</param>
        /// <param name="fadeInTime">Fade in time</param>
        /// <param name="startVolume">Starting volume</param>
        /// <returns>Loop handle for controlling the loop</returns>
        public LoopHandle PlayAtPosition(Vector3 position, int clipIndex = 0, float? fadeInTime = null, float? startVolume = null)
        {
            return Play(clipIndex, fadeInTime, startVolume, position, null);
        }

        /// <summary>
        /// Plays the loop attached to a transform.
        /// </summary>
        /// <param name="parent">Transform to attach to</param>
        /// <param name="clipIndex">Clip index to play (0 by default)</param>
        /// <param name="fadeInTime">Fade in time</param>
        /// <param name="startVolume">Starting volume</param>
        /// <returns>Loop handle for controlling the loop</returns>
        public LoopHandle PlayOnTransform(Transform parent, int clipIndex = 0, float? fadeInTime = null, float? startVolume = null)
        {
            return Play(clipIndex, fadeInTime, startVolume, null, parent);
        }
    }
}