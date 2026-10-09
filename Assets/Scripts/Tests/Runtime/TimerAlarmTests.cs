using Game.Runtime;
using NUnit.Framework;

namespace Game.Tests.Runtime
{
    public sealed class TimerAlarmTests
    {
        // Shown time rounds up (TimerText): 59.01 s left still shows 01:00.
        [TestCase(120f, TimerUrgency.None)]
        [TestCase(59.01f, TimerUrgency.None)]
        [TestCase(59f, TimerUrgency.Warning)]
        [TestCase(10.5f, TimerUrgency.Warning)]
        [TestCase(10f, TimerUrgency.Critical)]
        [TestCase(0.4f, TimerUrgency.Critical)]
        [TestCase(0f, TimerUrgency.None)]
        public void TimerAlarm_BeatsBelowTheWarning_AndFlashesInTheLastSeconds(float remaining, TimerUrgency expected)
        {
            var alarm = new TimerAlarm(warningSeconds: 60, criticalSeconds: 10);

            Assert.That(alarm.UrgencyOf(TimerText.From(remaining)), Is.EqualTo(expected));
        }

        [Test]
        public void TimerAlarm_RefusesACriticalTimeAboveTheWarning()
        {
            Assert.That(() => new TimerAlarm(warningSeconds: 10, criticalSeconds: 60), Throws.Exception);
        }
    }
}
