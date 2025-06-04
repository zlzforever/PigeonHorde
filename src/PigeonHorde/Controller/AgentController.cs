using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PigeonHorde.Dto.Agent;
using PigeonHorde.Extensions;
using PigeonHorde.Services;

namespace PigeonHorde.Controller;

public class AgentController(WebApplication app)
{
    private readonly ILogger<AgentController> _logger = app.Services.GetService<ILogger<AgentController>>();

    public void Register()
    {
        var jsonOptions = app.Services.GetRequiredService<IOptions<JsonOptions>>();

        // Register Service
        // https://developer.hashicorp.com/consul/api-docs/agent/service#register-service
        app.MapPut("/v1/agent/service/register", async context =>
        {
            var tuple = await context.GetModelAsync<Model.Service>();
            var agentService = new AgentService();
            var service = tuple.Entity;
            service.ContentHash = tuple.ContentHash[..16].ToLowerInvariant();
            agentService.Register(service);

            context.Response.ContentType = "application/json";
            context.Response.StatusCode = 200;

            _logger.LogInformation("register service: {ServiceName} {ServiceId} success", service.Name, service.Id);
        });

        // Deregister Service
        // https://developer.hashicorp.com/consul/api-docs/agent/service#deregister-service
        app.MapPut("/v1/agent/service/deregister/{serviceId}",
            ([FromRoute, StringLength(254)] string serviceId, HttpContext context) =>
            {
                var agentService = new AgentService();
                agentService.Deregister(serviceId);

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = 200;

                _logger.LogInformation("deregister service: {ServiceId}", serviceId);
            });

        // Get Service Configuration
        // https://developer.hashicorp.com/consul/api-docs/agent/service#get-service-configuration
        app.MapGet("/v1/agent/service/{serviceId}",
            async ([FromRoute, StringLength(254)] string serviceId, HttpContext context) =>
            {
                var service = Repository.GetService(serviceId);
                if (service == null)
                {
                    context.Response.StatusCode = 204;
                    context.Response.ContentType = "application/json";
                }
                else
                {
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    var dto = GetServiceConfigurationDto.From(service);
                    await context.Response.WriteAsync(JsonSerializer.Serialize(dto,
                        jsonOptions.Value.JsonSerializerOptions));
                }
            });

        // List Services
        // https://developer.hashicorp.com/consul/api-docs/agent/service#list-services
        app.MapGet("/v1/agent/services", () =>
        {
            var services = Repository.GetAllService();
            var dict = new Dictionary<string, ListServicesItemDto>();
            foreach (var item in services)
            {
                dict.TryAdd(item.Id, ListServicesItemDto.From(item));
            }

            return dict;
        });

        // Retrieve version information
        // https://developer.hashicorp.com/consul/api-docs/agent#retrieve-version-information
        app.MapGet("/v1/agent/version", () => new VersionDto
        {
            SHA = Defaults.Sha,
            BuildDate = Defaults.BuildDate,
            FIPS = "",
            HumanVersion = Defaults.Version
        });
    }
}