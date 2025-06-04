using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PigeonHorde.Extensions;

public static class HttpContextExtensions
{
    public static async Task<(T Entity, string ContentHash)> GetModelAsync<T>(this HttpContext context)
    {
        using var reader = new StreamReader(context.Request.Body);
        var body = await reader.ReadToEndAsync();
        var t = JsonSerializer.Deserialize<T>(body);
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(body));
        return (t, Convert.ToHexString(hash).ToLowerInvariant());
    }
}