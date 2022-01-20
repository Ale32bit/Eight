using System.Runtime.Serialization;

namespace Eight
{
    public class ScreenException : Exception
    {
        public ScreenException() { }

        public ScreenException(string? message) : base(message) { }

        public ScreenException(string? message, Exception? innerException) : base(message, innerException) { }

        protected ScreenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}