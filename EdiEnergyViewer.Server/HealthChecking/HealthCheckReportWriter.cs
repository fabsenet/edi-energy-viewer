using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Fabsenet.EdiEnergyViewer.HealthChecking;

public static class HealthCheckReportWriter
{
    private sealed record HealthCheckReportWriterExceptionInfo
    {
        public string? Type { get; set; }
        public string? Message { get; set; }
        public string[]? StackTrace { get; set; }
        public HealthCheckReportWriterExceptionInfo? InnerException { get; set; }
        public Dictionary<string, object>? Data { get; set; }
    }

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        Converters = { new JsonStringEnumConverter() },
        WriteIndented = true
    };

    public static async Task WriteReport(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            results = report.Entries.Select(e => new
            {
                key = e.Key,
                value = new
                {
                    status = e.Value.Status.ToString(),
                    duration = e.Value.Duration,
                    description = e.Value.Description,
                    data = e.Value.Data.Count > 0 ? e.Value.Data : null,
                    exception = e.Value.Exception == null ? null : MapException(e.Value.Exception)
                }
            })
        };

        var json = JsonSerializer.Serialize(result, _jsonSerializerOptions);
        await context.Response.WriteAsync(json);
    }

    private static HealthCheckReportWriterExceptionInfo MapException(Exception ex)
    {
        return new HealthCheckReportWriterExceptionInfo
        {
            Type = ex.GetType().FullName ?? "UnknownException",
            Message = ex.Message,
            StackTrace = ex.StackTrace?.Split(["\r\n", "\n"], StringSplitOptions.None),
            Data = ex.Data?.Count > 0
                ? ex.Data.Cast<DictionaryEntry>()
                    .ToDictionary(
                        d => d.Key?.ToString() ?? "null",
                        d => d.Value ?? "null")
                : null,
            InnerException = ex.InnerException != null ? MapException(ex.InnerException) : null
        };
    }
}
