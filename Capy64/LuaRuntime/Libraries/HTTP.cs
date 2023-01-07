using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Libraries;

public class HTTP : IPlugin
{
    private static IGame _game;
    public HTTP(IGame game)
    {
        _game = game;
    }

    public void LuaInit(Lua state)
    {
    }
}
