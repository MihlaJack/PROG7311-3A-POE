using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace GLMS.API.Services
{
    public class RedisCacheService : ICacheService
    {
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> _logger;

        public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<T?> GetAsync<T>(string key) where T : class
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                if (string.IsNullOrEmpty(data))
                    return null;

                return JsonSerializer.Deserialize<T>(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache get failed for key: {Key}", key);
                return null;
            }
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan? expiration = null) where T : class
        {
            try
            {
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration ?? TimeSpan.FromMinutes(10)
                };

                var data = JsonSerializer.Serialize(value);
                await _cache.SetStringAsync(key, data, options);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache set failed for key: {Key}", key);
            }
        }

        public async Task RemoveAsync(string key)
        {
            try
            {
                await _cache.RemoveAsync(key);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Cache remove failed for key: {Key}", key);
            }
        }

        public async Task<bool> ExistsAsync(string key)
        {
            try
            {
                var data = await _cache.GetStringAsync(key);
                return !string.IsNullOrEmpty(data);
            }
            catch
            {
                return false;
            }
        }
    }
}