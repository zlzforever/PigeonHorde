using System.Text.Json.Serialization;
using PigeonHorde.Model;

namespace PigeonHorde.Dto.Agent;

public class ListServicesItemDto
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
    [JsonPropertyName("Datacenter")]
    public string Datacenter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("ContentHash")]
    public string ContentHash { get; set; }

    public static ListServicesItemDto From(Service service)
    {
        return new ListServicesItemDto
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
            Datacenter = Defaults.DataCenter,
            ContentHash = service.ContentHash
        };
    }
}