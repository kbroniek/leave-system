namespace LeaveSystem.Shared;

using System;
public static class Result
{
    public static Result<TValue, TError> Ok<TValue, TError>(TValue v) => new(v, default, true);
    public static Result<TValue, TError> Error<TValue, TError>(TError e) => new(default, e, false);

    public static Result<TError> Ok<TError>() => Default;
    public static Result<TError> Error<TError>(TError e) => new(e, false);

    public class Empty();

    public static Empty Default { get; } = new Empty();
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

    public TResult Match<TResult>(
            Func<TValue, TResult> success,
            Func<TError, TResult> failure) =>
        this.success ? success(Value) : failure(Error);
}

public readonly struct Result<TError>
{
    private readonly bool success;
    public readonly TError Error;

    internal Result(TError e, bool success)
    {
        Error = e;
        this.success = success;
    }

    public bool IsOk => success;

    public static implicit operator Result<TError>(TError e) => new(e, false);
    public static implicit operator Result<TError>(Result.Empty _) => new(default, true);

    public TResult Match<TResult>(
            Func<TResult> success,
            Func<TError, TResult> failure) =>
        this.success ? success() : failure(Error);
}
