using System;
using Game.Meta;

namespace Game.Runtime
{
    /// <summary>
    /// Drives the Settings popup (D121, D126): each toggle shows its saved flag, and a press flips and saves it and
    /// slides the switch. Home and close go to the place's actions. Made once per popup; <see cref="Refresh"/> puts the
    /// saved flags back on the toggles before each open, without a slide.
    /// </summary>
    public sealed class SettingsPresenter : IDisposable
    {
        private readonly SettingsView view;
        private readonly Settings settings;
        private readonly ISettingsActions actions;
        private readonly Action flipVibration;
        private readonly Action flipSound;
        private readonly Action flipMusic;

        public SettingsPresenter(SettingsView view, Settings settings, ISettingsActions actions)
        {
            this.view = view;
            this.settings = settings;
            this.actions = actions;
            flipVibration = () => Flip(Setting.Vibration, view.Vibration);
            flipSound = () => Flip(Setting.Sound, view.Sound);
            flipMusic = () => Flip(Setting.Music, view.Music);

            view.ShowHome(actions.ShowsHome);
            view.Vibration.Clicked += flipVibration;
            view.Sound.Clicked += flipSound;
            view.Music.Clicked += flipMusic;
            view.HomeClicked += actions.Home;
            view.CloseClicked += actions.Close;
        }

        public void Refresh()
        {
            view.Vibration.Show(settings.IsOn(Setting.Vibration), false);
            view.Sound.Show(settings.IsOn(Setting.Sound), false);
            view.Music.Show(settings.IsOn(Setting.Music), false);
        }

        public void Dispose()
        {
            view.Vibration.Clicked -= flipVibration;
            view.Sound.Clicked -= flipSound;
            view.Music.Clicked -= flipMusic;
            view.HomeClicked -= actions.Home;
            view.CloseClicked -= actions.Close;
        }

        private void Flip(Setting setting, ToggleView toggle)
        {
            bool on = !settings.IsOn(setting);
            settings.Set(setting, on);
            toggle.Show(on, true);
        }
    }
}
