using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime.Libraries;

public class Event : IPlugin
{
    private static IGame _game;
    public Event(IGame game)
    {
        _game = game;
    }

    private static LuaRegister[] EventLib = new LuaRegister[]
    {
        new()
        {
            name = "pull",
            function = L_Pull,
        },
        new()
        {
            name = "pullRaw",
            function = L_PullRaw
        },
        new()
        {
            name = "push",
            function = L_Push,
        },
        new(),
    };

    public void LuaInit(Lua L)
    {
        L.RequireF("event", OpenLib, false);
    }

    private static int OpenLib(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(EventLib);
        return 1;
    }

    private static int LK_Pull(IntPtr state, int status, IntPtr ctx)
    {
        var L = Lua.FromIntPtr(state);

        if (L.ToString(1) == "interrupt")
        {
            L.Error("interrupt");
        }

        var nargs = L.GetTop();

        return nargs;
    }

    private static int L_Pull(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();
        for (int i = 1; i <= nargs; i++)
        {
            L.CheckString(i);
        }

        L.YieldK(nargs, 0, LK_Pull);

        return 0;
    }

    private static int L_PullRaw(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();
        for (int i = 1; i <= nargs; i++)
        {
            L.CheckString(i);
        }

        L.Yield(nargs);

        return 0;
    }

    private static int L_Push(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var eventName = L.CheckString(1);

        var nargs = L.GetTop();

        _game.LuaRuntime.QueueEvent(eventName, LK =>
        {
            for (int i = 2; i <= nargs; i++)
            {
                L.PushCopy(i);
            }

            L.XMove(LK, nargs - 1);

            return nargs - 1;
        });

        return 0;
    }
}
