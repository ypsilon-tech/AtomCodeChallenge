using System;
using System.Threading.Tasks;
using ImagesApi.Model.Caching;
using ImagesApi.Model.ImageHandling;
using ImagesApi.Model.ImageHandling.IO;

namespace ImagesApi.Model
{
    public class ImagesLibrary : IImagesLibrary
    {
        private readonly IImagesCache _imagesCache;
        private readonly IImageLoader _imageLoader;
        private readonly IImageTransformer _imageTransformer;

        public ImagesLibrary(IImagesCache imagesCache, IImageLoader imageLoader, IImageTransformer imageTransformer)
        {
            _imagesCache = imagesCache;
            _imageLoader = imageLoader;
            _imageTransformer = imageTransformer;
        }

        public async Task<Image> GetImageAsync(string imageName, string imageResolution = null, string backgroundColour = null,
            string watermarkText = null, string imageType = null)
        {
            if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("imageName must not be null or white space");

            Image image;

            try
            {
                var cacheKey = new ImageCacheKey(imageName, imageResolution, backgroundColour, watermarkText, imageType);

                if ((image = await _imagesCache.GetImageAsync(cacheKey)) != null) return image;

                if ((image = await _imageLoader.LoadFromLibraryAsync(imageName)) == null) throw new ImageNotAvailableException();

                image = _imageTransformer.ApplyTransforms(image, imageResolution, backgroundColour, watermarkText, imageType);

                await _imagesCache.CacheImageAsync(cacheKey, image);
            }
            catch (Exception e) when (e.GetType() != typeof(ImageNotAvailableException))
            {
                throw new ImageLibraryException("An unexpected error occurred while retrieving the requested image.", e);
            }

            return image;
        }
    }
}