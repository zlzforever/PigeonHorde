using System.Text.Json;
using PigeonHorde.Model;

namespace PigeonHorde;

public class AgentService(HttpContext httpContext)
{
    private readonly ILogger<AgentService> _logger =
        httpContext.RequestServices.GetRequiredService<ILogger<AgentService>>();

    public async Task RegisterAsync()
    {
        using var reader = new StreamReader(httpContext.Request.Body);
        var body = await reader.ReadToEndAsync();
        var service = JsonSerializer.Deserialize<Service>(body);
        service.Initialize();
        await Repositry.Add(service);

        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = 200;
        _logger.LogInformation("register service: {ServiceName} {ServiceId}", service.Name, service.Id);
    }

    public async Task DeregisterAsync(string serviceId)
    {
        await Repositry.RemoveById(serviceId);
        httpContext.Response.ContentType = "application/json";
        httpContext.Response.StatusCode = 200;
        _logger.LogInformation("deregister service: {ServiceId}", serviceId);
    }
}