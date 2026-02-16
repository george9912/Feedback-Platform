using System;

namespace MyFeedbackPlatform.Shared.Result;

public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string? Error { get; }
    public T? Value { get; }

    protected Result(bool isSuccess, T? value, string? error)
    {
        if (isSuccess && error is not null) throw new InvalidOperationException();
        if (!isSuccess && value is not null) throw new InvalidOperationException();

        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null);
    public static Result<T> Failure(string error) => new Result<T>(false, default, error);
}

