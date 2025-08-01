using System.Text.Json;
using System.Text.Json.Serialization;
// ReSharper disable UnusedMember.Global

namespace PigeonHorde.Model;

public class Connect
{
    /// <summary>
    /// Specifies whether this service supports the Consul service mesh protocol natively.
    /// If this is true, then service mesh proxies, DNS queries, etc. will be able to service discover this service.
    /// </summary>

    [JsonPropertyName("Native")]
    public bool Native { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("SidecarService")]
    public JsonElement SidecarService { get; set; }
}