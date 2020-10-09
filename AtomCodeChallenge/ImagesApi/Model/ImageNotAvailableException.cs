using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model
{
    public class ImageNotAvailableException : Exception
    {
        public ImageNotAvailableException()
        {
        }

        protected ImageNotAvailableException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ImageNotAvailableException(string message) : base(message)
        {
        }

        public ImageNotAvailableException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}