using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

public interface IHandle
{
    public void Push(Lua L, bool newTable = true);
}
