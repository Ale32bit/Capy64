using Capy64.API;
using KeraLua;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class LuaState : IDisposable
{
    public Lua Thread;
    private readonly Lua _parent;

    private readonly ConcurrentQueue<LuaEvent> _queue = new();

    private string[] _eventFilters = Array.Empty<string>();
    private static readonly System.Timers.Timer yieldTimeoutTimer = new(TimeSpan.FromSeconds(3));
    private static bool yieldTimedOut = false;

    public LuaState()
    {
        _parent = new Lua(false)
        {
            Encoding = Encoding.UTF8,
        };

        _parent.PushString("Capy64 " + Capy64.Version);
        _parent.SetGlobal("_HOST");

        Sandbox.OpenLibraries(_parent);
        Sandbox.Patch(_parent);

        Thread = _parent.NewThread();

        Thread.SetHook(LH_YieldTimeout, LuaHookMask.Count, 7000);
        yieldTimeoutTimer.Elapsed += (sender, ev) =>
        {
            yieldTimedOut = true;
        };

        InitPlugins();

        Thread.SetTop(0);
    }

    private static void LH_YieldTimeout(IntPtr state, IntPtr ar)
    {
        var L = Lua.FromIntPtr(state);

        if (yieldTimedOut)
        {
            L.Error("no yield timeout");
            Console.WriteLine("tick");
        }
    }

    private void InitPlugins()
    {
        var allPlugins = new List<IPlugin>(Capy64.Instance.NativePlugins);
        allPlugins.AddRange(Capy64.Instance.Plugins);
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
        _parent.Close();
    }
}
