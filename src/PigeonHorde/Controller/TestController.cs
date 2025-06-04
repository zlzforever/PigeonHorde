using System.Text.Json;
using PigeonHorde.Model;
using PigeonHorde.Services;

namespace PigeonHorde.Controller;

public class TestController(WebApplication app)
{
    public void Register()
    {
        if (!"true".Equals(Environment.GetEnvironmentVariable("PIGEON_HORDE_TEST_MODE"),
                StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        app.MapPost("v1/test/load", async context =>
        {
            var agentService = new AgentService();

            for (var i = 0; i < 500; i++)
            {
                var body = $$"""
                             {
                                 "ID": "test-api_{{i}}",
                                 "Name": "test-api-{{i}}",
                                 "Tags": [
                                     "dapr"
                                 ],
                                 "Port": {{i}},
                                 "Address": "255.255.255.{{i}}",
                                 "Meta": {
                                     "DAPR_METRICS_PORT": "1{{i}}",
                                     "DAPR_PORT": "2{{i}}",
                                     "DAPR_PROFILE_PORT": "-1"
                                 },
                                 "Check": {
                                     "CheckID": "daprHealth:test-api:{{i}}",
                                     "Name": "Dapr Health Status",
                                     "Interval": "5s",
                                     "HTTP": "http://127.0.0.1:8500/v1/health/service/test-api"
                                 },
                                 "Checks": null
                             }
                             """;
                var service = JsonSerializer.Deserialize<Service>(body);
                agentService.Register(service);
            }

            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("done");
        });
    }
}