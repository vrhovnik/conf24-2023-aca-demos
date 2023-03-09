using System.Net;
using ITS.Core;

namespace ITS.Web.ReportApi.Authentication;

public class ApiKeyAuthMiddleware
{
    private readonly RequestDelegate next;
    private readonly IConfiguration configuration;

    public ApiKeyAuthMiddleware(RequestDelegate next, IConfiguration configuration)
    {
        this.next = next;
        this.configuration = configuration;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Headers.TryGetValue(AuthOptions.ApiKeyHeaderName, out var extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Api key is missing.");
            return;
        }

        var apiKey = configuration.GetValue<string>(AuthOptions.SectionName);
        if (!apiKey.Equals(extractedApiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            await context.Response.WriteAsync("Invalid Api Key.");
            return;
        }
        
        await next(context);
    }
}