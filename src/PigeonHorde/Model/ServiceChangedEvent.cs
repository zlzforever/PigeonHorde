using System.Text.Json.Serialization;

namespace PigeonHorde.Model;

public class ServiceChangedEvent
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Id")]
    public string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("OperateType")]
    public OperateType OperateType { get; set; }
}