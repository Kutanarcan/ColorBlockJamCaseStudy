using Game.Meta;
using UnityEngine;

namespace Game.Runtime
{
    /// <summary>
    /// Plays each sound as a one-shot on one shared 2D <see cref="AudioSource"/> (FINDINGS). The sound setting is read
    /// at every play (D126), so a toggle in Settings acts on the next sound with nothing to tell.
    /// </summary>
    public sealed class AudioSfxPlayer : ISfxPlayer
    {
        private readonly AudioSource source;
        private readonly PresentationAssets assets;
        private readonly Settings settings;

        public AudioSfxPlayer(AudioSource source, PresentationAssets assets, Settings settings)
        {
            this.source = source;
            this.assets = assets;
            this.settings = settings;
        }

        public void Play(SoundEffect effect)
        {
            if (!settings.IsOn(Setting.Sound))
                return;

            AudioClip clip = assets.Sound(effect);

            if (clip != null)
                source.PlayOneShot(clip);
        }
    }
}
