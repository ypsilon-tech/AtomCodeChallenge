using System;
using System.Security.Cryptography;
using System.Text;
using ImagesApi.Model.ImageHandling;

namespace ImagesApi.Model.Caching
{
    public class ImageCacheKey
    {
        private readonly ImageResolution _imageResolution;
        private readonly BackgroundColour _backgroundColour;
        private readonly string _watermarkHash;
        private readonly ImageType _imageType;

        public string ImageName { get; }

        public ImageCacheKey(string imageName, string imageResolution = null, string backgroundColour = null,
            string watermarkText = null, string imageType = null)
        {
            _imageResolution = ImageResolution.Parse(imageResolution);
            _backgroundColour = BackgroundColour.Parse(backgroundColour);
            if (!string.IsNullOrWhiteSpace(watermarkText)) _watermarkHash = GetWatermarkStringHash(watermarkText);
            _imageType = ImageType.Parse(imageType);

            ImageName = imageName;
        }

        public override string ToString()
        {
            var keyString = new StringBuilder(ImageName);
            if (_imageResolution != ImageResolution.None) keyString.Append($"|res={_imageResolution}");
            if (_backgroundColour != BackgroundColour.None) keyString.Append($"|bgr={_backgroundColour}");
            if (!string.IsNullOrWhiteSpace(_watermarkHash)) keyString.Append($"|wm={_watermarkHash}");
            if (_imageType != ImageType.None) keyString.Append($"|type={_imageType}");
            return keyString.ToString();
        }

        public override bool Equals(object obj)
        {
            return obj is ImageCacheKey cacheKey && string.Equals(ToString(), cacheKey.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public static bool operator ==(ImageCacheKey key1, ImageCacheKey key2)
        {
            return Equals(key1, key2);
        }

        public static bool operator !=(ImageCacheKey key1, ImageCacheKey key2)
        {
            return !Equals(key1, key2);
        }

        private static string GetWatermarkStringHash(string watermarkText)
        {
            using var sha = new SHA256Managed();
            var watermarkHashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(watermarkText));
            return Convert.ToBase64String(watermarkHashBytes);
        }
    }
}