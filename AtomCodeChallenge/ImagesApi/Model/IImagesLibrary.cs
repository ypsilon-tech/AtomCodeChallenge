using System.Threading.Tasks;

namespace ImagesApi.Model
{
    public interface IImagesLibrary
    {
        Task<Image> GetImageAsync(string imageName, string imageResolution = null, string backgroundColour = null,
            string watermarkText = null, string imageType = null);
    }
}