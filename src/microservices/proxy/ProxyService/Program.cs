var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var app = builder.Build();

var monolithUrl = Environment.GetEnvironmentVariable("MONOLITH_URL") ?? "http://localhost:8080";
var moviesServiceUrl = Environment.GetEnvironmentVariable("MOVIES_SERVICE_URL") ?? "http://localhost:8081";
var eventsServiceUrl = Environment.GetEnvironmentVariable("EVENTS_SERVICE_URL") ?? "http://localhost:8082";
var moviesMigrationPercent = int.Parse(Environment.GetEnvironmentVariable("MOVIES_MIGRATION_PERCENT") ?? "0");

var random = new Random();

app.Map("{**path}", async (HttpContext context, IHttpClientFactory factory) =>
{
    var client = factory.CreateClient();
    var path = context.Request.Path + context.Request.QueryString;
    string targetUrl;

    if (context.Request.Path.StartsWithSegments("/api/movies"))
    {
        var roll = random.Next(100);
        targetUrl = roll < moviesMigrationPercent
            ? $"{moviesServiceUrl}{path}"
            : $"{monolithUrl}{path}";
    }
    else if (context.Request.Path.StartsWithSegments("/api/events"))
    {
        targetUrl = $"{eventsServiceUrl}{path}";
    }
    else
    {
        targetUrl = $"{monolithUrl}{path}";
    }

    var request = new HttpRequestMessage
    {
        Method = new HttpMethod(context.Request.Method),
        RequestUri = new Uri(targetUrl)
    };

    if (context.Request.ContentLength > 0)
    {
        request.Content = new StreamContent(context.Request.Body);
        if (context.Request.ContentType != null)
            request.Content.Headers.ContentType =
                new System.Net.Http.Headers.MediaTypeHeaderValue(context.Request.ContentType);
    }

    try
    {
        var response = await client.SendAsync(request);
        context.Response.StatusCode = (int)response.StatusCode;
        context.Response.ContentType = "application/json";
        await response.Content.CopyToAsync(context.Response.Body);
    }
    catch (Exception ex)
    {
        context.Response.StatusCode = 502;
        await context.Response.WriteAsync($"Gateway error: {ex.Message}");
    }
});

app.Run();