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

class TimerLib : IComponent
{
    public class Timer
    {
        public int RemainingTicks = 0;
        public TaskMeta.RuntimeTask Task = null!;
    }

    private readonly LuaRegister[] Library = new LuaRegister[]
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

    private static Capy64 _game;
    private static uint _timerId = 0;

    private static readonly ConcurrentDictionary<uint, Timer> timers = new();
    public TimerLib(Capy64 game)
    {
        _game = game;

        _game.EventEmitter.OnTick += OnTick;
    }

    private void OnTick(object sender, Eventing.Events.TickEvent e)
    {
        if (e.IsActiveTick)
        {
            foreach (var t in timers)
            {
                var timer = t.Value;
                timer.RemainingTicks--;
                if (timer.RemainingTicks <= 0)
                {
                    if (timer.Task == null)
                    {
                        _game.LuaRuntime.QueueEvent("timer", lk =>
                        {
                            lk.PushInteger(t.Key);

                            return 1;
                        });
                    }
                    else
                    {
                        timer.Task.Fulfill(lk =>
                        {
                            lk.PushInteger(t.Key);
                        });
                    }

                    timers.TryRemove(t.Key, out _);
                }
            }
        }
    }

    public void LuaInit(Lua state)
    {
        _timerId = 0;

        timers.Clear();

        state.RequireF("timer", Open, false);
    }

    private int Open(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(Library);
        return 1;
    }



    private static int L_StartTimer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var delay = L.CheckNumber(1);

        var timerId = _timerId++;

        timers[timerId] = new Timer
        {
            RemainingTicks = (int)(delay * Capy64.Instance.TickRate)
        };

        L.PushInteger(timerId);
        return 1;
    }

    private static int L_Delay(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var delay = L.CheckNumber(1);

        var task = TaskMeta.Push(L, "timer");

        var timerId = _timerId++;

        timers[timerId] = new Timer
        {
            RemainingTicks = (int)(delay * Capy64.Instance.TickRate),
            Task = task,
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
