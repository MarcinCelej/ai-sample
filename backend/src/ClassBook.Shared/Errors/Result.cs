using System.Diagnostics.CodeAnalysis;

namespace ClassBook.Shared.Errors;

/// <summary>Outcome of an operation without a value: success, or an expected <see cref="Errors.Error"/>.</summary>
public class Result
{
    protected Result(Error? error) => Error = error;

    public Error? Error { get; }

    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess => Error is null;

    public static Result Success() => new(null);

    public static Result Failure(Error error) => new(error ?? throw new ArgumentNullException(nameof(error)));

    public static implicit operator Result(Error error) => Failure(error);
}
