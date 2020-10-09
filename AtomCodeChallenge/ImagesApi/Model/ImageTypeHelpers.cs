using System;
using System.Collections.Generic;
using System.Drawing.Imaging;

namespace ImagesApi.Model
{
    public static class ImageTypeHelpers
    {
        public static Dictionary<string, string> MimeTypeMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            {ImageFormat.Png.ToString(), "image/png"},
            {ImageFormat.Bmp.ToString(), "image/bmp"},
            {ImageFormat.Gif.ToString(), "image/gif"},
            {ImageFormat.Jpeg.ToString(), "image/jpeg"},
            {ImageFormat.Tiff.ToString(), "image/tiff"}
        };

        public static Dictionary<string, ImageFormat> ImageFormatMappings = new Dictionary<string, ImageFormat>(StringComparer.OrdinalIgnoreCase)
        {
            {ImageFormat.Png.ToString(), ImageFormat.Png},
            {ImageFormat.Bmp.ToString(), ImageFormat.Bmp},
            {ImageFormat.Gif.ToString(), ImageFormat.Gif},
            {ImageFormat.Jpeg.ToString(), ImageFormat.Jpeg},
            {ImageFormat.Tiff.ToString(), ImageFormat.Tiff}
        };

        public static Dictionary<string, string[]> FileExtensionMappings = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase)
        {
            {ImageFormat.Png.ToString(), new[] {"png"}},
            {ImageFormat.Bmp.ToString(), new[] {"bmp"}},
            {ImageFormat.Gif.ToString(), new[] {"gif"}},
            {ImageFormat.Jpeg.ToString(), new[] {"jpg", "jpeg"}},
            {ImageFormat.Tiff.ToString(), new[] {"tiff"}}
        };
    }
}