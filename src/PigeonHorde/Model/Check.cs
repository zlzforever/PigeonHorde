using System.Text.Json.Serialization;

namespace PigeonHorde.Model;

public class Check
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
    [JsonPropertyName("Interval")]
    public string Interval { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("HTTP")]
    public string Http { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Timeout")]
    public string Timeout { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("DeregisterCriticalServiceAfter")]
    public string DeregisterCriticalServiceAfter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Header")]
    public string Header { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Method")]
    public string Method { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("Body")]
    public string Body { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("TLSServerName")]
    public string TlsServerName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("TLSSkipVerify")]
    public bool TlsSkipVerify { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("TCP")]
    public string Tcp { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("TCPUseTLS")]
    public bool TcpUseTls { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("UDP")]
    public string Udp { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("GRPC")]
    public string Grpc { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("OSService")]
    public string OsService { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [JsonPropertyName("GRPCUseTLS")]
    public bool GrpcUseTls { get; set; }

    public string GetCheckType()
    {
        return !string.IsNullOrEmpty(Http) ? "HTTP" : "";
    }

    public HealthData CreateHealthData(string serviceId, string serviceName, List<string> serviceTags)
    {
        return CreateHealthData(serviceId, serviceName, serviceTags, this);
    }

    public static HealthData CreateHealthData(string serviceId, string serviceName, List<string> serviceTags,
        Check check)
    {
        return new HealthData
        {
            Id = check.CheckId,
            Node = Defaults.DataCenter,
            CheckId = check.CheckId,
            Timeout = check.Timeout,
            Interval = check.Interval,
            Name = check.Name,
            Status = "passing",
            Notes = "",
            Output = "",
            ServiceId = serviceId,
            ServiceName = serviceName,
            ServiceTags = serviceTags,
            Type = check.GetCheckType(),
            Timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
    }
}