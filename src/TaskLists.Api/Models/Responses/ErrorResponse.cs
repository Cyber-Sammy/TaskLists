namespace TaskLists.Api.Models.Responses;

public sealed record ErrorResponse(
    IReadOnlyCollection<ErrorItemResponse> Errors);
