using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using HWT;
using PigeonHorde.Model;

namespace PigeonHorde.BackgroundService;

public partial class HealthCheckService(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory)
    : Microsoft.Extensions.Hosting.BackgroundService
{
    private static readonly HashedWheelTimer HashedWheelTimer = new();
    private static IHttpClientFactory _httpClientFactory;

    private static readonly Dictionary<string, Dictionary<string, ITimeout>> ServiceIdMapTasks =
        new();

    private static ILoggerFactory _loggerFactory;
    private ILogger<HealthCheckService> _logger;

    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _loggerFactory = loggerFactory;
        _httpClientFactory ??= httpClientFactory;
        _logger = _loggerFactory.CreateLogger<HealthCheckService>();

        // 1s = 1000ms
        // 5ms 加载一个 check
        // 则 1s 加载 1000/5=200 个 check
        // 加载 10000 个 check 需要 10000/200 = 50 秒
        // 如果有 10000 个 check 每个 check 的间隔是 5 秒, 则 QPS 为 2000 已经很高了， 需要考虑多节点进行健康检查
        return Task.Factory.StartNew([SuppressMessage("ReSharper", "MethodSupportsCancellation")] async () =>
        {
            await Task.Delay(500);

            long i = 0;
            // 一个一个消费， 创建定时任务
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var json = Repository.PopServiceRegisterEvent();

                    if (string.IsNullOrEmpty(json))
                    {
                        i++;
                        await Task.Delay(1000);
                    }
                    else
                    {
                        await HandleEventAsync(json);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "health check service error");
                    i++;
                    await Task.Delay(1000);
                }

                if (i != 0 && i % 300 == 0)
                {
                    _logger.LogInformation("health check service is running， task count is {PendingTimeouts}",
                        HashedWheelTimer.PendingTimeouts);
                }
            }
        }, stoppingToken);
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("health check service is stopped");
        return Task.CompletedTask;
    }

    private async Task HandleEventAsync(string json)
    {
        var @event = JsonSerializer.Deserialize<ServiceChangedEvent>(json);
        // 不管注册还是删除都要取消之前的定时任务， 
        CancelTimeout(@event.Id);

        switch (@event.OperateType)
        {
            case OperateType.Register:
            {
                var service = Repository.GetService(@event.Name, @event.Id);
                if (service == null)
                {
                    break;
                }

                foreach (var check in service.GetAllCheck())
                {
                    if (!"http".Equals(check.GetCheckType(),
                            StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Delay(5);
                        break;
                    }

                    if (!ServiceIdMapTasks.ContainsKey(service.Id))
                    {
                        ServiceIdMapTasks[service.Id] = new Dictionary<string, ITimeout>();
                    }

                    Dictionary<string, ITimeout> dict;
                    if (ServiceIdMapTasks.TryGetValue(service.Id, out var value))
                    {
                        dict = value;
                    }
                    else
                    {
                        dict = new Dictionary<string, ITimeout>();
                        ServiceIdMapTasks[service.Id] = dict;
                    }

                    var interval = HealthData.GetInterval(check);
                    var delay = TimeSpan.FromSeconds(interval);
                    var t = HashedWheelTimer.NewTimeout(new Http(interval)
                    {
                        Check = check,
                        ServiceId = service.Id,
                        ServiceName = service.Name,
                        ServiceTags = service.Tags
                    }, delay);
                    dict[check.CheckId] = t;

                    // QPS 500
                    await Task.Delay(5);
                }

                break;
            }
            case OperateType.Remove:
            default:
                await Task.Delay(5);
                break;
        }
    }

    private void CancelTimeout(string serviceId)
    {
        if (!ServiceIdMapTasks.Remove(serviceId, out var tasks))
        {
            return;
        }

        foreach (var kv in tasks)
        {
            if (!kv.Value.Cancelled)
            {
                kv.Value.Cancel();
            }
        }
    }
}