using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class TimerTextTests
    {
        [TestCase(300f, 5, 0)]
        [TestCase(299.2f, 5, 0)]
        [TestCase(299f, 4, 59)]
        [TestCase(61f, 1, 1)]
        [TestCase(59.01f, 1, 0)]
        [TestCase(0.4f, 0, 1)]
        public void TimerText_ShowsMinutesAndSeconds_AndStopsAtZero(float remaining, int minutes, int seconds)
        {
            var text = TimerText.From(remaining);

            Assert.That(text.Minutes, Is.EqualTo(minutes));
            Assert.That(text.Seconds, Is.EqualTo(seconds));
        }

        [TestCase(0f)]
        [TestCase(-3.5f)]
        public void TimerText_ShowsZero_OnceTheTimeIsOut(float remaining)
        {
            var text = TimerText.From(remaining);

            Assert.That(text, Is.EqualTo(TimerText.From(0f)));
            Assert.That(text.Minutes, Is.Zero);
            Assert.That(text.Seconds, Is.Zero);
        }
    }
}
