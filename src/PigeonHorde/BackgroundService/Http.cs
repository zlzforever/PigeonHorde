using System.Text;
using HWT;
using PigeonHorde.Model;

namespace PigeonHorde.BackgroundService;

public partial class HealthCheckService
{
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
            var data = Check.CreateHealthData(ServiceId, ServiceName, ServiceTags);

            try
            {
                using var response = await httpClient.GetAsync(Check.Http);
                data.Status = response.IsSuccessStatusCode ? "passing" : "critical";
                data.Output =
                    $"HTTP GET {Check.Http}: {(int)response.StatusCode} Content Output: {Encoding.UTF8.GetString(await response.Content.ReadAsByteArrayAsync())}";
            }
            catch (Exception e)
            {
                data.Output =
                    $"HTTP GET {Check.Http}: -1 Content Output: {e.Message}";
                data.Status = "critical";
            }
            finally
            {
                Repository.AddCheck(Check.CheckId, data);

                if (ServiceIdMapTasks.TryGetValue(ServiceId, out var dict))
                {
                    var t = HashedWheelTimer.NewTimeout(Clone(), TimeSpan.FromSeconds(interval));
                    dict[Check.CheckId] = t;

                    if (data.Status != "passing")
                    {
                        _logger.LogWarning(
                            "Id {ServiceId} service {ServiceName} checkId {CheckId} status {Status}",
                            data.ServiceName, data.ServiceId, Check.CheckId, data.Status);
                    }
                    else
                    {
                        _logger.LogDebug(
                            "Id {ServiceId} service {ServiceName} checkId {CheckId} status {Status}",
                            data.ServiceName, data.ServiceId, Check.CheckId, data.Status);
                    }
                }
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