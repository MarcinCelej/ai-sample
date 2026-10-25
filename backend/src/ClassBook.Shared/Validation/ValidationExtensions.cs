using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace ClassBook.Shared.Validation;

public static class ValidationExtensions
{
    /// <summary>Validates the <typeparamref name="TRequest"/> argument before the handler runs.</summary>
    public static RouteHandlerBuilder WithValidation<TRequest>(this RouteHandlerBuilder builder) =>
        builder.AddEndpointFilter<ValidationFilter<TRequest>>().ProducesValidationProblem();
}
