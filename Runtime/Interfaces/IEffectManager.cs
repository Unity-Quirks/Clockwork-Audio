using UnityEngine;

namespace Quirks.Audio
{
    public interface IEffectManager
    {
        /// <summary>Plays the specified audio effect from an EffectPack at a given index.</summary>
        void PlayEffect(EffectPack effectPack, int index = 0, Vector3? position = null, Transform parent = null);

        /// <summary>Plays a random audio effect from an EffectPack.</summary>
        void PlayRandomEffect(EffectPack effectPack, Vector3? position = null, Transform parent = null);

        /// <summary>Plays an effect at a specific world position with 3D audio.</summary>
        void PlayEffectAtPosition(EffectPack effectPack, Vector3 position, int index = 0);

        /// <summary>Plays a random effect at a specific world position with 3D audio.</summary>
        void PlayRandomEffectAtPosition(EffectPack effectPack, Vector3 position);

        /// <summary>Plays an effect attached to a transform.</summary>
        void PlayEffectOnTransform(EffectPack effectPack, Transform parent, int index = 0);

        /// <summary>Plays a random effect attached to a transform.</summary>
        void PlayRandomEffectOnTransform(EffectPack effectPack, Transform parent);
    }
}