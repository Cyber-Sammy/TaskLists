using Serilog.Context;
using TaskLists.Api.Constants;

namespace TaskLists.Api.Middleware;

public sealed class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = GetOrCreateCorrelationId(context);

        context.Response.Headers[LoggingConstants.CorrelationIdHeaderName] = correlationId;

        using (LogContext.PushProperty(LoggingConstants.CorrelationIdPropertyName, correlationId))
        {
            await _next(context);
        }
    }

    private static string GetOrCreateCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(LoggingConstants.CorrelationIdHeaderName, out var headerValues))
        {
            var headerValue = headerValues.FirstOrDefault();

            if (!string.IsNullOrWhiteSpace(headerValue))
            {
                return headerValue;
            }
        }

        return Guid.NewGuid().ToString("N");
    }
}
