using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime;

public class LuaException : Exception
{
    public LuaException()
    {
    }

    public LuaException(string message) : base(message)
    {
    }

    public LuaException(string message, Exception innerException) : base(message, innerException)
    {
    }

    protected LuaException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }
}
