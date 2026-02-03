using TenantSaas.Abstractions.Tenancy;
using TenantSaas.Core.Tenancy;
using TenantSaas.Sample.Middleware;

var app = SampleApp.BuildApp(args);
app.Run();

public static class SampleApp
{
    public static WebApplication BuildApp(string[] args, bool enableOpenApi = true)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Register context accessor as singleton - all instances share static AsyncLocal storage
        // Register both interfaces pointing to the same instance for DI flexibility
        var accessor = new AmbientTenantContextAccessor();
        builder.Services.AddSingleton<ITenantContextAccessor>(accessor);
        builder.Services.AddSingleton<IMutableTenantContextAccessor>(accessor);

        // Register tenant attribution resolver
        builder.Services.AddSingleton<ITenantAttributionResolver, TenantAttributionResolver>();

        if (enableOpenApi)
        {
            // Add services to the container.
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
        }

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (enableOpenApi && app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        if (!app.Environment.IsEnvironment("Test"))
        {
            app.UseHttpsRedirection();
        }

        // Exception handling middleware MUST be first (outermost) to catch all unhandled exceptions
        // and convert them to standardized RFC 7807 Problem Details responses
        app.UseMiddleware<ProblemDetailsExceptionMiddleware>();

        // Add tenant context middleware after exception handling, before endpoints
        app.UseMiddleware<TenantContextMiddleware>();

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        // Test endpoint demonstrating multi-source tenant attribution enforcement.
        //
        // Attribution sources (in precedence order):
        // 1. Route parameter: {tenantId}
        // 2. Header: X-Tenant-Id
        //
        // Behavior:
        // - Both match → 200 OK (unambiguous, uses higher precedence source)
        // - Both provided but conflicting → 422 Unprocessable Entity (ambiguous attribution)
        // - Only one provided → 200 OK (unambiguous)
        // - Neither provided → 422 Unprocessable Entity (attribution not found)
        //
        // This demonstrates Story 3.2 AC#1: ambiguous tenant attribution is refused consistently.
        app.MapGet("/tenants/{tenantId}/data", (string tenantId, ITenantContextAccessor accessor) =>
        {
            var context = accessor.Current;
            return Results.Ok(new
            {
                message = "Tenant attribution successful",
                tenantId = context!.Scope is TenantScope.Tenant t ? t.Id.Value : "unknown",
                traceId = context.TraceId
            });
        })
        .WithName("GetTenantData");

        /// <summary>
        /// Test endpoint for header-only attribution.
        /// </summary>
        /// <remarks>
        /// Requires X-Tenant-Id header. No route parameter to avoid ambiguity testing.
        /// Returns 422 if header is missing or empty.
        /// </remarks>
        app.MapGet("/test/attribution", (ITenantContextAccessor accessor) =>
        {
            var context = accessor.Current;
            return Results.Ok(new
            {
                message = "Attribution test endpoint",
                tenantId = context!.Scope is TenantScope.Tenant t ? t.Id.Value : "unknown",
                traceId = context.TraceId
            });
        })
        .WithName("TestAttribution");

        var summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        var weatherEndpoint = app.MapGet("/weatherforecast", () =>
        {
            var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateTimeOffset.UtcNow.AddDays(index),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
                .ToArray();
            return forecast;
        })
        .WithName("GetWeatherForecast");

        if (enableOpenApi && !app.Environment.IsEnvironment("Test"))
        {
            weatherEndpoint.WithOpenApi();
        }

        return app;
    }
}

record WeatherForecast(DateTimeOffset Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}

public partial class Program
{
}
