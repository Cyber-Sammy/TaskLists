namespace TaskLists.Api.Models.Responses;

public sealed record ErrorItemResponse(
    string Message,
    string? Code);
