using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ProxySwitcher.Actions.ProxyBase
{
    [Serializable]
    public class ProxyValidationException : Exception
    {
        public ProxyValidationException() { }
        public ProxyValidationException(string message) : base(message) { }
        public ProxyValidationException(string message, Exception inner) : base(message, inner) { }
        protected ProxyValidationException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
