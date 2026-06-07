namespace TaskLists.SharedKernel.Result;

public class Result
{
    private static readonly IReadOnlyCollection<ResultError> _emptyErrors = Array.Empty<ResultError>();

    public bool Successful => Status == ResultStatus.Success;

    public ResultStatus Status { get; }

    public IReadOnlyCollection<ResultError> Errors { get; }

    public string? Message => Errors.Count == 0
        ? null
        : string.Join("; ", Errors.Select(error => error.Message));

    protected Result(ResultStatus status, IReadOnlyCollection<ResultError> errors)
    {
        if (status == ResultStatus.Success && errors.Count > 0)
        {
            throw new ArgumentException(ResultMessages.SuccessHasErrors, nameof(errors));
        }

        if (status != ResultStatus.Success && errors.Count == 0)
        {
            throw new ArgumentException(ResultMessages.FailHasNoErrors, nameof(errors));
        }

        Status = status;
        Errors = errors;
    }

    public sealed record ResultError(string Message, string? Code = null);

    public enum ResultStatus
    {
        Success = 0,
        ValidationError = 1,
        NotFound = 2,
        Forbidden = 3,
        Conflict = 4,
        Error = 5
    }

    public static Result Success()
    {
        return new Result(ResultStatus.Success, _emptyErrors);
    }

    public static Result ValidationError(string message, string? code = null)
    {
        return Failure(ResultStatus.ValidationError, message, code);
    }

    public static Result NotFound(string message, string? code = null)
    {
        return Failure(ResultStatus.NotFound, message, code);
    }

    public static Result Forbidden(string message, string? code = null)
    {
        return Failure(ResultStatus.Forbidden, message, code);
    }

    public static Result Conflict(string message, string? code = null)
    {
        return Failure(ResultStatus.Conflict, message, code);
    }

    public static Result Error(string message, string? code = null)
    {
        return Failure(ResultStatus.Error, message, code);
    }

    public static Result Failure(ResultStatus status, string message, string? code = null)
    {
        return Failure(status, [new ResultError(message, code)]);
    }

    public static Result Failure(ResultStatus status, IReadOnlyCollection<ResultError> errors)
    {
        return new Result(status, errors);
    }

}

public sealed class Result<T> : Result
{
    public T Value
    {
        get
        {
            if (!Successful)
            {
                throw new InvalidOperationException(ResultMessages.FailHasNoValue);
            }

            return field!;
        }
    }

    private Result(T value)
        : base(ResultStatus.Success, Array.Empty<ResultError>())
    {
        Value = value;
    }

    private Result(ResultStatus status, IReadOnlyCollection<ResultError> errors)
        : base(status, errors)
    {
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(value);
    }

    public static new Result<T> ValidationError(string message, string? code = null)
    {
        return Failure(ResultStatus.ValidationError, message, code);
    }

    public static new Result<T> NotFound(string message, string? code = null)
    {
        return Failure(ResultStatus.NotFound, message, code);
    }

    public static new Result<T> Forbidden(string message, string? code = null)
    {
        return Failure(ResultStatus.Forbidden, message, code);
    }

    public static new Result<T> Conflict(string message, string? code = null)
    {
        return Failure(ResultStatus.Conflict, message, code);
    }

    public static new Result<T> Error(string message, string? code = null)
    {
        return Failure(ResultStatus.Error, message, code);
    }

    public static new Result<T> Failure(ResultStatus status, string message, string? code = null)
    {
        return Failure(status, [new ResultError(message, code)]);
    }

    public static new Result<T> Failure(ResultStatus status, IReadOnlyCollection<ResultError> errors)
    {
        return new Result<T>(status, errors);
    }
}

internal static class ResultMessages
{
    public const string SuccessHasErrors = "Successful result cannot contain errors.";
    public const string FailHasNoErrors = "Failed result must contain at least one error.";
    public const string FailHasNoValue = "Failed result does not contain a value.";
}
