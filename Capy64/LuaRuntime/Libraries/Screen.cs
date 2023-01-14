using Capy64.API;
using KeraLua;
using System;

namespace Capy64.LuaRuntime.Libraries;

public class Screen : IPlugin
{
    private static IGame _game;
    public Screen(IGame game)
    {
        _game = game;
    }

    private LuaRegister[] ScreenLib = new LuaRegister[] {
        new()
        {
            name = "getSize",
            function = L_GetSize,
        },
        new()
        {
            name = "setSize",
            function = L_SetSize,
        },
        new()
        {
            name = "getScale",
            function = L_GetScale,
        },
        new()
        {
            name = "setScale",
            function = L_SetScale,
        },
        new(), // NULL
    };

    public void LuaInit(Lua state)
    {
        state.RequireF("screen", Open, false);
    }

    public int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(ScreenLib);
        return 1;
    }

    private static int L_GetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(_game.Width);
        L.PushInteger(_game.Height);

        return 2;
    }

    private static int L_SetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var w = L.CheckInteger(1);
        var h = L.CheckInteger(2);

        _game.Width = (int)w;
        _game.Height = (int)h;

        _game.UpdateSize();

        return 0;
    }

    private static int L_GetScale(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushNumber(_game.Scale);

        return 1;
    }

    private static int L_SetScale(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var s = L.CheckNumber(1);

        _game.Scale = (float)s;

        _game.UpdateSize();

        return 0;
    }

}
