namespace LeaveSystem.Shared;

using System;
public static class Result
{
    public static Result<TValue, TError> Ok<TValue, TError>(TValue v) => new(v, default, true);
    public static Result<TValue, TError> Err<TValue, TError>(TError e) => new(default, e, false);
}

public readonly struct Result<TValue, TError>
{
    private readonly bool success;
    public readonly TValue Value;
    public readonly TError Error;

    internal Result(TValue v, TError e, bool success)
    {
        Value = v;
        Error = e;
        this.success = success;
    }

    public bool IsOk => success;

    public static implicit operator Result<TValue, TError>(TValue v) => new(v, default, true);
    public static implicit operator Result<TValue, TError>(TError e) => new(default, e, false);

    public R Match<R>(
            Func<TValue, R> success,
            Func<TError, R> failure) =>
        this.success ? success(Value) : failure(Error);
}
