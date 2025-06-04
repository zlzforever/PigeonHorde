using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using PigeonHorde.Services;

namespace PigeonHorde.Controller;

public class HealthController(WebApplication app)
{
    public void Register()
    {
        // List Service Instances for Service
        // https://developer.hashicorp.com/consul/api-docs/health#list-service-instances-for-service
        app.MapGet("v1/health/service/{serviceName}",
            ([FromRoute, StringLength(254)] string serviceName, string passing, HttpContext context) =>
            {
                var healthService = new HealthService(context);
                var services = healthService.Get(serviceName,
                    "true".Equals(passing, StringComparison.OrdinalIgnoreCase));
                return services;
            });

        // List Checks for Service
        // https://developer.hashicorp.com/consul/api-docs/health#list-checks-for-service
        app.MapGet("v1/health/checks/{serviceName}",
            ([FromRoute, StringLength(254)] string serviceName,
                HttpContext context) =>
            {
                var healthService = new HealthService(context);
                var services = healthService.GetChecks(serviceName);
                return services;
            });
    }
}