using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model.ImageHandling
{
    public abstract class ImageHandlingFormatException : Exception
    {
        protected ImageHandlingFormatException()
        {
        }

        protected ImageHandlingFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        protected ImageHandlingFormatException(string message) : base(message)
        {
        }

        protected ImageHandlingFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}