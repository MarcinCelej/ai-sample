using ClassBook.Shared.Domain;

namespace ClassBook.UnitTests.Shared;

public class MoneyTests
{
    [Theory]
    [InlineData(10.004, 10.00)]
    [InlineData(10.005, 10.00)] // banker's rounding: to the even neighbour
    [InlineData(10.015, 10.02)]
    [InlineData(10.016, 10.02)]
    public void Pln_RoundsToTwoDecimalsWithBankersRounding(decimal input, decimal expected)
    {
        Money.Pln(input).Amount.ShouldBe(expected);
    }

    [Fact]
    public void Add_And_Subtract_ReturnRoundedAmounts()
    {
        var a = Money.Pln(100.10m);
        var b = Money.Pln(0.25m);

        (a + b).ShouldBe(Money.Pln(100.35m));
        (a - b).ShouldBe(Money.Pln(99.85m));
        Money.Add(a, b).ShouldBe(a + b);
        Money.Subtract(a, b).ShouldBe(a - b);
    }

    [Fact]
    public void Zero_HasZeroAmount()
    {
        Money.Zero.Amount.ShouldBe(0m);
    }

    [Fact]
    public void ToString_ShowsAmountAndCurrency()
    {
        Money.Pln(1234.5m).ToString().ShouldBe("1234.50 PLN");
    }
}
