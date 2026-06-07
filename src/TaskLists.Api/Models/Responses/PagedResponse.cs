namespace TaskLists.Api.Models.Responses;

public sealed record PagedResponse<T>(
    IReadOnlyCollection<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    long TotalPages);
