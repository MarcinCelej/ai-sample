using ClassBook.Shared.Domain;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace ClassBook.Shared.Persistence;

/// <summary>Stores <see cref="Money"/> as <c>decimal(18,2)</c>; the currency is implicitly PLN.</summary>
public sealed class MoneyConverter() : ValueConverter<Money, decimal>(money => money.Amount, amount => Money.Pln(amount));
