using System.Text.Json;
using ClassBook.Shared.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace ClassBook.UnitTests.Shared;

public class GlobalExceptionHandlerTests
{
    [Fact]
    public async Task TryHandleAsync_ConcurrencyConflict_Returns409WithCode()
    {
        var (status, body) = await HandleAsync(new DbUpdateConcurrencyException());

        status.ShouldBe(StatusCodes.Status409Conflict);
        body.GetProperty("code").GetString().ShouldBe(GlobalExceptionHandler.ConcurrencyConflictCode);
    }

    [Fact]
    public async Task TryHandleAsync_UnexpectedException_Returns500WithoutDetails()
    {
        var (status, body) = await HandleAsync(new InvalidOperationException("secret internals"));

        status.ShouldBe(StatusCodes.Status500InternalServerError);
        body.GetProperty("code").GetString().ShouldBe(GlobalExceptionHandler.UnexpectedErrorCode);
        body.GetRawText().ShouldNotContain("secret internals");
    }

    private static async Task<(int Status, JsonElement Body)> HandleAsync(Exception exception)
    {
        var services = new ServiceCollection().AddLogging().AddProblemDetails().BuildServiceProvider();
        var context = new DefaultHttpContext { RequestServices = services };
        context.Response.Body = new MemoryStream();
        var handler = new GlobalExceptionHandler(
            services.GetRequiredService<IProblemDetailsService>(), NullLogger<GlobalExceptionHandler>.Instance);

        (await handler.TryHandleAsync(context, exception, CancellationToken.None)).ShouldBeTrue();

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(context.Response.Body);
        return (context.Response.StatusCode, document.RootElement.Clone());
    }
}
