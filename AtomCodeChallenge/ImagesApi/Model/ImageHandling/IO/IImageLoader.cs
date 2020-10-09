using System.Threading.Tasks;

namespace ImagesApi.Model.ImageHandling.IO
{
    public interface IImageLoader
    {
        Task<Image> LoadFromLibraryAsync(string imageName);
    }
}