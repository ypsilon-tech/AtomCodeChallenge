using GDI = System.Drawing;
using System.Threading.Tasks;

namespace ImagesApi.Model.ImageHandling.IO
{
    public interface IGdiImageLoader
    { 
        Task<GDI.Image> LoadGdiImageAsync(string imageName);
    }
}