
namespace Quirks.Audio
{
    public interface IMusicManager
    {
        /// <summary>Plays the specified audio track from a MusicPack at a given index.</summary>
        void PlayMusic(MusicPack musicPack, int clipIndex = 0, float startTime = 0f, float blendOutTime = 1f, float blendInTime = 1f);

        /// <summary>Stops the currently playing music with optional fade out.</summary>
        void StopMusic(float fadeOutDuration = 1f);
    }
}