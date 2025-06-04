using System.IO.Compression;
using FreeRedis;
using Microsoft.AspNetCore.ResponseCompression;
using PigeonHorde.Controller;
using PigeonHorde.Logging;
using PigeonHorde.Model;
using PigeonHorde.Services;

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
        builder.WebHost.UseUrls("http://0.0.0.0:8500");
        builder.Logging.AddSimpleConsole(options =>
        {
            options.SingleLine = true;
            options.TimestampFormat = "yyyy-MM-dd HH:mm:ss ";
        }).AddProvider(
            new FileLoggerProvider(new FileLoggerOutput(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "log.txt"),
                10)));
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

        app.MapGet("/v1/stats", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync($"{Defaults.DataCenter} timestamp {DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds()}");
        });
        app.MapGet("/healthx", async context =>
        {
            context.Response.ContentType = "text/plain";
            context.Response.StatusCode = 200;
            await context.Response.WriteAsync($"{Defaults.DataCenter} timestamp {DateTimeOffset.Now.ToLocalTime().ToUnixTimeSeconds()}");
        });

        Repository.LoadEvents();

        var agentService = new AgentService();
        agentService.Register(new Service
        {
            Id = "PigeonHorde",
            Name = "PigeonHorde Service",
            Tags = ["PigeonHorde", "Infra"],
            Address = "127.0.0.1",
            Port = 8500,
            Meta = new Dictionary<string, string> { { "PORT", "8500" } }
        });

        app.Run();
        Console.WriteLine("Bye!");

#if !DEBUG
        Environment.Exit(0);
#endif
    }
}