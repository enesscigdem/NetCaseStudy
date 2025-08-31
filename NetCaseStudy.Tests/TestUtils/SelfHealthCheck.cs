using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace NetCaseStudy.Tests.TestUtils;

public class SelfHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context, CancellationToken cancellationToken = default)
        => Task.FromResult(HealthCheckResult.Healthy("OK"));
}