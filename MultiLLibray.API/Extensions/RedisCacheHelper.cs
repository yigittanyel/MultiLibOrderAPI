using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace MultiLLibray.API.Extensions;

public class RedisCacheHelper
{
    private readonly IDistributedCache _cache;

    public RedisCacheHelper(IDistributedCache cache)
    {
        _cache = cache;
    }

    public async Task<T> GetCacheValueAsync<T>(string key)
    {
        string cachedData = await _cache.GetStringAsync(key);

        if (cachedData != null)
        {
            return JsonConvert.DeserializeObject<T>(cachedData);
        }

        return default;
    }

    public async Task SetCacheValueAsync<T>(string key, T value, TimeSpan expiration)
    {
        string serializedData = JsonConvert.SerializeObject(value);

        await _cache.SetStringAsync(key, serializedData, new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = expiration
        });
    }

    public async Task RemoveCacheValueAsync(string key)
    {
        await _cache.RemoveAsync(key);
    }
}


