using System.ComponentModel.DataAnnotations;
using System.IO.Compression;
using System.Text.Json;
using FreeRedis;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Options;
using PigeonHorde.Dto;
using PigeonHorde.Logging;
using PigeonHorde.Model;

namespace PigeonHorde;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("""

                            _                          
                           /_/._  _  _  _  /_/_  _ _/_ 
                          /  //_//_'/_// // //_///_//_' PigeonHorde 0.0.9 64 bit;
                              _/                        Listening on: 0.0.0.0:8500
                                                        https://github.com/zlzforever/PigeonHorde
                          """);
        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls("http://+:8500");
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        }).AddProvider(new FileLoggerProvider(new FileLoggerOutput("log.txt", 10)));

        builder.Configuration.AddEnvironmentVariables("PIGEON_HORDE_");
        builder.Services.AddHostedService<HealthCheckBackgroundService>();
        builder.Services.AddHttpClient();
        builder.Services.AddMemoryCache();
        builder.Services.AddResponseCompression(options =>
        {
            options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(
                ["application/json"]);
            // 添加Brotli和Gzip提供器
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });
        builder.Services.Configure<GzipCompressionProviderOptions>(options =>
        {
            options.Level = CompressionLevel.Optimal;
        });

        var app = builder.Build();

        var jsonOptions = app.Services.GetRequiredService<IOptions<JsonOptions>>();

        app.UseResponseCaching();
        app.UseResponseCompression();

        var redisUrl = Environment.GetEnvironmentVariable("PIGEON_HORDE_REDIS_URL");
        if (string.IsNullOrEmpty(redisUrl))
        {
            var connectionStringBuilder = app.Configuration.GetSection("Redis").Get<ConnectionStringBuilder>();
            Connector.Load(connectionStringBuilder);
        }
        else
        {
            Connector.Load(ConnectionStringBuilder.Parse(redisUrl));
        }

        app.MapGet("/v1/stats", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync("ok");
        });
        app.MapGet("/healthx", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync($"Dc1 timestamp {DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds()}");
        });
        app.MapPut("/v1/agent/service/register", async context =>
        {
            var agentService = new AgentService(context);
            await agentService.RegisterAsync();
        });

        app.MapPut("/v1/agent/service/deregister/{serviceId}",
            async ([FromRoute, StringLength(254)] string serviceId, HttpContext context) =>
            {
                var agentService = new AgentService(context);
                await agentService.DeregisterAsync(serviceId);
            });

        app.MapGet("/v1/agent/services", () =>
        {
            var services = Repositry.GetAll();
            var dict = new Dictionary<string, AgentListServicesItemDto>();
            foreach (var item in services)
            {
                dict.TryAdd(item.Id, AgentListServicesItemDto.From(item));
            }

            return dict;
        });

        app.MapGet("/v1/agent/service/{serviceId}",
            async ([FromRoute, StringLength(254)] string serviceId, HttpContext context) =>
            {
                var service = await Repositry.GetService(serviceId);
                if (service == null)
                {
                    context.Response.StatusCode = 204;
                    context.Response.ContentType = "application/json";
                }
                else
                {
                    var dto = AgentListServicesItemDto.From(service);
                    context.Response.StatusCode = 200;
                    context.Response.ContentType = "application/json";
                    await context.Response.WriteAsync(JsonSerializer.Serialize(dto,
                        jsonOptions.Value.JsonSerializerOptions));
                }
            });

        app.MapGet("v1/health/service/{serviceName}",
            ([FromRoute, StringLength(254)] string serviceName, HttpContext context) =>
            {
                // return memoryCache.GetOrCreate($"V1_HEALTH_SERVICE_{serviceName}", entry =>
                // {
                //     var healthService = new HealthService(context);
                //     var services = healthService.Get(serviceName);
                //
                //     entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                //     entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                //     entry.Priority = CacheItemPriority.Normal;
                //     entry.SetValue(services);
                //
                //     return services;
                // });

                var healthService = new HealthService(context);
                var services = healthService.Get(serviceName);

                // entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                // entry.SlidingExpiration = TimeSpan.FromSeconds(5);
                // entry.Priority = CacheItemPriority.Normal;
                // entry.SetValue(services);

                return services;
            });

        app.MapPost("v1/data/load", async () =>
        {
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
                                     "DAPR_METRICS_PORT": "51780",
                                     "DAPR_PORT": "51781",
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
                service.Initialize();
                await Repositry.Add(service);
            }
        });

        Repositry.LoadEvents();

        app.Run();
        Console.WriteLine("Bye!");
        Environment.Exit(0);
    }
}