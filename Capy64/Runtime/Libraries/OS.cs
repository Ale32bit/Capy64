using Capy64.API;
using KeraLua;
using System;

namespace Capy64.Runtime.Libraries;

public class OS : IPlugin
{
    private static IGame _game;
    public OS(IGame game)
    {
        _game = game;
    }

    public void LuaInit(Lua state)
    {
        state.GetGlobal("os");
    }
}
