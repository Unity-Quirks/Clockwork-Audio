using UnityEngine;

namespace Quirks.Audio
{
    [CreateAssetMenu(menuName = "Quirks/Audio/Music Pack", fileName = "New Music Pack")]
    public class MusicPack : AudioPack
    {
        [Header("Music Base")]
        [SerializeField] float reverbTail = 0f;
        public float ReverbTail => reverbTail;

        /// <summary>
        /// Plays the first music clip in the pack.
        /// </summary>
        public void Play() => AudioManager.Instance.PlayMusic(this);

        /// <summary>
        /// Plays the first music clip in the pack with a given Fade blend Time.
        /// </summary>
        public void Play(float blendOutTime, float blendInTime) => AudioManager.Instance.PlayMusic(this, 0, 0, blendOutTime, blendInTime);

        /// <summary>
        /// Plays a random music clip from this pack.
        /// </summary>
        public void PlayRandom() => AudioManager.Instance.PlayMusic(this, Random.Range(0, ClipCount));

        /// <summary>
        /// Plays a random music clip from this pack with a given Fade blend Time.
        /// </summary>
        public void PlayRandom(float blendOutTime, float blendInTime) => AudioManager.Instance.PlayMusic(this, Random.Range(0, ClipCount), 0, blendOutTime, blendInTime);
    }
}
