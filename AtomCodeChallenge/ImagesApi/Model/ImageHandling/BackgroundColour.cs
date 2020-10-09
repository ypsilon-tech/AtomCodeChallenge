using System;
using System.Drawing;
using System.Linq;

namespace ImagesApi.Model.ImageHandling
{
    public class BackgroundColour
    {
        public static BackgroundColour None = new BackgroundColour {Colour = Color.FromArgb(0)};

        public Color Colour { get; private set; }

        public override bool Equals(object obj)
        {
            return obj is BackgroundColour backgroundColour && backgroundColour.ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return $"#{Convert.ToString(Colour.ToArgb(), 16)}";
        }

        public static BackgroundColour Parse(string backgroundColour)
        {
            if (string.IsNullOrWhiteSpace(backgroundColour)) return None;

            var colour = Color.FromName(backgroundColour);
            if (colour.ToArgb() != 0) return new BackgroundColour {Colour = colour};

            if (backgroundColour.Length != 6) throw new ColourFormatException($"'{backgroundColour}' is not a valid colour format");

            var rgbValues = Enumerable.Range(0, 3)
                .Select(index => backgroundColour.Substring(0 + (index * 2), 2))
                .Select(hexVal =>
                {
                    try
                    {
                        return Convert.ToInt32(hexVal, 16);
                    }
                    catch
                    {
                        return -1;
                    }
                })
                .ToList();
            if (rgbValues.Count != 3) throw new ColourFormatException($"'{backgroundColour}' is not a valid colour format");

            return new BackgroundColour {Colour = Color.FromArgb(rgbValues[0], rgbValues[1], rgbValues[2])};
        }

        public static bool operator ==(BackgroundColour colour1, BackgroundColour colour2)
        {
            return Equals(colour1, colour2);
        }

        public static bool operator !=(BackgroundColour colour1, BackgroundColour colour2)
        {
            return !Equals(colour1, colour2);
        }
    }
}