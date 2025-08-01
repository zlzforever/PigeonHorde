using System.Text.Json;
using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace PigeonHorde.Model;

public class Proxy
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("destination_service_id")]
    public string DestinationServiceId { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("destination_service_name")]
    public string DestinationServiceName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("local_service_address")]
    public string LocalServiceAddress { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("local_service_port")]
    public int LocalServicePort { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("local_service_socket_path")]
    public string LocalServiceSocketPath { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Mode")]
    public string Mode { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("transparent_proxy")]
    public JsonElement TransparentProxy { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Upstreams")]
    public List<JsonElement> Upstreams { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("mesh_gateway")]
    public JsonElement MeshGateway { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Expose")]
    public JsonElement Expose { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Config")]
    public Dictionary<string, JsonElement> Config { get; set; }
}