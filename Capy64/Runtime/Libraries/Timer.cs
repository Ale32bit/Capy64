using Capy64.API;
using KeraLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Capy64.Runtime.Libraries;

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
    private static uint _timerId = 0;

    private static ConcurrentDictionary<uint, System.Timers.Timer> timers = new();
    public Timer(IGame game)
    {
        _game = game;
    }

    public void LuaInit(Lua state)
    {
        _timerId = 0;

        foreach (var pair in timers)
        {
            pair.Value.Stop();
            pair.Value.Dispose();
        }

        timers.Clear();

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

        var delay = L.CheckNumber(1);
        L.ArgumentCheck(delay > 0, 1, "delay must be greater than 0");

        var timerId = _timerId++;
        var timer = new System.Timers.Timer
        {
            AutoReset = false,
            Enabled = true,
            Interval = delay,
        };

        timers[timerId] = timer;

        timer.Elapsed += (o, e) =>
        {
            timers.TryRemove(timerId, out _);

            _game.LuaRuntime.QueueEvent("timer", LK =>
            {
                LK.PushInteger(timerId);

                return 1;
            });
        };

        L.PushInteger(timerId);
        return 1;
    }
}
