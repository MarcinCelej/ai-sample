using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ClassBook.Shared.Errors;

/// <summary>
/// Turns unhandled exceptions into ProblemDetails: concurrency conflicts become <c>409</c>, everything else <c>500</c>
/// without exception details. The ProblemDetails service adds <c>traceId</c>.
/// </summary>
public sealed partial class GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public const string ConcurrencyConflictCode = "concurrency.conflict";
    public const string UnexpectedErrorCode = "server.unexpected";

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var (status, code, detail) = exception switch
        {
            DbUpdateConcurrencyException => (StatusCodes.Status409Conflict, ConcurrencyConflictCode, "The resource was changed by someone else. Reload and try again."),
            _ => (StatusCodes.Status500InternalServerError, UnexpectedErrorCode, (string?)null),
        };

        if (status == StatusCodes.Status500InternalServerError)
        {
            LogUnhandled(logger, exception);
        }

        httpContext.Response.StatusCode = status;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Detail = detail,
                Extensions = { [ResultExtensions.CodeExtension] = code },
            },
        });
    }

    [LoggerMessage(Level = LogLevel.Error, Message = "Unhandled exception")]
    private static partial void LogUnhandled(ILogger logger, Exception exception);
}
