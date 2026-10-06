using System.Collections.Generic;
using Game.Runtime;

namespace Game.Tests.Runtime
{
    /// <summary>Records every sound asked for, in order.</summary>
    internal sealed class FakeSfxPlayer : ISfxPlayer
    {
        public List<SoundEffect> Played { get; } = new List<SoundEffect>();

        public void Play(SoundEffect effect) => Played.Add(effect);
    }
}
