using System;
using System.Runtime.Serialization;

namespace ImagesApi.Model.ImageHandling
{
    public class ColourFormatException : ImageHandlingFormatException
    {
        public ColourFormatException()
        {
        }

        protected ColourFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }

        public ColourFormatException(string message) : base(message)
        {
        }

        public ColourFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}