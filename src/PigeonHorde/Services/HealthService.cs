using PigeonHorde.Dto.Health;

namespace PigeonHorde.Services;

public class HealthService(HttpContext httpContext)
{
    private readonly ILogger<HealthService> _logger =
        httpContext.RequestServices.GetRequiredService<ILogger<HealthService>>();


    public List<ListServiceInstancesDto.CheckDto> GetChecks(string serviceName)
    {
        var services = Repository.GetServices(serviceName);

        var checkIdList = services
            .SelectMany(x => x.GetAllCheck().Select(y => y.CheckId))
            .ToArray();
        var healthDataDict = Repository.GetChecks(checkIdList).ToDictionary(x => x.CheckId, x => x);

        var healths = new List<ListServiceInstancesDto.CheckDto>();
        foreach (var service in services)
        {
            foreach (var check in service.GetAllCheck())
            {
                if (!healthDataDict.TryGetValue(check.CheckId, out var healthData))
                {
                    continue;
                }

                healths.Add(ListServiceInstancesDto.CheckDto.From(healthData));
            }
        }

        return healths;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceName"></param>
    /// <param name="passing"></param>
    public List<ListServiceInstancesDto> Get(string serviceName, bool passing)
    {
        var services = Repository.GetServices(serviceName);

        var checkIdList = services
            .SelectMany(x => x.GetAllCheck().Select(y => y.CheckId))
            .ToArray();
        var healthDataDict = Repository.GetChecks(checkIdList).ToDictionary(x => x.CheckId, x => x);

        var result = new List<ListServiceInstancesDto>();
        foreach (var service in services)
        {
            var serviceDto = ListServiceInstancesDto.ServiceDto.From(service);

            var healths = new List<ListServiceInstancesDto.CheckDto>();
            var success = true;
            foreach (var check in service.GetAllCheck())
            {
                if (!healthDataDict.TryGetValue(check.CheckId, out var healthData))
                {
                    continue;
                }

                if (passing && !"passing".Equals(healthData.Status, StringComparison.InvariantCulture))
                {
                    success = false;
                    break;
                }

                healths.Add(ListServiceInstancesDto.CheckDto.From(healthData));
            }

            if (success)
            {
                result.Add(new ListServiceInstancesDto
                {
                    Service = serviceDto,
                    Checks = healths
                });
            }
        }

        _logger.LogDebug("query service health: {ServiceName}", serviceName);
        return result;
    }
}