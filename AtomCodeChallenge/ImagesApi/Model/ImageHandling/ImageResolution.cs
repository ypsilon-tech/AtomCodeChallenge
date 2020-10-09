using System;
using System.Linq;

namespace ImagesApi.Model.ImageHandling
{
    public class ImageResolution
    {
        public static ImageResolution None = new ImageResolution {Height = 0, Width = 0};

        public int Width { get; private set; }
        public int Height { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is ImageResolution resolution && resolution.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"{Width}x{Height}";
        }

        public static ImageResolution Parse(string imageResolution)
        {
            if (imageResolution == null) return None;

            var parts = imageResolution.ToLower().Split("x", StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length > 2) 
                throw new ResolutionFormatException($"'{imageResolution}' is not a valid resolution format");

            var values = parts.Select(part =>
                {
                    var isNum = int.TryParse(part, out var value);
                    return isNum ? value : 0;
                })
                .Where(val => val > 0)
                .ToList();

            if (values.Count != parts.Length)
                throw new ResolutionFormatException($"'{imageResolution}' is not a valid resolution format");

            return new ImageResolution
            {
                Width = values[0],
                Height = values.Count < 2 ? values[0] : values[1]
            };
        }

        public static bool operator ==(ImageResolution resA, ImageResolution resB)
        {
            return Equals(resA, resB);
        }

        public static bool operator !=(ImageResolution resA, ImageResolution resB)
        {
            return !Equals(resA, resB);
        }
    }
}