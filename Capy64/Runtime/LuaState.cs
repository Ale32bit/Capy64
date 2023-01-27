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
    private Lua _parent;

    private ConcurrentQueue<LuaEvent> _queue = new();

    private string[] _eventFilters = Array.Empty<string>();

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

        InitPlugins();

        Thread.SetTop(0);
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
        while (Dequeue(out var npars))
        {
            if (!Resume(npars))
            {
                return false;
            }
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

        if (_queue.Count == 0)
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

        var status = Thread.Resume(null, npars, out var nresults);

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
        builder.AppendLine(error);
        if (!string.IsNullOrWhiteSpace(stacktrace))
        {
            builder.Append(stacktrace);
        }

        throw new LuaException(builder.ToString());
    }

    public void Dispose()
    {
        _queue.Clear();
        Thread.Close();
        _parent.Close();
    }
}
