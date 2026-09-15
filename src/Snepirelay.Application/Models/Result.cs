namespace Snepirelay.Application.Models
{
    public class Result
    {
        private Result(bool isSuccess, string? error, IReadOnlyDictionary<string, string>? errorValues)
        {
            IsSuccess = isSuccess;
            Error = error;
            ErrorValues = errorValues;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public string? Error { get; }
        public IReadOnlyDictionary<string, string>? ErrorValues { get; }

        public static Result Success() => new(true, null, null);

        public static Result Failure(string error, IReadOnlyDictionary<string, string>? values = null) =>
            new(false, error, values);

        public static Result Failure(Result other) => new(false, other.Error, other.ErrorValues);

        public static Result Failure<T>(Result<T> other) => new(false, other.Error, other.ErrorValues);

        public static Result<T> Success<T>(T value) => Result<T>.Success(value);

        public static Result<T> Failure<T>(string error, IReadOnlyDictionary<string, string>? values = null) =>
            Result<T>.Failure(error, values);
    }
}
