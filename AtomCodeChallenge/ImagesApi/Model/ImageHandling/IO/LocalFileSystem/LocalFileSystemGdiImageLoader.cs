using System;
using System.Collections.Generic;
using GDI = System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ImagesApi.Model.ImageHandling.IO.LocalFileSystem
{
    public class LocalFileSystemGdiImageLoader : IGdiImageLoader
    {
        private static readonly List<string> SupportedFileExtensions = ImageTypeHelpers.FileExtensionMappings.Values
            .SelectMany(extensions => extensions)
            .ToList();

        private readonly ILocalFileSystemConfiguration _fileSystemConfig;

        public LocalFileSystemGdiImageLoader(ILocalFileSystemConfiguration fileSystemConfig)
        {
            _fileSystemConfig = fileSystemConfig;
        }

        public async Task<GDI.Image> LoadGdiImageAsync(string imageName)
        {
            if (string.IsNullOrWhiteSpace(imageName)) throw new ArgumentException("imageName must not be null or white space");

            var localLibraryFolderPath = _fileSystemConfig.GetLocalLibraryFolderPath();
            if (string.IsNullOrWhiteSpace(localLibraryFolderPath)) return null;

            var fullPathToImage = FindLibraryImagePath(localLibraryFolderPath, imageName);
            if (fullPathToImage == null) return null;

            await using var byteStream = await GetImageFileStreamAsync(fullPathToImage);
            return GDI.Image.FromStream(byteStream);
        }

        private static string FindLibraryImagePath(string localLibraryFolderPath, string imageName)
        {
            var libraryDir = new DirectoryInfo(localLibraryFolderPath);
            if (!libraryDir.Exists) throw new GdiImageLoaderFileSystemException($"Local library folder '{localLibraryFolderPath}' was not found");

            var fileMatches = libraryDir.GetFiles($"{imageName}.*", SearchOption.AllDirectories);
            var foundImage = fileMatches.FirstOrDefault(fm => IsSupportedImageType(fm));
            return foundImage?.FullName;
        }

        private static bool IsSupportedImageType(FileInfo fm)
        {
            var sanitisedImageExtension = fm.Extension.TrimStart('.');
            return SupportedFileExtensions
                .Any(ext => string.Equals(ext, sanitisedImageExtension, StringComparison.OrdinalIgnoreCase));
        }

        private static async Task<MemoryStream> GetImageFileStreamAsync(string fullPathToImage)
        {
            var imageBytes = await File.ReadAllBytesAsync(fullPathToImage);
            return new MemoryStream(imageBytes);
        }
    }
}