using System.Text.Json.Serialization;
using PigeonHorde.Model;

namespace PigeonHorde.Dto.Agent;

public class GetServiceConfigurationDto
{
    /// <summary>
    /// Specifies a unique ID for this service.
    /// This must be unique per agent. This defaults to the Name parameter if not provided
    /// </summary>
    [JsonPropertyName("ID")]
    public string Id { get; set; }

    /// <summary>
    /// Specifies the logical name of the service.
    /// Many service instances may share the same logical service name.
    /// We recommend using valid DNS labels for service definition names.
    /// Refer to the Internet Engineering Task Force's RFC 1123 for additional information.
    /// Service names that conform to standard usage ensures compatibility with external DNSs
    /// </summary>
    [JsonPropertyName("Service")]
    public string Name { get; set; }

    /// <summary>
    /// Specifies a list of tags to assign to the service.
    /// Tags enable you to filter when querying for the services and are exposed in Consul APIs.
    /// We recommend using valid DNS labels for tags.
    /// Refer to the Internet Engineering Task Force's RFC 1123 for additional information.
    /// Tags that conform to standard usage ensures compatibility with external DNSs.
    /// </summary>
    [JsonPropertyName("Tags")]
    public List<string> Tags { get; set; }

    /// <summary>
    /// Specifies the address of the service. If not provided, the agent's address is used as the address for the service during DNS queries.
    /// </summary>
    [JsonPropertyName("Address")]
    public string Address { get; set; }

    /// <summary>
    /// Specifies a map of explicit LAN and WAN addresses for the service instance. Both the address and port can be specified within the map values.
    /// </summary>
    [JsonPropertyName("TaggedAddresses")]
    public Dictionary<string, AddressInfo> TaggedAddresses { get; set; }

    /// <summary>
    /// Specifies arbitrary KV metadata linked to the service instance.
    /// </summary>
    [JsonPropertyName("Meta")]
    public Dictionary<string, string> Meta { get; set; }

    /// <summary>
    /// Specifies the port of the service.
    /// </summary>
    [JsonPropertyName("Port")]
    public int Port { get; set; }

    /// <summary>
    /// The kind of service. Defaults to "", which is a typical Consul service
    /// connect-proxy
    /// mesh-gateway
    /// terminating-gateway
    /// ingress-gateway
    /// </summary>
    [JsonPropertyName("Kind")]
    public string Kind { get; set; } = "";

    /// <summary>
    /// Specifies the configuration for a service mesh proxy instance. This is only valid if Kind defines a proxy or gateway
    /// </summary>
    [JsonPropertyName("Proxy")]
    public Proxy Proxy { get; set; }

    /// <summary>
    /// Specifies the configuration for service mesh.
    /// The connect subsystem provides Consul's service mesh capabilities.
    /// </summary>
    [JsonPropertyName("Connect")]
    public Connect Connect { get; set; }

    /// <summary>
    /// Specifies a check. Please see the check documentation for more information about the accepted fields.
    /// If you don't provide a name or id for the check then they will be generated.
    /// To provide a custom id and/or name set the CheckID and/or Name field.
    /// </summary>
    [JsonPropertyName("Check")]
    public Check Check { get; set; }

    /// <summary>
    /// Check & Checks 互斥
    /// </summary>
    [JsonPropertyName("Checks")]
    public List<Check> Checks { get; set; }

    /// <summary>
    /// Specifies to disable the anti-entropy feature for this service's tags.
    /// If EnableTagOverride is set to true then external agents can update this service in the catalog and modify the tags.
    /// Subsequent local sync operations by this agent will ignore the updated tags.
    /// For instance, if an external agent modified both the tags and the port for this service and EnableTagOverride was set to
    /// true then after the next sync cycle the service's port would revert to the original value but the tags would maintain the updated value.
    /// As a counter example, if an external agent modified both the tags and port for this service and EnableTagOverride was set to false then
    /// after the next sync cycle the service's port and the tags would revert to the original value and all modifications would be lost.
    /// </summary>
    [JsonPropertyName("EnableTagOverride")]
    public bool EnableTagOverride { get; set; }

    /// <summary>
    ///  Specifies weights for the service.
    /// Refer to Services Configuration Reference for additional information. Default is {"Passing": 1, "Warning": 1}
    /// </summary>
    [JsonPropertyName("Weights")]
    public Weights Weights { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("ContentHash")]
    public string ContentHash { get; set; }

    public static GetServiceConfigurationDto From(Service service)
    {
        return new GetServiceConfigurationDto
        {
            Address = service.Address,
            Id = service.Id,
            Name = service.Name,
            Tags = service.Tags,
            Meta = service.Meta,
            Port = service.Port,
            TaggedAddresses = service.TaggedAddresses,
            Kind = service.Kind,
            Proxy = service.Proxy,
            Connect = service.Connect,
            Check = service.Check,
            Checks = service.Checks,
            EnableTagOverride = service.EnableTagOverride,
            Weights = service.Weights,
            ContentHash = service.ContentHash
        };
    }
}