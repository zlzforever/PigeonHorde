using System.Security.Cryptography;

namespace PigeonHorde;

public static class Defaults
{
    public static readonly string Version;
    public static readonly string Sha;
    public static readonly string BuildDate;
    public static readonly string DataCenter;

    static Defaults()
    {
        var executablePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PigeonHorde");
        var assemblyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "PigeonHorde.dll");
        var path = File.Exists(executablePath) ? executablePath : assemblyPath;
        using var sha1 = SHA1.Create();
        using var stream = File.OpenRead(path);
        var hashBytes = sha1.ComputeHash(stream);
        Sha = Convert.ToHexString(hashBytes)[..8];

        var fileInfo = new FileInfo(path);
        BuildDate = fileInfo.CreationTime.ToString("yyyy-MM-dd HH:mm:ss");
        Version = typeof(Program).Assembly.GetName().Version?.ToString();

        var dc = Environment.GetEnvironmentVariable("PIGEON_HORDE_DATA_CENTER");
        DataCenter = string.IsNullOrEmpty(dc) ? "dc1" : dc;
    }
}