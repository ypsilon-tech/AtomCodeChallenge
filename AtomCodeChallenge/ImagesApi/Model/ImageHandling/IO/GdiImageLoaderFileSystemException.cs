using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model.ImageHandling.IO
{
    public class GdiImageLoaderFileSystemException : Exception
    {
        public GdiImageLoaderFileSystemException()
        {
        }

        protected GdiImageLoaderFileSystemException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public GdiImageLoaderFileSystemException(string message) : base(message)
        {
        }

        public GdiImageLoaderFileSystemException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}