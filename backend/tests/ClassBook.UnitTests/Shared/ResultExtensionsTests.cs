using ClassBook.Shared.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace ClassBook.UnitTests.Shared;

public class ResultExtensionsTests
{
    private static readonly Error NotFound = new("thing.notFound", "Thing not found.", ErrorType.NotFound);

    [Theory]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Forbidden, StatusCodes.Status403Forbidden)]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    public void ToStatusCode_MapsErrorType(ErrorType type, int expected)
    {
        type.ToStatusCode().ShouldBe(expected);
    }

    [Fact]
    public void ToStatusCode_UnknownType_Throws()
    {
        Should.Throw<ArgumentOutOfRangeException>(() => ((ErrorType)99).ToStatusCode());
    }

    [Fact]
    public void ToProblem_ReturnsProblemDetailsWithCode()
    {
        var problem = NotFound.ToProblem().ShouldBeOfType<ProblemHttpResult>().ProblemDetails;

        problem.Status.ShouldBe(StatusCodes.Status404NotFound);
        problem.Detail.ShouldBe("Thing not found.");
        problem.Extensions[ResultExtensions.CodeExtension].ShouldBe("thing.notFound");
    }

    [Fact]
    public void ToOkResult_MapsSuccessAndFailure()
    {
        Result<string>.Success("x").ToOkResult().ShouldBeOfType<Ok<string>>().Value.ShouldBe("x");
        Result<string>.Failure(NotFound).ToOkResult().ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void ToCreatedResult_MapsSuccessWithLocationAndFailure()
    {
        var created = Result<int>.Success(7).ToCreatedResult(id => $"/api/things/{id}").ShouldBeOfType<Created<int>>();
        created.Location.ShouldBe("/api/things/7");
        created.Value.ShouldBe(7);

        Result<int>.Failure(NotFound).ToCreatedResult(id => $"/api/things/{id}").ShouldBeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void ToNoContentResult_MapsSuccessAndFailure()
    {
        Result.Success().ToNoContentResult().ShouldBeOfType<NoContent>();
        Result.Failure(NotFound).ToNoContentResult().ShouldBeOfType<ProblemHttpResult>();
    }
}
