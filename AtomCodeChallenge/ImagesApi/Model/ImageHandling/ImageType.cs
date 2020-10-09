using System.Drawing.Imaging;

namespace ImagesApi.Model.ImageHandling
{
    public class ImageType
    {
        public static ImageType None = new ImageType { ImageFormat = null };

        public ImageFormat ImageFormat { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is ImageType imageType && imageType.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{ImageFormat}".ToLower();
        }

        public static ImageType Parse(string imageType)
        {
            if (string.IsNullOrWhiteSpace(imageType)) return ImageType.None;

            if (!ImageTypeHelpers.ImageFormatMappings.ContainsKey(imageType.Trim())) throw new ImageTypeFormatException($"'{imageType}' is not a supported image type");
            return new ImageType { ImageFormat = ImageTypeHelpers.ImageFormatMappings[imageType.Trim()] };
        }

        public static bool operator ==(ImageType imgTypeA, ImageType imgTypeB)
        {
            return Equals(imgTypeA, imgTypeB);
        }

        public static bool operator !=(ImageType imgTypeA, ImageType imgTypeB)
        {
            return !Equals(imgTypeA, imgTypeB);
        }
    }
}