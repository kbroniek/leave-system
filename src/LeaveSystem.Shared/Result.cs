namespace LeaveSystem.Shared;

using System;
public static class Result
{
    public static Result<TValue, TError> Ok<TValue, TError>(TValue v) => new(v);
    public static Result<TValue, TError> Err<TValue, TError>(TError e) => new(e);
}

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    public Result(TError? error)
    {
        _value = default;
        _error = error;
        IsOk = false;
    }
    public Result(TValue? value)
    {
        _value = value;
        _error = default;
        IsOk = true;
    }
    public bool IsOk { get; }

    public static implicit operator Result<TValue, TError>(TValue v) => new(v);
    public static implicit operator Result<TValue, TError>(TError e) => new(e);

    public TResult Match<TResult>(
            Func<TValue, TResult> success,
            Func<TError, TResult> failure) =>
        IsOk ? success(_value!) : failure(_error!);

    public Task<TResult> MatchAsync<TResult>(Func<TValue, Task<TResult>> success, Func<TError, Task<TResult>> failure) =>
        IsOk ? success(_value!) : failure(_error!);
}

public readonly struct Result<TError>
{
    public bool IsOk { get; }

    private readonly TError? _error;

    public Result(TError? error)
    {
        _error = error;
        IsOk = false;
    }

    public Result()
    {
        IsOk = true;
    }

    public static implicit operator Result<TError>(TError e) => new(e);

    public TResult Match<TResult>(
            Func<TResult> success,
            Func<TError, TResult> failure) =>
    IsOk ? success() : failure(_error!);

    public Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> success, Func<TError, Task<TResult>> failure) =>
        IsOk ? success() : failure(_error!);
    public void IfSuccess(Action success)
    {
        if (IsOk)
        {
            success();
        }
    }
    public void IfFailed(Action<TError> failure) => failure(_error!);
}
