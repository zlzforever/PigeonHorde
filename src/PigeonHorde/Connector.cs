using FreeRedis;

namespace PigeonHorde;

public static class Connector
{
    public static RedisClient Redis;

    public static void Load(ConnectionStringBuilder connectionStringBuilder)
    {
        Redis = new RedisClient(connectionStringBuilder);
    }
}