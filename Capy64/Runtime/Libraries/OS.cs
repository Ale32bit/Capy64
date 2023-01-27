﻿using Capy64.API;
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

        state.PushString("shutdown");
        state.PushCFunction(L_Shutdown);
        state.SetTable(-3);
    }

    private static int L_Shutdown(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var doReboot = false;
        if (L.IsBoolean(1))
        {
            doReboot = L.ToBoolean(1);
        }

        if (doReboot)
        {
            RuntimeManager.Reboot();
        }
        else
        {
            RuntimeManager.Shutdown();
        }

        return 0;
    }
}
