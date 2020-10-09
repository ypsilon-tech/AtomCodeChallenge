using System;
using GDI = System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;

namespace ImagesApi.Model.ImageHandling
{
    public class ImageTransformer : IImageTransformer
    {
        public Image ApplyTransforms(Image image, string newResolution = null, string backgroundColour = null,
            string watermarkText = null, string imageType = null)
        {
            if (image == null) throw new ArgumentNullException(nameof(image));
            if (image.ToGdiImage() == null) throw new ArgumentException("image must contain a GDI image object");
            if (!TransformsSupplied(newResolution, backgroundColour, watermarkText, imageType)) return image;

            var resolution = ImageResolution.Parse(newResolution);
            var backColour = BackgroundColour.Parse(backgroundColour);
            var imgType = ImageType.Parse(imageType);
            
            using var oldImage = image;
            var gdiImage = oldImage.ToGdiImage();
            var newGdiImage = CreateTransformBitmap(resolution, gdiImage);
            using var graphics = GDI.Graphics.FromImage(newGdiImage);

            ApplyBackgroundColour(graphics, backColour, newGdiImage);

            if (resolution != ImageResolution.None)
            {
                DrawImageAtNewResolution(graphics, resolution, gdiImage);
            }
            else
            {
                DrawImageAtOriginalResolution(graphics, gdiImage);
            }

            if (!string.IsNullOrWhiteSpace(watermarkText)) AddWatermark(graphics, watermarkText);

            if (imgType == ImageType.None) imgType = ImageType.Parse(gdiImage.RawFormat.ToString());
            if (!newGdiImage.RawFormat.Equals(imgType.ImageFormat)) newGdiImage = SetNewGdiImageFormat(newGdiImage, imgType.ImageFormat);

            return new Image(image.Name, newGdiImage);
        }

        private static bool TransformsSupplied(params string[] transforms)
        {
            return string.Concat(transforms).Trim().Length != 0;
        }

        private static GDI.Bitmap CreateTransformBitmap(ImageResolution resolution, GDI.Image gdiImage)
        {
            return resolution != ImageResolution.None
                ? new GDI.Bitmap(resolution.Width, resolution.Height)
                : new GDI.Bitmap(gdiImage.Width, gdiImage.Height);
        }

        private void ApplyBackgroundColour(GDI.Graphics graphics, BackgroundColour backColour, GDI.Bitmap newGdiImage)
        {
            if (backColour == BackgroundColour.None) backColour = BackgroundColour.Parse("White");
            using var brush = new GDI.SolidBrush(backColour.Colour);
            graphics.FillRectangle(brush, 0, 0, newGdiImage.Width, newGdiImage.Height);
        }

        private static void DrawImageAtNewResolution(GDI.Graphics graphics, ImageResolution resolution, GDI.Image gdiImage)
        {
            graphics.SmoothingMode = SmoothingMode.HighQuality;
            graphics.InterpolationMode = InterpolationMode.Bicubic;
            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            graphics.DrawImage(gdiImage, 0, 0, resolution.Width, resolution.Height);
        }

        private static void DrawImageAtOriginalResolution(GDI.Graphics graphics, GDI.Image gdiImage)
        {
            graphics.DrawImage(gdiImage, 0, 0, gdiImage.Width, gdiImage.Height);
        }

        private static void AddWatermark(GDI.Graphics graphics, string watermarkText)
        {
            using var font = new GDI.Font(GDI.FontFamily.GenericMonospace, 12f, GDI.GraphicsUnit.Point);
            using var brush = new GDI.SolidBrush(GDI.Color.FromArgb(150, GDI.Color.Black));
            graphics.DrawString(watermarkText.Trim(), font, brush, 5f,5f);
        }

        private static GDI.Bitmap SetNewGdiImageFormat(GDI.Bitmap gdiImage, ImageFormat imageFormat)
        {
            using var prevGdiImage = gdiImage;
            var stream = new MemoryStream();
            prevGdiImage.Save(stream, imageFormat);
            return new GDI.Bitmap(stream);
        }
    }
}