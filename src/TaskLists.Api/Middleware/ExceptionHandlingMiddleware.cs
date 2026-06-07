using TaskLists.Api.Models.Responses;

namespace TaskLists.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private const string UnexpectedErrorMessage = Constants.ApiConstants.UnexpectedErrorMessage;

    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, UnexpectedErrorMessage);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new ErrorResponse(
            [
                new ErrorItemResponse(UnexpectedErrorMessage, null)
            ]);

            await context.Response.WriteAsJsonAsync(response);
        }
    }
}
