using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using HWT;
using PigeonHorde.Model;

namespace PigeonHorde;

public class HealthCheckBackgroundService(
    IHttpClientFactory httpClientFactory,
    ILoggerFactory loggerFactory)
    : BackgroundService
{
    private static readonly HashedWheelTimer HashedWheelTimer = new();
    private static IHttpClientFactory _httpClientFactory;

    private static readonly Dictionary<string, Dictionary<string, ITimeout>> ServiceIdMapTasks =
        new();

    private static ILoggerFactory _loggerFactory;
    private ILogger<HealthCheckBackgroundService> _logger;

    [SuppressMessage("ReSharper", "MethodHasAsyncOverload")]
    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _loggerFactory = loggerFactory;
        _httpClientFactory ??= httpClientFactory;
        _logger = _loggerFactory.CreateLogger<HealthCheckBackgroundService>();

        // 1s = 1000ms
        // 1ms 加载一个 check
        // 10s 加载 10000 个check
        return Task.Factory.StartNew([SuppressMessage("ReSharper", "MethodSupportsCancellation")] async () =>
        {
            await Task.Delay(500);

            long i = 0;
            // 一个一个消费， 创建定时任务
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var json = Connector.Redis.LPop(Repositry.ServiceRegisterEventKey);
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
                    _logger.LogError(ex, "Health check service error");
                    i++;
                    await Task.Delay(1000);
                }

                if (i % 300 == 0)
                {
                    _logger.LogInformation("Health check service is running， task count {PendingTimeouts}",
                        HashedWheelTimer.PendingTimeouts);
                }
            }
        }, stoppingToken);
    }

    public override Task StopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Health check service is stopped");
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
                var service = await Repositry.GetService(@event.Name, @event.Id);
                if (service == null)
                {
                    break;
                }

                foreach (var check in service.GetAllCheck())
                {
                    if (!"http".Equals(check.GetCheckType(),
                            StringComparison.InvariantCultureIgnoreCase))
                    {
                        await Task.Delay(1);
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
                    var t = HashedWheelTimer.NewTimeout(new Http(interval)
                    {
                        Check = check,
                        ServiceId = service.Id,
                        ServiceName = service.Name,
                        ServiceTags = service.Tags
                    }, TimeSpan.FromMilliseconds(interval * 1000));
                    dict.TryAdd(check.CheckId, t);

                    // QPS 500
                    await Task.Delay(2);
                }

                break;
            }
            case OperateType.Remove:
            default:
                await Task.Delay(1);
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

    private class Http(int interval) : TimerTask
    {
        private readonly ILogger<Http> _logger = _loggerFactory.CreateLogger<Http>();

        public Check Check { get; set; }
        public string ServiceId { get; set; }
        public string ServiceName { get; set; }
        public List<string> ServiceTags { get; set; }

        public override async Task RunAsync(ITimeout timeout)
        {
            if (timeout.Cancelled)
            {
                return;
            }

            var httpClient = _httpClientFactory.CreateClient();
            using var response = await httpClient.GetAsync(Check.Http);
            var data = Check.CreateHealthData(ServiceId, ServiceName, ServiceTags);
            data.Status = response.IsSuccessStatusCode ? "passing" : "critical";
            data.Output =
                $"HTTP GET {Check.Http}: {(int)response.StatusCode} Content Output: {Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync())}";

            // ReSharper disable once MethodHasAsyncOverload
            Connector.Redis.HSet(Repositry.ServiceHealthCheckKey, Check.CheckId,
                JsonSerializer.Serialize(data));

            if (ServiceIdMapTasks.TryGetValue(ServiceId, out var dict))
            {
                var t = HashedWheelTimer.NewTimeout(Clone(), TimeSpan.FromSeconds(interval));
                dict[Check.CheckId] = t;

                _logger.LogDebug(
                    "[{ID}] ServiceName {ServiceName} serviceId {ServiceId} checkId {CheckId} status {Status}",
                    GetHashCode(),
                    data.ServiceName, data.ServiceId, Check.CheckId, data.Status);
            }
        }

        private Http Clone()
        {
            return new Http(interval)
            {
                Check = Check,
                ServiceId = ServiceId,
                ServiceName = ServiceName,
                ServiceTags = ServiceTags
            };
        }
    }
}