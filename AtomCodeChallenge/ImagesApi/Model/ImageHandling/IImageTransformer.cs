namespace ImagesApi.Model.ImageHandling
{
    public interface IImageTransformer
    {
        Image ApplyTransforms(Image image, string newResolution = null, string backgroundColour = null,
            string watermarkText = null, string imageType = null);
    }
}