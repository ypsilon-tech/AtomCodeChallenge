using System;
using System.Threading.Tasks;

namespace ImagesApi.Model.ImageHandling.IO
{
    public class ImageLoader : IImageLoader
    {
        private readonly IGdiImageLoader _gdiImageLoader;

        public ImageLoader(IGdiImageLoader gdiImageLoader)
        {
            _gdiImageLoader = gdiImageLoader;
        }

        public async Task<Image> LoadFromLibraryAsync(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("imageName must not be null or white space");

            var gdiImage = await _gdiImageLoader.LoadGdiImageAsync(imageName);
            return gdiImage != null ? new Image(imageName, gdiImage) : null;
        }
    }
}