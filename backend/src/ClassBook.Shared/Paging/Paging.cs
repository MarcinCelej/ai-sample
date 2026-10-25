using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace ClassBook.Shared.Paging;

public static class Paging
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;

    public const string PageInvalidCode = "paging.page.invalid";
    public const string PageSizeInvalidCode = "paging.pageSize.invalid";

    /// <summary>Validation rule for a 1-based page number.</summary>
    public static IRuleBuilderOptions<T, int> ValidPage<T>(this IRuleBuilder<T, int> rule) =>
        rule.GreaterThanOrEqualTo(1).WithErrorCode(PageInvalidCode);

    /// <summary>Validation rule for a page size between 1 and <see cref="MaxPageSize"/>.</summary>
    public static IRuleBuilderOptions<T, int> ValidPageSize<T>(this IRuleBuilder<T, int> rule) =>
        rule.InclusiveBetween(1, MaxPageSize).WithErrorCode(PageSizeInvalidCode);

    /// <summary>Executes the ordered query for one page and the total count.</summary>
    public static async Task<PagedResponse<T>> ToPagedResponseAsync<T>(
        this IQueryable<T> orderedQuery, int page, int pageSize, CancellationToken cancellationToken)
    {
        var totalCount = await orderedQuery.CountAsync(cancellationToken);
        var items = await orderedQuery.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return new PagedResponse<T>(items, totalCount);
    }
}
