namespace Game.Runtime
{
    /// <summary>The one way gameplay plays a sound (P6.4); the sound setting (M3) sits behind it.</summary>
    public interface ISfxPlayer
    {
        void Play(SoundEffect effect);
    }
}
