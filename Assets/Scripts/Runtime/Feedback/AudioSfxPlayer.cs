using UnityEngine;

namespace Game.Runtime
{
    /// <summary>Plays each sound as a one-shot on one shared 2D <see cref="AudioSource"/> (FINDINGS).</summary>
    public sealed class AudioSfxPlayer : ISfxPlayer
    {
        private readonly AudioSource source;
        private readonly PresentationAssets assets;

        public AudioSfxPlayer(AudioSource source, PresentationAssets assets)
        {
            this.source = source;
            this.assets = assets;
        }

        public void Play(SoundEffect effect)
        {
            AudioClip clip = assets.Sound(effect);

            if (clip != null)
                source.PlayOneShot(clip);
        }
    }
}
