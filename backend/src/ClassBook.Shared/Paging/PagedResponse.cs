namespace ClassBook.Shared.Paging;

/// <summary>One page of a list that can grow.</summary>
public sealed record PagedResponse<T>(IReadOnlyList<T> Items, int TotalCount);
