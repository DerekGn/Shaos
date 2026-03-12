using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Shaos
{
    public static class ProgramOpenTelemtry
    {
        public static void AddOpenTelemetry(this WebApplicationBuilder builder)
        {
            builder.Logging.AddOpenTelemetry(_ =>
            {
                _.IncludeFormattedMessage = true;
                _.IncludeScopes = true;
            });

            var otel = builder
                .Services
                .AddOpenTelemetry()
                .WithMetrics(_ =>
                {
                    // Metrics provider from OpenTelemetry
                    _.AddAspNetCoreInstrumentation();
                    // Metrics provides by ASP.NET Core in .NET
                    _.AddMeter("Microsoft.AspNetCore.Hosting");
                    _.AddMeter("Microsoft.AspNetCore.Server.Kestrel");
                })
                .WithTracing(_ =>
                {
                    _.AddAspNetCoreInstrumentation();
                    _.AddHttpClientInstrumentation();
                });

            var OtlpEndpoint = builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"];
            if (OtlpEndpoint != null)
            {
                otel.UseOtlpExporter();
            }
        }
    }
}
