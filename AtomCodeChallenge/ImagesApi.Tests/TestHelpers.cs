using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace ImagesApi.Tests
{
    public static class TestHelpers
    {
        public static Image GetTestImage(Color? imageColour = null, bool makeTransparent = false, int width = 20,
            int height = 20, string watermark = null)
        {
            imageColour ??= Color.DarkSeaGreen;

            using var testImage = new Bitmap(width, height);
            using var graphics = Graphics.FromImage(testImage);
            using var brush = new SolidBrush(imageColour.Value);
            graphics.FillRectangle(brush, 0, 0, width, height);
            graphics.FillEllipse(Brushes.Black, 5, 5, width - 5, height - 5);
            if (makeTransparent) testImage.MakeTransparent(imageColour.Value);
            if (!string.IsNullOrWhiteSpace(watermark)) AddWatermark(graphics, watermark);


            // Convert from default in-memory to PNG format to avoid needing to use encoders
            // when serialising to byte[] in future
            var imageStream = new MemoryStream();
            testImage.Save(imageStream, ImageFormat.Png);

            return GetGdiImageFromStream(imageStream);
        }

        public static CompareResult CompareImages(Image imgA, Image imgB)
        {
            if (!imgA.RawFormat.Equals(imgB.RawFormat)) return CompareResult.DifferentFormat;
            if (imgA.Height != imgB.Height || imgA.Width != imgB.Width) return CompareResult.DifferentSize;

            var compareResult = CompareResult.Same;
            using var bmpA = new Bitmap(imgA);
            using var bmpB = new Bitmap(imgB);

            for (var x = 0; x < imgA.Width && compareResult == CompareResult.Same; x++)
            {
                for (var y = 0; y < imgA.Height && compareResult == CompareResult.Same; y++)
                {
                    if (bmpA.GetPixel(x, y) != bmpB.GetPixel(x, y))
                        compareResult = CompareResult.DifferentPixels;
                }
            }

            return compareResult;
        }

        public static Image GetGdiImageFromStream(Stream imageStream)
        {
            using (imageStream)
            {
                return Image.FromStream(imageStream);
            }
        }

        private static void AddWatermark(Graphics graphics, string watermark)
        {
            using var font = new Font(FontFamily.GenericMonospace, 12f, GraphicsUnit.Point);
            using var brush = new SolidBrush(Color.FromArgb(150, Color.Black));
            graphics.DrawString(watermark.Trim(), font, brush, 5f, 5f);
        }
    }

    public enum CompareResult
    {
        Same,
        DifferentFormat,
        DifferentSize,
        DifferentPixels
    }
}