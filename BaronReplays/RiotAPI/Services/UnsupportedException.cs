using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaronReplays.RiotAPI.Services
{
    [Serializable]
    public class UnsupportedException : Exception
    {
        public UnsupportedException() { }
        public UnsupportedException(string message) : base(message) { }
        public UnsupportedException(string message, Exception inner) : base(message, inner) { }
        protected UnsupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
