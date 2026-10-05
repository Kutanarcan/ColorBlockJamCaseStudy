using System;

namespace Game.Core
{
    // Result with a value on success. Same rule as Result: authored data that is
    // wrong comes back as a failure, wiring bugs still throw.
    //
    // Success is tracked by the value, so default(Result<T>) is a failure that
    // still says what went wrong.
    public readonly struct Result<T>
    {
        private const string UnassignedError = "The result was never assigned.";

        private readonly T value;
        private readonly string error;
        private readonly bool hasValue;

        public bool IsSuccess => hasValue;
        public bool IsFailure => !hasValue;
        public string Error => hasValue ? null : error ?? UnassignedError;

        public T Value
        {
            get
            {
                if (!hasValue)
                {
                    throw new InvalidOperationException($"A failed result has no value. {Error}");
                }

                return value;
            }
        }

        private Result(T value, string error, bool hasValue)
        {
            this.value = value;
            this.error = error;
            this.hasValue = hasValue;
        }

        public static Result<T> Success(T value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value), "A success has to carry its value.");
            }

            return new Result<T>(value, null, true);
        }

        public static Result<T> Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("A failure has to say what went wrong.", nameof(error));
            }

            return new Result<T>(default, error, false);
        }
    }
}
