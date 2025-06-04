using System.Text.Json.Serialization;

namespace PigeonHorde.Dto.Agent;

public class VersionDto
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("SHA")]
    public string SHA { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("BuildDate")]
    public string BuildDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("HumanVersion")]
    public string HumanVersion { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("FIPS")]
    public string FIPS { get; set; }
}