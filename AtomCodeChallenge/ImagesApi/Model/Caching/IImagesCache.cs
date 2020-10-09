using System.Threading.Tasks;

namespace ImagesApi.Model.Caching
{
    public interface IImagesCache
    {
        Task<Image> GetImageAsync(ImageCacheKey cacheKey);
        Task CacheImageAsync(ImageCacheKey cacheKey, Image image);
    }
}