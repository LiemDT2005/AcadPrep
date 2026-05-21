using System;

namespace Domain.Common;

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public int Code { get; }

    protected Result(bool isSuccess, string? error, int code)
    {
        IsSuccess = isSuccess;
        Error = error;
        Code = code;
    }

    public static Result Success(int code = 200) => new(true, null, code);
    public static Result Failure(string error, int code = 400) => new(false, error, code);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, string? error, int code, T? value) 
        : base(isSuccess, error, code)
    {
        Value = value;
    }

    public static Result<T> Success(T value, int code = 200) => new(true, null, code, value);
    public static new Result<T> Failure(string error, int code = 400) => new(false, error, code, default);
    
    // Explicit conversion helper to allow direct string responses, etc.
    public static Result<T> Success(string message, T value, int code = 200) => new(true, message, code, value);
}
