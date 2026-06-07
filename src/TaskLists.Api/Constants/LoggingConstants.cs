namespace TaskLists.Api.Constants;

internal static class LoggingConstants
{
    public const string CorrelationIdHeaderName = "X-Correlation-Id";
    public const string CorrelationIdPropertyName = "CorrelationId";
    public const string FilePathConfigurationKey = "ApplicationLogging:FilePath";
    public const string DefaultLogFilePath = "logs/tasklists-api-.log";
    public const string RequestLogMessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
}
