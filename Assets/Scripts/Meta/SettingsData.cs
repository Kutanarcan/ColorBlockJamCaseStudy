using System;

namespace Game.Meta
{
    /// <summary><see cref="Settings"/>' save section, every flag on by default. Public fields, because the store
    /// writes fields.</summary>
    [Serializable]
    public sealed class SettingsData
    {
        public int version = 1;
        public bool vibration = true;
        public bool sound = true;
        public bool music = true;
    }
}
