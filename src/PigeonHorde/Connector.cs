using FreeRedis;

namespace PigeonHorde;

public static class Connector
{
    public static RedisClient Redis;

    public static void Load(ConnectionStringBuilder connectionStringBuilder)
    {
        Redis = new RedisClient(connectionStringBuilder);
    }

    public static void Load()
    {
        var password = Environment.GetEnvironmentVariable("PIGEON_HORDE_PASSWORD");
        var connectionStringBuilder =
            ConnectionStringBuilder.Parse($"127.0.0.1:6379,password={password},defaultDatabase=0");
        Redis = new RedisClient(connectionStringBuilder);
    }
}