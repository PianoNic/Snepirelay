namespace Snepirelay.Application.Models
{
    public class Result<T>
    {
        private Result(bool isSuccess, T? value, string? error, IReadOnlyDictionary<string, string>? errorValues)
        {
            IsSuccess = isSuccess;
            Value = value;
            Error = error;
            ErrorValues = errorValues;
        }

        public bool IsSuccess { get; }
        public bool IsFailure => !IsSuccess;
        public T? Value { get; }
        public string? Error { get; }
        public IReadOnlyDictionary<string, string>? ErrorValues { get; }

        public static Result<T> Success(T value) => new(true, value, null, null);

        public static Result<T> Failure(string error, IReadOnlyDictionary<string, string>? values = null) =>
            new(false, default, error, values);

        public static Result<T> Failure(Result other) => new(false, default, other.Error, other.ErrorValues);

        public static Result<T> Failure<TOther>(Result<TOther> other) =>
            new(false, default, other.Error, other.ErrorValues);
    }
}
