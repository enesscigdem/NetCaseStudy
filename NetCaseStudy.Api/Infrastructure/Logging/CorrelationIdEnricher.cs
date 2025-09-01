using Serilog.Core;
using Serilog.Events;

namespace NetCaseStudy.Api.Infrastructure.Logging
{
    public class CorrelationIdEnricher : ILogEventEnricher
    {
        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            var traceId = System.Diagnostics.Activity.Current?.Id;
            if (!string.IsNullOrWhiteSpace(traceId))
            {
                logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("TraceId", traceId));
            }
        }
    }
}