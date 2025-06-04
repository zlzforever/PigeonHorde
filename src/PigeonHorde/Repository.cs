using System.Text.Json;
using FreeRedis;
using PigeonHorde.Model;

namespace PigeonHorde;

/// <summary>
/// KEY: service_id -> JSON DATA
/// KEY: service_name -> service_id
/// </summary>
internal static class Repository
{
    private const string Prefix = "PIGEON_HORDE_";
    private const string ServiceKey = "PIGEON_HORDE_SERVICE_ID_MAP_NAME";
    private const string ServiceHealthCheckKey = "PIGEON_HORDE_SERVICE_HEALTH_CHECK";
    private const string ServiceRegisterEventKey = "PIGEON_HORDE_SERVICE_REGISTER_EVENT";

    public static void RemoveByName(string serviceName)
    {
        var nameKey = ToServiceName(serviceName);
        var dict = Connector.Redis.HGetAll(nameKey);

        using var pipe = Connector.Redis.StartPipe();
        foreach (var json in dict.Values)
        {
            var service = JsonSerializer.Deserialize<Service>(json);
            RemoveService(pipe, service);
            RemoveChecks(pipe, service.GetAllCheck());

            PublishServiceRegisterEvent(pipe, new ServiceChangedEvent
            {
                Id = service.Id,
                Name = service.Name,
                OperateType = OperateType.Remove
            });
        }

        pipe.EndPipe();
    }

    public static void AddService(RedisClient.PipelineHook pipe, Service service)
    {
        var serviceName = service.Name;
        var serviceId = service.Id;
        var nameKey = ToServiceName(serviceName);
        var data = JsonSerializer.Serialize(service);
        pipe.HSet(ServiceKey, serviceId, serviceName);
        pipe.HSet(nameKey, serviceId, data);
        foreach (var check in service.GetAllCheck())
        {
            var healthData = check.CreateHealthData(service.Id, service.Name, service.Tags);
            AddCheck(pipe, check.CheckId, healthData);
        }
    }

    public static void RemoveService(RedisClient.PipelineHook pipe, Service service)
    {
        if (service == null)
        {
            return;
        }

        var serviceName = service.Name;
        var serviceId = service.Id;
        var nameKey = ToServiceName(serviceName);
        pipe.HDel(ServiceKey, serviceId);
        pipe.HDel(nameKey, serviceId);
        RemoveChecks(pipe, service.GetAllCheck());
    }

    private static void AddCheck(RedisClient.PipelineHook pipe, string checkId,
        HealthData check)
    {
        pipe.HSet(ServiceHealthCheckKey, checkId, JsonSerializer.Serialize(check));
    }

    public static void AddCheck(string checkId, HealthData check)
    {
        Connector.Redis.HSet(ServiceHealthCheckKey, checkId, JsonSerializer.Serialize(check));
    }

    private static void RemoveChecks(RedisClient.PipelineHook pipe, IEnumerable<Check> checks)
    {
        foreach (var check in checks)
        {
            pipe.HDel(ServiceHealthCheckKey, check.CheckId);
        }
    }

    public static List<Service> GetServices(string serviceName)
    {
        var dict = Connector.Redis.HGetAll(ToServiceName(serviceName));
        if (dict == null || dict.Count == 0)
        {
            return [];
        }

        return dict.Select(x => JsonSerializer.Deserialize<Service>(x.Value)).ToList();
    }

    public static Service GetService(string serviceId)
    {
        // 获取服务名称
        var serviceName = Connector.Redis.HGet(ServiceKey, serviceId);
        return string.IsNullOrEmpty(serviceName) ? null : GetService(serviceName, serviceId);
    }

    public static Service GetService(string serviceName, string serviceId)
    {
        var nameKey = ToServiceName(serviceName);
        var json = Connector.Redis.HGet(nameKey, serviceId);
        var service = string.IsNullOrEmpty(json) ? null : JsonSerializer.Deserialize<Service>(json);
        return service;
    }

    /// <summary>
    /// ServiceID -> ServiceName
    /// </summary>
    /// <returns></returns>
    public static Dictionary<string, string> GetServiceIdAndName()
    {
        return Connector.Redis.HGetAll(ServiceKey);
    }

    // public static string GetServiceName(string serviceId)
    // {
    //     return await Connector.Redis.HGet(ServiceKey, serviceId);
    // }

    public static IEnumerable<Service> GetAllService()
    {
        var dict = GetServiceIdAndName();
        var handled = new HashSet<string>();
        foreach (var service in dict.Values)
        {
            if (handled.Contains(service))
            {
                continue;
            }

            var list = GetServices(service);
            foreach (var item in list)
            {
                yield return item;
            }

            handled.Add(service);
        }
    }

    public static IEnumerable<HealthData> GetChecks(string[] checkIdList)
    {
        if (checkIdList.Length == 0)
        {
            return [];
        }

        var items = Connector.Redis.HMGet(ServiceHealthCheckKey, checkIdList);
        var result = items.Select(x => JsonSerializer.Deserialize<HealthData>(x));
        return result;
    }

    public static void LoadEvents()
    {
        // 清空所有历史未被消费的数据
        Connector.Redis.Del(ServiceRegisterEventKey);

        // 所有的服务全部发布一次
        var serviceList = GetServiceIdAndName();

        var pipeline = Connector.Redis.StartPipe();
        foreach (var kv in serviceList)
        {
            pipeline.RPush(ServiceRegisterEventKey, JsonSerializer.Serialize(new ServiceChangedEvent
            {
                Id = kv.Key,
                Name = kv.Value,
                OperateType = OperateType.Register
            }));
        }

        pipeline.EndPipe();
    }

    public static void PublishServiceRegisterEvent(RedisClient.PipelineHook pipe,
        ServiceChangedEvent @event)
    {
        pipe.RPush(ServiceRegisterEventKey, JsonSerializer.Serialize(@event));
    }

    public static string PopServiceRegisterEvent()
    {
        return Connector.Redis.LPop(ServiceRegisterEventKey);
    }

    private static string ToServiceName(string name)
    {
        return Prefix + "CONFIG_" + name;
    }
}