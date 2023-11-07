using UnityEngine;

namespace Quartzified.Audio
{
    [CreateAssetMenu(menuName = "Quartzified/Audio/Effect Pack", fileName = "New Effect Pack")]
    public class EffectPack : AudioPack
    {
        public void Play(int index = 0) => AudioManager.Instance.PlayEffect(this, index);

        public void PlayRandom() => AudioManager.Instance.PlayRandomEffect(this);
    }
}