using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model.ImageHandling
{
    public class ImageTypeFormatException : ImageHandlingFormatException
    {
        public ImageTypeFormatException()
        {
        }

        protected ImageTypeFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ImageTypeFormatException(string message) : base(message)
        {
        }

        public ImageTypeFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}