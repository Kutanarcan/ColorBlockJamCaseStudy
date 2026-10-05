using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class LevelTimerTests
    {
        [Test]
        public void Tick_CountsDown_AndClampsAtZero()
        {
            var timer = new LevelTimer();
            timer.Reset(5f);

            timer.Tick(2f);
            Assert.That(timer.Remaining, Is.EqualTo(3f));
            Assert.That(timer.IsExpired, Is.False);

            timer.Tick(10f);
            Assert.That(timer.Remaining, Is.EqualTo(0f));
            Assert.That(timer.IsExpired, Is.True);
        }

        [Test]
        public void Add_ExtendsRemainingTime()
        {
            var timer = new LevelTimer();
            timer.Reset(0f);

            timer.Add(4f);

            Assert.That(timer.Remaining, Is.EqualTo(4f));
            Assert.That(timer.IsExpired, Is.False);
        }
    }
}
