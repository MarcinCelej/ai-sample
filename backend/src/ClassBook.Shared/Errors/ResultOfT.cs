namespace ClassBook.Shared.Errors;

/// <summary>Outcome of an operation: a value, or an expected <see cref="Errors.Error"/>.</summary>
public sealed class Result<T> : Result
{
    private readonly T? _value;

    private Result(T value)
        : base(null) => _value = value;

    private Result(Error error)
        : base(error)
    {
    }

    /// <summary>The value of a successful result. Throws when the result is a failure.</summary>
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");

#pragma warning disable CA1000 // Factory methods on the generic type are the intended API.
    public static Result<T> Success(T value) => new(value);

    public static new Result<T> Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));
#pragma warning restore CA1000

    public static implicit operator Result<T>(T value) => Success(value);

    public static implicit operator Result<T>(Error error) => Failure(error);
}
