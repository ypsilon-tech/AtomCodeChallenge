using Microsoft.Extensions.Configuration;

namespace ImagesApi.Model.ImageHandling.IO.LocalFileSystem
{
    public class LocalFileSystemConfiguration : ILocalFileSystemConfiguration
    {
        private const string LibraryFolderPathKey = "LocalFileSystemImageLibrary:LibraryFolderPath";
        private readonly string _libraryFolderPath;

        public LocalFileSystemConfiguration(IConfiguration configuration)
        {
            _libraryFolderPath = configuration[LibraryFolderPathKey];
        }

        public string GetLocalLibraryFolderPath()
        {
            return _libraryFolderPath;
        }
    }
}