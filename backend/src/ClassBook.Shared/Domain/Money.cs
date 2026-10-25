namespace ClassBook.Shared.Domain;

/// <summary>An amount in PLN, always rounded to 2 decimals (banker's rounding).</summary>
public readonly record struct Money
{
    public const string Currency = "PLN";

    private Money(decimal amount) => Amount = decimal.Round(amount, 2, MidpointRounding.ToEven);

    public decimal Amount { get; }

    public static Money Zero => new(0m);

    public static Money Pln(decimal amount) => new(amount);

    public static Money operator +(Money a, Money b) => new(a.Amount + b.Amount);

    public static Money operator -(Money a, Money b) => new(a.Amount - b.Amount);

    public static Money Add(Money a, Money b) => a + b;

    public static Money Subtract(Money a, Money b) => a - b;

    public override string ToString() => $"{Amount:0.00} {Currency}";
}
