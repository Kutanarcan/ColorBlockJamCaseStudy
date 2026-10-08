using System;

namespace Game.Meta
{
    /// <summary>
    /// The player's settings, on by default and saved on every change (D126). Only the flags live here; what a
    /// flag does (SFX muting) is the reader's business. Its section is read on first use.
    /// </summary>
    public sealed class Settings
    {
        public const string SaveKey = "settings";

        private readonly ISaveStore store;
        private SettingsData data;

        public Settings(ISaveStore store) => this.store = store;

        private SettingsData Data => data ??= store.Load<SettingsData>(SaveKey);

        public bool IsOn(Setting setting)
        {
            switch (setting)
            {
                case Setting.Vibration: return Data.vibration;
                case Setting.Sound: return Data.sound;
                case Setting.Music: return Data.music;
                default: throw new ArgumentOutOfRangeException(nameof(setting), setting, null);
            }
        }

        public void Set(Setting setting, bool on)
        {
            if (IsOn(setting) == on)
                return;

            switch (setting)
            {
                case Setting.Vibration: data.vibration = on; break;
                case Setting.Sound: data.sound = on; break;
                case Setting.Music: data.music = on; break;
            }

            store.Save(SaveKey, data);
        }
    }
}
