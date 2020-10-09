using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Distributed;

namespace ImagesApi.Model.Caching
{
    public class ImagesCache : IImagesCache
    {
        private readonly IDistributedCache _distributedCache;

        public ImagesCache(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public async Task<Image> GetImageAsync(ImageCacheKey cacheKey)
        {
            if (cacheKey == null) throw new ArgumentNullException(nameof(cacheKey));

            byte[] imageBytes;
            return (imageBytes = await _distributedCache.GetAsync(cacheKey.ToString())) == null
                ? null
                : Image.FromBytes(imageBytes);
        }

        public async Task CacheImageAsync(ImageCacheKey cacheKey, Image image)
        {
            if (cacheKey == null) throw new ArgumentNullException(nameof(cacheKey));
            if (image == null) throw new ArgumentNullException(nameof(image));

            await _distributedCache.SetAsync(
                cacheKey.ToString(),
                image.ToBytes(),
                new DistributedCacheEntryOptions());
        }
    }
}