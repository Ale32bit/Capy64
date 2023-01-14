using Capy64.API;
using KeraLua;
using System;

namespace Capy64.LuaRuntime.Libraries;

class Timer : IPlugin
{
    private LuaRegister[] TimerLib = new LuaRegister[]
    {
        new()
        {
            name = "start",
            function = L_StartTimer,
        },

        new(),
    };

    private static IGame _game;
    private static uint _timerId;
    public Timer(IGame game)
    {
        _game = game;
        _timerId = 0;
    }

    public void LuaInit(Lua state)
    {
        state.RequireF("timer", Open, false);
    }

    private int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(TimerLib);
        return 1;
    }

    private static int L_StartTimer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var delay = L.CheckInteger(1);
        L.ArgumentCheck(delay > 0, 1, "delay must be greater than 0");

        var timerId = _timerId++;
        var timer = new System.Timers.Timer
        {
            AutoReset = false,
            Enabled = true,
            Interval = delay,
        };

        timer.Elapsed += (o, e) =>
        {
            _game.LuaRuntime.PushEvent("timer", timerId);
        };

        L.PushInteger(timerId);
        return 1;
    }

    private static int L_Sleep(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);



        return 0;
    }
}
