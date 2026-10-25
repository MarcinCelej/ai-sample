using Microsoft.AspNetCore.Http;

namespace ClassBook.Shared.Errors;

/// <summary>The single place where <see cref="Result"/> and <see cref="Error"/> map to HTTP responses.</summary>
public static class ResultExtensions
{
    public const string CodeExtension = "code";

    public static int ToStatusCode(this ErrorType type) => type switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unknown error type."),
    };

    /// <summary>ProblemDetails (RFC 9457) with the error code in the <c>code</c> extension.</summary>
    public static IResult ToProblem(this Error error)
    {
        ArgumentNullException.ThrowIfNull(error);
        return TypedResults.Problem(
            detail: error.Message,
            statusCode: error.Type.ToStatusCode(),
            extensions: new Dictionary<string, object?> { [CodeExtension] = error.Code });
    }

    public static IResult ToOkResult<T>(this Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }

    public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> location)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(location);
        return result.IsSuccess ? TypedResults.Created(location(result.Value), result.Value) : result.Error.ToProblem();
    }

    public static IResult ToNoContentResult(this Result result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.IsSuccess ? TypedResults.NoContent() : result.Error.ToProblem();
    }
}
