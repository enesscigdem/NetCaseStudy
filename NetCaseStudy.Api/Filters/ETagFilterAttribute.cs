using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace NetCaseStudy.Api.Filters;

public class ETagFilterAttribute : ActionFilterAttribute
{
    public override void OnActionExecuted(ActionExecutedContext context)
    {
        if (context.Result is ObjectResult objectResult && objectResult.Value != null)
        {
            var json = JsonSerializer.Serialize(objectResult.Value);
            var eTag = Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(json)));
            var response = context.HttpContext.Response;
            response.Headers.ETag = $"\"{eTag}\"";

            var requestETag = context.HttpContext.Request.Headers.IfNoneMatch.ToString();
            if (requestETag == $"\"{eTag}\"")
            {
                context.Result = new StatusCodeResult(StatusCodes.Status304NotModified);
            }
        }
    }
}