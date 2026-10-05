using System;

namespace Game.Core
{
    // Two ways of saying no live in this codebase, and which one a method uses
    // is decided by who made the mistake rather than by how bad it is:
    //
    //   Result     -- authored data is wrong. A level file, an Inspector field:
    //                 a person wrote it, so the game reports it and carries on.
    //   exception  -- the code is wired wrong. A null collaborator, a negative
    //                 capacity: a bug, and it belongs where it was wired.
    //
    // Success is derived from Error, so default(Result) is a success and a
    // failure without a message cannot exist.
    public readonly struct Result
    {
        public string Error { get; }

        public bool IsSuccess => Error == null;
        public bool IsFailure => Error != null;

        private Result(string error)
        {
            Error = error;
        }

        public static Result Success()
        {
            return default;
        }

        public static Result Failure(string error)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                throw new ArgumentException("A failure has to say what went wrong.", nameof(error));
            }

            return new Result(error);
        }
    }
}
