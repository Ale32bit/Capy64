using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime;

public class LuaDelegateEvent : ILuaEvent
{
    public string Name { get; set; }
    public Func<Lua, int> Handler { get; set; }
    public bool BypassFilter { get; set; } = false;
}
