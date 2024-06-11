namespace LeaveSystem.Shared;

using System;

public readonly struct Result<TValue, TError>
{
    private readonly TValue? _value;
    private readonly TError? _error;

    public Result(TError? error)
    {
        Value = value;
        Error = error;
        _success = success;
    }

    public bool IsOk => _success;

    public static Result<T, E> Ok(T v)
    {
        return new(v, default(E), true);
    }

    public static Result<T, E> Err(E e)
    {
        return new(default(T), e, false);
    }

    public static implicit operator Result<T, E>(T v) => new(v, default(E), true);
    public static implicit operator Result<T, E>(E e) => new(default(T), e, false);

    public R Match<R>(
            Func<T, R> success,
            Func<E, R> failure) =>
        _success ? success(Value) : failure(Error);
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

    public static implicit operator Result<TError>(TError e) => new(e);

    public TResult Match<TResult>(
            Func<TResult> success,
            Func<TError, TResult> failure) =>
    IsOk ? success() : failure(_error!);

    public Task<TResult> MatchAsync<TResult>(Func<Task<TResult>> success, Func<TError, Task<TResult>> failure) =>
        IsOk ? success() : failure(_error!);
}
