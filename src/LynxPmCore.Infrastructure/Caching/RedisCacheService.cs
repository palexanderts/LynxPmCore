using LynxPmCore.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using StackExchange.Redis;

namespace LynxPmCore.Infrastructure.Caching;

internal sealed class RedisCacheService(
    IConnectionMultiplexer redis,
    ILogger<RedisCacheService> logger) : ICacheService
{
    private readonly IDatabase _db = redis.GetDatabase();

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default)
    {
        try
        {
            var value = await _db.StringGetAsync(key);
            return value.HasValue ? JsonSerializer.Deserialize<T>(value!) : default;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache GET failed for key {Key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value);
            await _db.StringSetAsync(key, json, expiry ?? TimeSpan.FromMinutes(10));
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Cache SET failed for key {Key}", key);
        }
    }

    public async Task RemoveAsync(string key, CancellationToken ct = default)
    {
        try { await _db.KeyDeleteAsync(key); }
        catch (Exception ex) { logger.LogWarning(ex, "Cache DELETE failed for key {Key}", key); }
    }

    public async Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default)
    {
        try
        {
            var server = redis.GetServer(redis.GetEndPoints().First());
            var keys = server.Keys(pattern: $"{prefix}*").ToArray();
            if (keys.Length > 0) await _db.KeyDeleteAsync(keys);
        }
        catch (Exception ex) { logger.LogWarning(ex, "Cache DELETE prefix failed for {Prefix}", prefix); }
    }
}
