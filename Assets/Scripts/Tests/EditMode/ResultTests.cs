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

        [Test]
        public void ValueResult_Success_CarriesItsValue()
        {
            Result<string> result = Result<string>.Success("level");

            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value, Is.EqualTo("level"));
            Assert.That(result.Error, Is.Null);
        }

        [Test]
        public void ValueResult_Failure_HasNoValue()
        {
            Result<string> result = Result<string>.Failure("broken");

            Assert.That(result.Error, Is.EqualTo("broken"));
            Assert.Throws<InvalidOperationException>(() => _ = result.Value);
        }

        [Test]
        public void ValueResult_Default_IsFailure_WithAMessage()
        {
            Result<string> result = default;

            Assert.That(result.IsFailure, Is.True);
            Assert.That(result.Error, Is.Not.Empty);
        }
    }
}
