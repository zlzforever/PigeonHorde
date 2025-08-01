namespace PigeonHorde.Controller;

public class StatusController(WebApplication app)
{
    public void Register()
    {
        var logger = app.Services.GetRequiredService<ILogger<Program>>();
        app.MapGet("/v1/stats", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                $"{Defaults.DataCenter} timestamp {DateTimeOffset.Now.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
        });
        app.MapGet("/healthx", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync(
                $"{Defaults.DataCenter} timestamp {DateTimeOffset.Now.ToLocalTime():yyyy-MM-dd HH:mm:ss}");
            logger.LogDebug("Health check passed at {Time}",
                DateTimeOffset.Now.ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss"));
        });
    }
}