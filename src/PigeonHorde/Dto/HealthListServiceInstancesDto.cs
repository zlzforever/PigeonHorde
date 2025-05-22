using System.Text.Json.Serialization;
using PigeonHorde.Model;

namespace PigeonHorde.Dto;

public class HealthListServiceInstancesDto
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Service")]
    public ServiceDto Service { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Checks")]
    public List<CheckDto> Checks { get; set; }

    public class ServiceDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ID")]
        public string Id { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Service")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Tags")]
        public List<string> Tags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Meta")]
        public Dictionary<string, string> Meta { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Port")]
        public int Port { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Address")]
        public string Address { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("TaggedAddresses")]
        public Dictionary<string, AddressInfo> TaggedAddresses { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Weights")]
        public Weights Weights { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("EnableTagOverride")]
        public bool EnableTagOverride { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Proxy")]
        public object Proxy { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Connect")]
        public object Connect { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("PeerName")]
        public string PeerName { get; set; }

        public static ServiceDto From(Service service)
        {
            return new ServiceDto
            {
                Id = service.Id,
                Name = service.Name,
                Tags = service.Tags,
                Meta = service.Meta,
                Port = service.Port,
                Address = service.Address,
                TaggedAddresses = service.TaggedAddresses,
                Weights = service.Weights,
                EnableTagOverride = service.EnableTagOverride,
                Proxy = service.Proxy ?? new object(),
                Connect = service.Connect ?? new object(),
                PeerName = ""
            };
        }
    }

    public class CheckDto
    {
        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("CheckID")]
        public string CheckId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Status")]
        public string Status { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Notes")]
        public string Notes { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Output")]
        public string Output { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ServiceID")]
        public string ServiceId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ServiceName")]
        public string ServiceName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ServiceTags")]
        public List<string> ServiceTags { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Type")]
        public string Type { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Interval")]
        public string Interval { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Timeout")]
        public string Timeout { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("ExposedPort")]
        public int ExposedPort { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Definition")]
        public object Definition { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [JsonPropertyName("Node")]
        public string Node { get; set; }

        public static CheckDto From(HealthData value)
        {
            return new CheckDto
            {
                Node = value.Node,
                CheckId = value.CheckId,
                Name = value.Name,
                Status = value.Status,
                Notes = value.Notes,
                Output = value.Output,
                ServiceId = value.ServiceId,
                ServiceName = value.ServiceName,
                ServiceTags = value.ServiceTags ?? [],
                Type = value.Type ?? "",
                Interval = value.Interval ?? "",
                Timeout = value.Timeout ?? "",
                ExposedPort = 0,
                Definition = new object()
            };
        }
    }
}