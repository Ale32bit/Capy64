using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class LuaEvent
{
    public LuaEvent(string name, Func<Lua, int> handler)
    {
        Name = name;
        Handler = handler;
    }

    public string Name { get; set; }
    public Func<Lua, int> Handler { get; set; }
}
