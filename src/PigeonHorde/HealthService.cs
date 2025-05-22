using PigeonHorde.Dto;

namespace PigeonHorde;

public class HealthService(HttpContext httpContext)
{
    private readonly ILogger<AgentService> _logger =
        httpContext.RequestServices.GetRequiredService<ILogger<AgentService>>();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceName"></param>
    public List<HealthListServiceInstancesDto> Get(string serviceName)
    {
        var services = Repositry.GetList(serviceName);
        var result = new List<HealthListServiceInstancesDto>();
        var checkIdList = services.SelectMany(x => x.GetAllCheck().Select(y => y.CheckId)).ToArray();
        var healthDataList = Repositry.GetCheckList(checkIdList);
        var healthDataDict = healthDataList.ToDictionary(x => x.CheckId, x => x);

        foreach (var item in services)
        {
            var serviceDto = HealthListServiceInstancesDto.ServiceDto.From(item);

            var healths = new List<HealthListServiceInstancesDto.CheckDto>();
            foreach (var check in item.GetAllCheck())
            {
                if (healthDataDict.TryGetValue(check.CheckId, out var value))
                {
                    healths.Add(HealthListServiceInstancesDto.CheckDto.From(value));
                }
            }

            result.Add(new HealthListServiceInstancesDto
            {
                Service = serviceDto,
                Checks = healths
            });
        }

        _logger.LogDebug("Get service health info: {ServiceName}", serviceName);
        return result;
    }
}