using System.Text;
using HWT;

namespace PigeonHorde.BackgroundService;

public partial class HealthCheckService
{
    private class Http(int interval) : TimerTask
    {
        private readonly ILogger<Http> _logger = _loggerFactory.CreateLogger<Http>();

        public Model.Check Check { get; init; }
        public string ServiceId { get; init; }
        public string ServiceName { get; init; }
        public List<string> ServiceTags { get; init; }
        public int FailedTimes { get; init; }

        public override async Task RunAsync(ITimeout timeout)
        {
            if (timeout.Cancelled)
            {
                return;
            }

            var httpClient = _httpClientFactory.CreateClient();
            var checkResult = Check.CreateHealthData(ServiceId, ServiceName, ServiceTags);

            try
            {
                using var response = await httpClient.GetAsync(Check.Http);
                checkResult.Status = response.IsSuccessStatusCode ? "passing" : "critical";
                checkResult.Output =
                    $"HTTP GET {Check.Http}: {(int)response.StatusCode} Content Output: {Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync())}";
            }
            catch (Exception e)
            {
                checkResult.Output =
                    $"HTTP GET {Check.Http}: -1 Content Output: {e.Message}";
                checkResult.Status = "critical";
            }
            finally
            {
                Repository.AddOrUpdateCheckData(Check.CheckId, checkResult);

                if (ServiceIdMapTasks.TryGetValue(ServiceId, out var dict))
                {
                    if (checkResult.Status != "passing")
                    {
                        var failedTimes = FailedTimes + 1;
                        // 如果连续失败超过 30 次，则不再继续检查
                        if (FailedTimes < 30)
                        {
                            var hashedWheelTimeout = HashedWheelTimer.NewTimeout(
                                Clone(failedTimes),
                                TimeSpan.FromSeconds(interval));
                            dict[Check.CheckId] = hashedWheelTimeout;
                        }

                        _logger.LogWarning(
                            "Id {ServiceId} service {ServiceName} checkId {CheckId} status {Status}, times: {FailedTimes}",
                            checkResult.ServiceName, checkResult.ServiceId, Check.CheckId, checkResult.Status,
                            FailedTimes);
                    }
                    else
                    {
                        var hashedWheelTimeout = HashedWheelTimer.NewTimeout(
                            Clone(0),
                            TimeSpan.FromSeconds(interval));
                        dict[Check.CheckId] = hashedWheelTimeout;
                        _logger.LogDebug(
                            "Id {ServiceId} service {ServiceName} checkId {CheckId} status {Status}",
                            checkResult.ServiceName, checkResult.ServiceId, Check.CheckId, checkResult.Status);
                    }
                }
            }
        }

        private Http Clone(int failedTimes)
        {
            return new Http(interval)
            {
                FailedTimes = failedTimes,
                Check = Check,
                ServiceId = ServiceId,
                ServiceName = ServiceName,
                ServiceTags = ServiceTags
            };
        }
    }
}