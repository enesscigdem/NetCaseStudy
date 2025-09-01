using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using NetCaseStudy.Application.Localization;

namespace NetCaseStudy.Api.Infrastructure.Middlewares
{
    public class ProblemDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly IStringLocalizer<SharedResource> _localizer;
        private readonly ILogger<ProblemDetailsMiddleware> _logger;

        public ProblemDetailsMiddleware(RequestDelegate next, IStringLocalizer<SharedResource> localizer, ILogger<ProblemDetailsMiddleware> logger)
        {
            _next = next;
            _localizer = localizer;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var traceId = context.TraceIdentifier;
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                context.Response.ContentType = MediaTypeNames.Application.Json;
                context.Response.Headers["X-Correlation-Id"] = traceId;

                _logger.LogError(ex, "Unhandled exception. TraceId: {TraceId}", traceId);

                var pd = new ProblemDetails
                {
                    Title = _localizer["UnexpectedErrorTitle"],
                    Detail = _localizer["UnexpectedErrorDetail"],
                    Status = StatusCodes.Status500InternalServerError,
                    Instance = context.Request.Path
                };
                pd.Extensions["traceId"] = traceId;

                await context.Response.WriteAsJsonAsync(pd);
            }
        }
    }
}