using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace NetCaseStudy.Api.Infrastructure.Swagger
{
    public class AcceptLanguageHeaderOperationFilter : IOperationFilter
    {
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            operation.Parameters ??= new List<OpenApiParameter>();
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Schema = new OpenApiSchema { Type = "string", Default = new Microsoft.OpenApi.Any.OpenApiString("en-US") },
                Description = "UI culture (e.g., en-US, tr-TR)"
            });
        }
    }
}