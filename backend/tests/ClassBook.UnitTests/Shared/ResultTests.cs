using ClassBook.Shared.Errors;

namespace ClassBook.UnitTests.Shared;

public class ResultTests
{
    private static readonly Error SomeError = new("some.error", "Some error.", ErrorType.Conflict);

    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        result.IsSuccess.ShouldBeTrue();
        result.Error.ShouldBeNull();
    }

    [Fact]
    public void Failure_CarriesError()
    {
        Result result = SomeError;

        result.IsSuccess.ShouldBeFalse();
        result.Error.ShouldBe(SomeError);
    }

    [Fact]
    public void Failure_WithNullError_Throws()
    {
        Should.Throw<ArgumentNullException>(() => Result.Failure(null!));
        Should.Throw<ArgumentNullException>(() => Result<int>.Failure(null!));
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        Result<int> result = 42;

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(42);
    }

    [Fact]
    public void GenericFailure_ValueThrows()
    {
        Result<int> result = SomeError;

        result.Error.ShouldBe(SomeError);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }
}
