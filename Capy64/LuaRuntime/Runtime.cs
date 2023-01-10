using Capy64.LuaRuntime.Extensions;
using Capy64.LuaRuntime.Libraries;
using KeraLua;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace Capy64.LuaRuntime;

public class Runtime
{
    private readonly ConcurrentQueue<ILuaEvent> eventQueue = new(new LuaEvent[]
    {
        new()
        {
            Name = "init",
            Parameters = Array.Empty<object>()
        }
    });

    private readonly Lua Parent;
    public Lua Thread { get; private set; }
    private string[] filters = Array.Empty<string>();
    private bool Disposing = false;
    public Runtime()
    {
        Parent = new(false)
        {
            Encoding = Encoding.UTF8,
        };

        Sandbox.OpenLibraries(Parent);

        Thread = Parent.NewThread();

    }

    public void Patch()
    {
        Sandbox.Patch(Parent);
    }

    public void Init()
    {
        Parent.SetTop(0);
        var initContent = File.ReadAllText(Path.Combine(FileSystem.DataPath, "init.lua"));
        var status = Thread.LoadString(initContent, "=init.lua");
        if (status != LuaStatus.OK)
        {
            throw new LuaException(Thread.ToString(-1));
        }
    }

    public void PushEvent(LuaEvent ev)
    {
        eventQueue.Enqueue(ev);
    }

    public void PushEvent(string name, params object[] pars)
    {
        eventQueue.Enqueue(new LuaEvent() { Name = name, Parameters = pars });
    }

    public void PushEvent(LuaDelegateEvent ev)
    {
        eventQueue.Enqueue(ev);
    }

    /// <summary>
    /// Push a new event to the event queue.
    /// </summary>
    /// <param name="name">Event name</param>
    /// <param name="handler">Event handler. Push any needed parameter from here. Return n as amount of parameters pushed, minus event name.
    /// For example: I push an event with 3 parameters, then I return 3. 
    /// </param>
    public void PushEvent(string name, Func<Lua, int> handler)
    {
        eventQueue.Enqueue(new LuaDelegateEvent {
            Name = name,
            Handler = handler
        });
    }

    /// <summary>
    /// Resume the Lua thread
    /// </summary>
    /// <returns>Whether it yielded</returns>
    public bool Resume()
    {
        while (eventQueue.TryDequeue(out ILuaEvent ev))
        {
            if (!ResumeThread(ev))
                return false;
        }

        return true;
    }

    private bool ResumeThread(ILuaEvent ev)
    {
        

        if (filters.Length > 0 && !filters.Contains(ev.Name))
        {
            if (!ev.BypassFilter)
            {
                return true;
            }
        }

        filters = Array.Empty<string>();
        var evpars = PushEventToStack(ev);
        if (Disposing)
            return false;
        var status = Thread.Resume(null, evpars, out int pars);
        if (status is LuaStatus.OK or LuaStatus.Yield)
        {
            if(Disposing)
                return false;
            filters = new string[pars];
            for (int i = 0; i < pars; i++)
            {
                filters[i] = Thread.OptString(i + 1, null);
            }
            Thread.Pop(pars);
            return status == LuaStatus.Yield;
        }

        var error = Thread.OptString(-1, "Unknown exception");
        Thread.Traceback(Thread);
        var stacktrace = Thread.OptString(-1, "");

        throw new LuaException($"Top thread exception:\n{error}\n{stacktrace}");
    }

    private int PushEventToStack(ILuaEvent ev)
    {
        Thread.PushString(ev.Name);

        switch(ev)
        {
            case LuaEvent e:

                if (e.Parameters != null)
                {
                    foreach (var par in e.Parameters)
                    {
                        Thread.PushValue(par);
                    }
                }

                return (e.Parameters?.Length ?? 0) + 1;

            case LuaDelegateEvent e:

                int n = e.Handler(Thread);
                return n + 1;

            default:
                throw new NotImplementedException();
        }
    }

    public void Close()
    {
        Disposing = true;
        Parent.Close();
    }
}
