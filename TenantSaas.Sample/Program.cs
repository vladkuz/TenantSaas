var app = SampleApp.BuildApp(args);
app.Run();

public static class SampleApp
{
    public static WebApplication BuildApp(string[] args, bool enableOpenApi = true)
    {
        var builder = WebApplication.CreateBuilder(args);

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

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

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

        if (enableOpenApi)
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
