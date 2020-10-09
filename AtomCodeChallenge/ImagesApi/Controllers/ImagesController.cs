using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using ImagesApi.Model;
using ImagesApi.Model.ImageHandling;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ImagesApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImagesController : ControllerBase
    {
        private readonly IImagesLibrary _imagesLibrary;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImagesLibrary imagesLibrary, ILogger<ImagesController> logger)
        {
            _imagesLibrary = imagesLibrary;
            _logger = logger;
        }

        [HttpGet("{imageName:required}")]
        public async Task<IActionResult> GetImage(string imageName, string resolution = null, string backgroundColour = null,
            string watermark = null, string imageType = null)
        {
            try
            {
                using var image = await _imagesLibrary.GetImageAsync(
                    imageName,
                    resolution,
                    backgroundColour,
                    watermark,
                    imageType);
                return GetFileResultForImage(image);
            }
            catch (ImageNotAvailableException)
            {
                return NotFound();
            }
            catch (ImageLibraryException ile) when (ile.InnerException is ImageHandlingFormatException fe)
            {
                return BadRequest(fe.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.ToString());
                return StatusCode((int)HttpStatusCode.InternalServerError);
            }
        }

        private IActionResult GetFileResultForImage(Image image)
        {
            return new FileStreamResult(
                GetFileStream(image),
                GetContentType(image));
        }

        private Stream GetFileStream(Image image)
        {
            var stream = new MemoryStream();
            var gdiImage = image.ToGdiImage();
            gdiImage.Save(stream, gdiImage.RawFormat);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }

        private string GetContentType(Image image)
        {
            return ImageTypeHelpers.MimeTypeMappings[image.ToGdiImage().RawFormat.ToString()];
        }
    }
}
