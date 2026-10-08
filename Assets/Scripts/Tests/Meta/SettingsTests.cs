using Game.Meta;
using NUnit.Framework;

namespace Game.Tests.Meta
{
    public sealed class SettingsTests
    {
        [TestCase(Setting.Vibration)]
        [TestCase(Setting.Sound)]
        [TestCase(Setting.Music)]
        public void Settings_AreOn_InAFreshSave(Setting setting)
        {
            Assert.That(new Settings(new FakeSaveStore()).IsOn(setting), Is.True);
        }

        [TestCase(Setting.Vibration)]
        [TestCase(Setting.Sound)]
        [TestCase(Setting.Music)]
        public void SettingsToggles_PersistAcrossARestart(Setting setting)
        {
            var store = new FakeSaveStore();
            new Settings(store).Set(setting, false);

            var next = new Settings(store);

            Assert.That(next.IsOn(setting), Is.False);
            Assert.That(store.WritesOf(Settings.SaveKey), Is.EqualTo(1));
        }

        [Test]
        public void Settings_ChangeOnlyTheirOwnFlag_AndOnlyTheirOwnSection()
        {
            var store = new FakeSaveStore();
            var settings = new Settings(store);

            settings.Set(Setting.Sound, false);

            Assert.That(settings.IsOn(Setting.Vibration), Is.True);
            Assert.That(settings.IsOn(Setting.Music), Is.True);
            Assert.That(store.Has(Progression.SaveKey), Is.False);
        }

        [Test]
        public void Settings_DoNotWrite_WhenAFlagKeepsItsValue()
        {
            var store = new FakeSaveStore();

            new Settings(store).Set(Setting.Music, true);

            Assert.That(store.WritesOf(Settings.SaveKey), Is.Zero);
        }
    }
}
