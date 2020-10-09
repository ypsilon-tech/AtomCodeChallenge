using System;
using GDI = System.Drawing;
using System.IO;
using System.Text;

namespace ImagesApi.Model
{
    public class Image : IDisposable
    {
        private readonly GDI.Image _gdiImage;
        private bool _disposed;

        public string Name { get; }

        public Image()
        {
            Name = string.Empty;
        }

        public Image(string name)
        {
            Name = name;
        }

        public Image(string name, GDI.Image gdiImage)
            : this(name)
        {
            _gdiImage = gdiImage;
        }

        public byte[] ToBytes()
        {
            var byteStream = new MemoryStream();
            using var writer = new BinaryWriter(byteStream, Encoding.UTF8, false);

            writer.Write(nameof(Image));
            writer.Write(Name);

            var gdiImageBytes = GetGdiImageBytes(_gdiImage);
            writer.Write(gdiImageBytes.Length);
            writer.Write(gdiImageBytes);

            return byteStream.ToArray();
        }

        public static Image FromBytes(byte[] imageBytes)
        {
            if (imageBytes == null) throw new ArgumentNullException(nameof(imageBytes));

            var byteStream = VerifyBytesAndGetStream(imageBytes);
            if (byteStream == null) return null;

            using var reader = new BinaryReader(byteStream, Encoding.UTF8, false);

            var imageName = reader.ReadString();

            int imageBytesLength;

            if ((imageBytesLength = reader.ReadInt32()) <= 0) return new Image(imageName);

            var gdiImageBytes = reader.ReadBytes(imageBytesLength);
            var gdiImage = GetGdiImageFromBytes(gdiImageBytes);
            return new Image(imageName, gdiImage);

        }

        public GDI.Image ToGdiImage()
        {
            return _gdiImage;
        }

        private static MemoryStream VerifyBytesAndGetStream(byte[] imageBytes)
        {
            if (imageBytes.Length == 0) return null;

            var byteStream = new MemoryStream(imageBytes);
            using var reader = new BinaryReader(byteStream, Encoding.UTF8, true);
            var objectHeader = reader.ReadString();
            if (objectHeader == nameof(Image)) return byteStream;

            byteStream.Close();
            return null;
        }

        private static byte[] GetGdiImageBytes(GDI.Image gdiImage)
        {
            if (gdiImage == null) return Array.Empty<byte>();

            var converter = new GDI.ImageConverter();
            return (byte[])converter.ConvertTo(gdiImage, typeof(byte[]));
        }

        private static GDI.Image GetGdiImageFromBytes(byte[] gdiImageBytes)
        {
            if (gdiImageBytes == null) throw new ArgumentNullException(nameof(gdiImageBytes));

            var converter = new GDI.ImageConverter();
            return (GDI.Image)converter.ConvertFrom(gdiImageBytes);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool isDisposing)
        {
            if (_disposed) return;

            if (isDisposing)
            {
                _gdiImage?.Dispose();
                GC.SuppressFinalize(this);
            }

            _disposed = true;
        }

        ~Image()
        {
            Dispose(false);
        }
    }
}