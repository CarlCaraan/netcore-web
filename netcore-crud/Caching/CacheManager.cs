using Microsoft.Extensions.Caching.Memory;

namespace netcore_crud.Caching
{
    public class CacheManager
    {
        private readonly IMemoryCache _cache;

        public CacheManager(IMemoryCache cache)
        {
            _cache = cache;
        }

        public void Clear(params string[] cacheKeys)
        {
            foreach (string cacheKey in cacheKeys)
            {
                _cache.Remove(cacheKey);
            }
        }
    }
}
