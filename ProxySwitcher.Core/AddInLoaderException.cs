using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Core
{
    [Serializable]
    public class AddInLoaderException : Exception
    {
        public AddInLoaderException() { }
        public AddInLoaderException(string message) : base(message) { }
        public AddInLoaderException(string message, Exception inner) : base(message, inner) { }
        protected AddInLoaderException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
