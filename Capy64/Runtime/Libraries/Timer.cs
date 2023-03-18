// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Capy64.API;
using Capy64.Runtime.Objects;
using KeraLua;
using System;
using System.Collections.Concurrent;

namespace Capy64.Runtime.Libraries;

class Timer : IComponent
{
    private readonly LuaRegister[] TimerLib = new LuaRegister[]
    {
        new()
        {
            name = "start",
            function = L_StartTimer,
        },
        new()
        {
            name = "delay",
            function = L_Delay,
        },
        new()
        {
            name = "now",
            function = L_Now
        },

        new(),
    };

    private static IGame _game;
    private static uint _timerId = 0;

    private static readonly ConcurrentDictionary<uint, System.Timers.Timer> timers = new();
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
            _game.LuaRuntime.QueueEvent("timer", lk =>
            {
                lk.PushInteger(timerId);

                return 1;
            });
            timers.TryRemove(timerId, out _);
        };

        L.PushInteger(timerId);
        return 1;
    }

    private static int L_Delay(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var delay = L.CheckNumber(1);
        L.ArgumentCheck(delay > 0, 1, "delay must be greater than 0");

        var task = TaskMeta.Push(L, "timer");

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
            task.Fulfill(lk =>
            {
                lk.PushInteger(timerId);
            });
            timers.TryRemove(timerId, out _);
        };

        return 1;
    }

    private static int L_Now(IntPtr state)
    {
        var now = DateTimeOffset.UtcNow;
        var L = Lua.FromIntPtr(state);

        L.PushInteger(now.ToUnixTimeMilliseconds());

        return 1;
    }
}
