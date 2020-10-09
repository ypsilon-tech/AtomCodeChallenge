using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model.ImageHandling
{
    public class ResolutionFormatException : ImageHandlingFormatException
    {
        public ResolutionFormatException()
        {
        }

        protected ResolutionFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ResolutionFormatException(string message) : base(message)
        {
        }

        public ResolutionFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}