using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Events;
using System.Text.Json.Serialization;
using TaskLists.Api.Constants;
using TaskLists.Api.Extensions;
using TaskLists.Api.Middleware;

namespace TaskLists.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSerilog((context, services, loggerConfiguration) =>
        {
            var logFilePath = context.Configuration[LoggingConstants.FilePathConfigurationKey]
                ?? LoggingConstants.DefaultLogFilePath;

            loggerConfiguration
                .MinimumLevel.Information()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
                .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7);
        });

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });
        builder.Services.AddTaskLists(builder.Configuration);
        builder.Services.AddOpenApi(options =>
        {
            options.AddOperationTransformer((operation, context, cancellationToken) =>
            {
                if (context.Description.RelativePath?.StartsWith("api/users", StringComparison.OrdinalIgnoreCase) == true)
                {
                    return Task.CompletedTask;
                }

                operation.Parameters ??= [];

                operation.Parameters.Add(new OpenApiParameter
                {
                    Name = CurrentUserConstants.UserIdHeaderName,
                    In = ParameterLocation.Header,
                    Required = true,
                    Description = "Current user identifier.",
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.String,
                        Format = "uuid"
                    }
                });

                return Task.CompletedTask;
            });

            options.AddDocumentTransformer((document, context, cancellationToken) =>
            {
                document.Info.Title = ApiConstants.ServiceTitle;
                document.Info.Version = "v1";
                document.Info.Description = "Test task for HELSI.";

                return Task.CompletedTask;
            });
        });

        var app = builder.Build();

        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseSerilogRequestLogging(options =>
        {
            options.MessageTemplate = LoggingConstants.RequestLogMessageTemplate;
        });
        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.MapScalarApiReference(options =>
            {
                options.Title = ApiConstants.ServiceTitle;
                options.Theme = ScalarTheme.DeepSpace;
                options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
            });
        }

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
