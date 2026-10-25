using ClassBook.Shared.Validation;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClassBook.UnitTests.Shared;

public class ValidationFilterTests
{
    private static readonly EndpointFilterDelegate Next = _ => ValueTask.FromResult<object?>("handler result");

    private readonly ValidationFilter<SampleRequest> _filter = new(new SampleValidator());

    [Fact]
    public async Task InvokeAsync_ValidRequest_CallsNext()
    {
        var result = await _filter.InvokeAsync(Context(new SampleRequest("ok", new SampleAddress("00-001"))), Next);

        result.ShouldBe("handler result");
    }

    [Fact]
    public async Task InvokeAsync_NoRequestArgument_CallsNext()
    {
        var result = await _filter.InvokeAsync(Context("something else"), Next);

        result.ShouldBe("handler result");
    }

    [Fact]
    public async Task InvokeAsync_InvalidRequest_ReturnsValidationProblemWithCamelCaseFieldsAndCodes()
    {
        var result = await _filter.InvokeAsync(Context(new SampleRequest("", new SampleAddress(""))), Next);

        var problem = result.ShouldBeOfType<ValidationProblem>().ProblemDetails;
        problem.Errors.Keys.ShouldBe(["name", "address.postalCode"], ignoreOrder: true);
        problem.Extensions["code"].ShouldBe(ValidationFilter<SampleRequest>.ValidationFailedCode);
        var codes = problem.Extensions[ValidationFilter<SampleRequest>.ErrorCodesExtension].ShouldBeOfType<Dictionary<string, string[]>>();
        codes["name"].ShouldBe(["sample.name.required"]);
        codes["address.postalCode"].ShouldBe(["sample.postalCode.required"]);
    }

    private static DefaultEndpointFilterInvocationContext Context(params object[] arguments) =>
        new(new DefaultHttpContext(), arguments);

    public sealed record SampleRequest(string Name, SampleAddress Address);

    public sealed record SampleAddress(string PostalCode);

    private sealed class SampleValidator : AbstractValidator<SampleRequest>
    {
        public SampleValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithErrorCode("sample.name.required");
            RuleFor(x => x.Address.PostalCode).NotEmpty().WithErrorCode("sample.postalCode.required");
        }
    }
}
