using System.Text.Json.Serialization;

namespace PigeonHorde.Model;

public class AddressInfo
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Address")]
    public string Address { get; set; }
 
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Port")]
    public int Port { get; set; }
}