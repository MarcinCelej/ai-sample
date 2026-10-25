using System.Text.Json;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ClassBook.Shared.Validation;

/// <summary>
/// Runs the FluentValidation validator of <typeparamref name="TRequest"/> and returns <c>400</c> ValidationProblemDetails
/// with English messages in <c>errors</c> and stable codes in <c>errorCodes</c>, both keyed by camelCase field name.
/// </summary>
internal sealed class ValidationFilter<TRequest>(IValidator<TRequest> validator) : IEndpointFilter
{
    public const string ErrorCodesExtension = "errorCodes";
    public const string ValidationFailedCode = "validation.failed";

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var request = context.Arguments.OfType<TRequest>().FirstOrDefault();
        if (request is null)
        {
            return await next(context);
        }

        var result = await validator.ValidateAsync(request, context.HttpContext.RequestAborted);
        if (result.IsValid)
        {
            return await next(context);
        }

        var byField = result.Errors.GroupBy(f => ToFieldName(f.PropertyName)).ToList();
        return TypedResults.ValidationProblem(
            errors: byField.ToDictionary(g => g.Key, g => g.Select(f => f.ErrorMessage).ToArray()),
            extensions: new Dictionary<string, object?>
            {
                ["code"] = ValidationFailedCode,
                [ErrorCodesExtension] = byField.ToDictionary(g => g.Key, g => g.Select(f => f.ErrorCode).ToArray()),
            });
    }

    // "Address.PostalCode" -> "address.postalCode", matching the JSON property names.
    private static string ToFieldName(string propertyName) =>
        string.Join('.', propertyName.Split('.').Select(JsonNamingPolicy.CamelCase.ConvertName));
}
