using System.Text.Json.Serialization;

namespace PigeonHorde.Model;

public class HealthData
{
    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("ID")]
    public string Id { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Node")]
    public string Node { get; set; }

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
    [JsonPropertyName("Timestamp")]
    public string Timestamp { get; set; }

    public static int GetInterval(Check check)
    {
        if (check == null)
        {
            return 5;
        }

        var interval = (string.IsNullOrWhiteSpace(check.Interval) ? "5s" : check.Interval)
            .Replace("s", "").Replace("S", "").Trim();
        return int.TryParse(interval, out var v) ? v : 5;
    }
}