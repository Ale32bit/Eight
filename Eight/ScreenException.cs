using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Eight {
    public class ScreenException : Exception {
        public ScreenException() { }

        public ScreenException(string? message) : base(message) { }

        public ScreenException(string? message, Exception? innerException) : base(message, innerException) { }

        protected ScreenException(SerializationInfo info, StreamingContext context) : base(info, context) { }
    }
}