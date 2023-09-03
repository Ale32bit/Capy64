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
using KeraLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Capy64.Runtime;

public class LuaState : IDisposable
{
    public Lua Thread;
    public readonly Lua Parent;

    private readonly ConcurrentQueue<LuaEvent> _queue = new();

    private string[] _eventFilters = Array.Empty<string>();
    private static readonly System.Timers.Timer yieldTimeoutTimer = new(TimeSpan.FromSeconds(3));
    private static bool yieldTimedOut = false;

    public LuaState()
    {
        Parent = new Lua(false)
        {
            Encoding = Encoding.UTF8,
        };

        Sandbox.OpenLibraries(Parent);
        Sandbox.Patch(Parent);

        Thread = Parent.NewThread();

        Thread.SetHook(LH_YieldTimeout, LuaHookMask.Count, 7000);
        yieldTimeoutTimer.Elapsed += (sender, ev) =>
        {
            yieldTimedOut = true;
        };
    }

    private static void LH_YieldTimeout(IntPtr state, IntPtr ar)
    {
        var L = Lua.FromIntPtr(state);

        if (yieldTimedOut)
        {
            L.Error("no yield timeout");
        }
    }

    public void Init()
    {
        InitPlugins();

        Thread.SetTop(0);
    }

    private void InitPlugins()
    {
        var allPlugins = new List<IComponent>(Game.Instance.NativePlugins);
        allPlugins.AddRange(Game.Instance.Plugins);
        foreach (var plugin in allPlugins)
        {
            plugin.LuaInit(Thread);
        }
    }

    public void QueueEvent(string eventName, Func<Lua, int> handler)
    {
        _queue.Enqueue(new(eventName, handler));
    }

    /// <summary>
    /// Process all events in the queue.
    /// </summary>
    /// <returns>Whether the thread can still be resumed and is not finished</returns>
    public bool ProcessQueue()
    {
        var ncycle = _queue.Count;
        var done = 0;
        while (done < ncycle && Dequeue(out var npars))
        {
            if (!Resume(npars))
            {
                return false;
            }
            done++;
        }

        return true;
    }

    /// <summary>
    /// Dequeue the state with event parameters at head of queue
    /// and push values to thread
    /// </summary>
    /// <param name="npars">Amount of parameters of event</param>
    /// <returns>If an event is dequeued</returns>
    private bool Dequeue(out int npars)
    {
        npars = 0;

        if (_queue.IsEmpty)
            return false;

        if (!_queue.TryDequeue(out var ev))
            return false;

        Thread.PushString(ev.Name);
        npars = 1;

        npars += ev.Handler(Thread);

        return true;
    }

    /// <summary>
    /// Resume the Lua thread
    /// </summary>
    /// <returns>If yieldable</returns>
    /// <exception cref="LuaException"></exception>
    private bool Resume(int npars)
    {
        //Sandbox.DumpStack(Thread);
        var eventName = Thread.ToString(1);
        if (_eventFilters.Length != 0 && !_eventFilters.Contains(eventName) && eventName != "interrupt")
        {
            Thread.Pop(npars);
            return true;
        }
        yieldTimeoutTimer.Start();
        var status = Thread.Resume(null, npars, out var nresults);
        yieldTimedOut = false;
        yieldTimeoutTimer.Stop();

        // the Lua script is finished, there's nothing else to resume
        if (status == LuaStatus.OK)
            return false;

        if (status == LuaStatus.Yield)
        {
            _eventFilters = new string[nresults];
            for (var i = 1; i <= nresults; i++)
            {
                _eventFilters[i - 1] = Thread.ToString(i);
            }

            Thread.Pop(nresults);

            return true;
        }

        // something bad happened
        var error = Thread.ToString(-1);
        Thread.Traceback(Thread);
        string stacktrace = Thread.OptString(-1, null);

        var builder = new StringBuilder();
        builder.AppendLine(status switch
        {
            LuaStatus.ErrSyntax => "Syntax error",
            LuaStatus.ErrMem => "Out of memory",
            LuaStatus.ErrErr => "Error",
            LuaStatus.ErrRun => "Runtime error",
            _ => status.ToString()
        });
        builder.AppendLine(error);
        if (!string.IsNullOrWhiteSpace(stacktrace))
        {
            builder.AppendLine();
            builder.Append(stacktrace);
        }

        throw new LuaException(builder.ToString());
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _queue.Clear();
        Thread.Close();
        Parent.Close();
    }
}
