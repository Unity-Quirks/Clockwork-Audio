using UnityEngine;

namespace Quirks.Audio
{
    [CreateAssetMenu(menuName = "Quirks/Audio/Effect Pack", fileName = "New Effect Pack")]
    public class EffectPack : AudioPack
    {
        /// <summary>
        /// Plays the audio effect at the specified index.
        /// </summary>
        /// <param name="index">Index of the audio clip to play. Default is 0.</param>
        public void Play(int index = 0) => AudioManagerRegistry.Current.PlayEffect(this, index);

        /// <summary>
        /// Plays a random audio effect from this pack.
        /// </summary>
        public void PlayRandom() => AudioManagerRegistry.Current.PlayRandomEffect(this);

        /// <summary>
        /// Plays the audio effect at the specified index at a world position.
        /// </summary>
        /// <param name="position">World position to play the sound at</param>
        /// <param name="index">Index of the audio clip to play. Default is 0.</param>
        public void PlayAtPosition(Vector3 position, int index = 0) => AudioManagerRegistry.Current.PlayEffectAtPosition(this, position, index);

        /// <summary>
        /// Plays a random audio effect from this pack at a world position.
        /// </summary>
        /// <param name="position">World position to play the sound at</param>
        public void PlayRandomAtPosition(Vector3 position) => AudioManagerRegistry.Current.PlayRandomEffectAtPosition(this, position);

        /// <summary>
        /// Plays the audio effect attached to a transform.
        /// </summary>
        /// <param name="parent">Transform to attach the audio source to</param>
        /// <param name="index">Index of the audio clip to play. Default is 0.</param>
        public void PlayOnTransform(Transform parent, int index = 0) => AudioManagerRegistry.Current.PlayEffectOnTransform(this, parent, index);

        /// <summary>
        /// Plays a random audio effect from this pack attached to a transform.
        /// </summary>
        /// <param name="parent">Transform to attach the audio source to</param>
        public void PlayRandomOnTransform(Transform parent) => AudioManagerRegistry.Current.PlayRandomEffectOnTransform(this, parent);
    }
}