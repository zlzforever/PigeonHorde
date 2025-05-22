using System.Text.Json;
using PigeonHorde.Model;

namespace PigeonHorde;

/// <summary>
/// KEY: service_id -> JSON DATA
/// KEY: service_name -> service_id
/// </summary>
internal static class Repositry
{
    private const string Prefix = "PIGEON_HORDE_";
    private const string ServiceKey = "PIGEON_HORDE_SERVICE";
    internal const string ServiceHealthCheckKey = "PIGEON_HORDE_SERVICE_HEALTH_CHECK";
    internal const string ServiceRegisterEventKey = "PIGEON_HORDE_SERVICE_REGISTER_EVENT";

    public static Task Add(Service service)
    {
        var serviceName = service.Name;
        var serviceId = service.Id;
        var data = JsonSerializer.Serialize(service);
        var nameKey = ToServiceName(serviceName);

        using var pipe = Connector.Redis.StartPipe();

        pipe.HSet(ServiceKey, serviceId, serviceName);
        pipe.HSet(nameKey, serviceId, data);

        foreach (var check in service.GetAllCheck())
        {
            var healthData = check.CreateHealthData(serviceId, serviceName, service.Tags);
            pipe.HSet(ServiceHealthCheckKey, check.CheckId, JsonSerializer.Serialize(healthData));
        }

        pipe.RPush(ServiceRegisterEventKey,
            JsonSerializer.Serialize(new ServiceChangedEvent
            {
                Id = service.Id,
                Name = serviceName,
                OperateType = OperateType.Register
            }));
        pipe.EndPipe();
        return Task.CompletedTask;
    }

    public static Task RemoveById(string serviceId)
    {
        var serviceName = Connector.Redis.HGet(ServiceKey, serviceId);
        if (string.IsNullOrEmpty(serviceName))
        {
            return Task.CompletedTask;
        }

        var nameKey = ToServiceName(serviceName);
        var json = Connector.Redis.HGet(nameKey, serviceId);
        if (string.IsNullOrEmpty(json))
        {
            return Task.CompletedTask;
        }

        var service = JsonSerializer.Deserialize<Service>(json);

        using var pipe = Connector.Redis.StartPipe();
        pipe.HDel(ServiceKey, serviceId);
        pipe.HDel(nameKey, serviceId);

        foreach (var check in service.GetAllCheck())
        {
            pipe.HDel(ServiceHealthCheckKey, check.CheckId);
        }

        pipe.RPush(ServiceRegisterEventKey,
            JsonSerializer.Serialize(new ServiceChangedEvent
            {
                Id = serviceId,
                Name = service.Name,
                OperateType = OperateType.Remove
            }));
        pipe.EndPipe();
        return Task.CompletedTask;
    }

    public static Task RemoveByName(string serviceName)
    {
        var nameKey = ToServiceName(serviceName);
        var dict = Connector.Redis.HGetAll(nameKey);

        using var pipe = Connector.Redis.StartPipe();
        foreach (var json in dict.Values)
        {
            var service = JsonSerializer.Deserialize<Service>(json);
            pipe.HDel(ServiceKey, service.Id);
            // 删除存储的数据
            pipe.HDel(nameKey);
            foreach (var check in service.GetAllCheck())
            {
                pipe.HDel(ServiceHealthCheckKey, check.CheckId);
            }

            pipe.RPush(ServiceRegisterEventKey,
                JsonSerializer.Serialize(new ServiceChangedEvent
                {
                    Id = service.Id,
                    Name = service.Name,
                    OperateType = OperateType.Remove
                }));
        }

        pipe.EndPipe();
        return Task.CompletedTask;
    }

    public static List<Service> GetList(string serviceName)
    {
        var dict = Connector.Redis.HGetAll(ToServiceName(serviceName));
        if (dict == null || dict.Count == 0)
        {
            return [];
        }

        return dict.Select(x => JsonSerializer.Deserialize<Service>(x.Value)).ToList();
    }

    public static async Task<Service> GetService(string serviceId)
    {
        // 获取服务名称
        var serviceName = await Connector.Redis.HGetAsync(ServiceKey, serviceId);
        return await GetService(serviceName, serviceId);
    }

    public static async Task<Service> GetService(string serviceName, string serviceId)
    {
        var nameKey = ToServiceName(serviceName);
        // 获取服务信息
        var json = await Connector.Redis.HGetAsync(nameKey, serviceId);
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

    public static IEnumerable<Service> GetAll()
    {
        var dict = GetServiceIdAndName();
        var handled = new HashSet<string>();
        foreach (var service in dict.Values)
        {
            if (!handled.Contains(service))
            {
                var list = GetList(service);
                foreach (var item in list)
                {
                    yield return item;
                }

                handled.Add(service);
            }
        }
    }

    public static IEnumerable<HealthData> GetCheckList(string[] checkIdList)
    {
        var items = Connector.Redis.HMGet(ServiceHealthCheckKey, checkIdList);
        var result = items.Select(x => JsonSerializer.Deserialize<HealthData>(x));
        return result;
    }

    private static string ToServiceName(string name)
    {
        return Prefix + name;
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
}