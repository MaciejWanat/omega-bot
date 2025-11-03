using Microsoft.Extensions.Caching.Memory;

namespace OmegaBot.Application.Extensions
{
    public static class MemoryCacheExtensions
    {
        public static Task<T?> GetOrCreateWithDefaultExpirationAsync<T>(
            this IMemoryCache cache,
            object key,
            Func<Task<T>> factory,
            TimeSpan? defaultTtl = null)
        {
            defaultTtl ??= TimeSpan.FromDays(1);

            return cache.GetOrCreateAsync(key, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = defaultTtl;
                return await factory().ConfigureAwait(false);
            });
        }
    }

}
