using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model
{
    public class ImageLibraryException : Exception
    {
        public ImageLibraryException()
        {
        }

        protected ImageLibraryException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ImageLibraryException(string message) : base(message)
        {
        }

        public ImageLibraryException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}