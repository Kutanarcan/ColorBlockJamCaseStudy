using System;
using Game.Core;
using NUnit.Framework;

namespace Game.Tests.EditMode
{
    public class ResultTests
    {
        [Test]
        public void Default_IsSuccess()
        {
            Result result = default;

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Error, Is.Null);
        }

        [Test]
        public void Failure_CarriesItsError()
        {
            Result result = Result.Failure("broken");

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.EqualTo("broken"));
        }

        [Test]
        public void Failure_WithoutMessage_Throws()
        {
            Assert.Throws<ArgumentException>(() => Result.Failure(" "));
        }
    }
}
