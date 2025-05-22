using System.Text.Json.Serialization;

namespace PigeonHorde.Model;

public class Weights
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Passing")]
    public int Passing { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Warning")]
    public int Warning { get; set; }
}