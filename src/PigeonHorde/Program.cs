using System.IO.Compression;
using FreeRedis;
using Microsoft.AspNetCore.ResponseCompression;
using PigeonHorde.BackgroundService;
using PigeonHorde.Controller;
using PigeonHorde.Logging;
using PigeonHorde.Model;
using PigeonHorde.Services;

namespace PigeonHorde;

public class Program
{
    public static async Task Main(string[] args)
    {
        var port = Environment.GetEnvironmentVariable("PIGEON_HORDE_PORT");
        port = string.IsNullOrWhiteSpace(port) ? "9500" : port;
        if (!int.TryParse(port, out var portValue))
        {
            Console.WriteLine($"PORT {port} is not valid, using default port 9500 instead.");
            portValue = 9500;
        }

        Console.WriteLine($"""

                             _                          
                            /_/._  _  _  _  /_/_  _ _/_ 
                           /  //_//_'/_// // //_///_//_' PigeonHorde 0.0.9 64 bit;
                               _/                        Listening on: 0.0.0.0:{portValue}
                                                         https://github.com/zlzforever/PigeonHorde
                           """);
        var i = 0;
        while (i < 30)
        {
            try
            {
                var info = Connector.Redis.Info();
                Console.WriteLine(info);
                break;
            }
            catch
            {
                i++;
                await Task.Delay(1000);
            }
        }

        var builder = WebApplication.CreateBuilder(args);
        builder.WebHost.UseUrls($"http://0.0.0.0:{portValue}");
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        }).AddProvider(
            new FileLoggerProvider(new FileLoggerOutput(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"),
                10)));
        builder.Configuration.AddEnvironmentVariables("PIGEON_HORDE_");
        builder.Services.AddHostedService<HealthCheckService>();
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
        var connectionString = Environment.GetEnvironmentVariable("PIGEON_HORDE_REDIS_URL");
        if (string.IsNullOrEmpty(connectionString))
        {
            var connectionStringBuilder = app.Configuration.GetSection("Redis").Get<ConnectionStringBuilder>();
            Connector.Load(connectionStringBuilder);
        }
        else
        {
            Connector.Load(ConnectionStringBuilder.Parse(connectionString));
        }

        app.UseResponseCaching();
        app.UseResponseCompression();

        new AgentController(app).Register();
        new HealthController(app).Register();
        new TestController(app).Register();

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

        Repository.LoadEvents();

        var agentService = new AgentService();
        agentService.Register(new Service
        {
            Id = "PigeonHorde",
            Name = "PigeonHordeService",
            Tags = ["PigeonHorde", "Infra"],
            Address = "127.0.0.1",
            Port = 8500,
            Meta = new Dictionary<string, string> { { "PORT", "8500" } },
            Check = new Check
            {
                CheckId = "PigeonHordeHealth:127.0.0.1",
                Name = "PigeonHorde Health Status",
                Interval = "10s",
                Http = "http://127.0.0.1:8500/healthx"
            }
        });

        await app.RunAsync();
        Console.WriteLine("Bye!");

#if !DEBUG
        Environment.Exit(0);
#endif
    }
}