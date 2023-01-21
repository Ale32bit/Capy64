using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capy64.LuaRuntime.Objects.Handlers;

public class WriteHandle : IHandle
{
    private readonly StreamWriter Stream;
    private bool IsClosed = false;
    public WriteHandle(Stream stream)
    {
        Stream = new StreamWriter(stream);
    }

    public WriteHandle(StreamWriter stream)
    {
        Stream = stream;
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["write"] = L_Write,
        ["writeLine"] = L_WriteLine,
        ["flush"] = L_Flush,
        ["close"] = L_Close,
    };

    public void Push(Lua L, bool newTable = true)
    {
        if (newTable)
            L.NewTable();

        // metatable
        L.NewTable();
        L.PushString("__close");
        L.PushCFunction(L_Close);
        L.SetTable(-3);
        L.PushString("__gc");
        L.PushCFunction(L_Close);
        L.SetTable(-3);
        L.SetMetaTable(-2);

        foreach (var pair in functions)
        {
            L.PushString(pair.Key);
            L.PushCFunction(pair.Value);
            L.SetTable(-3);
        }

        L.PushString("_handle");
        L.PushObject(this);
        L.SetTable(-3);
    }

    private static WriteHandle GetHandle(Lua L, bool gc = true)
    {
        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        return L.ToObject<WriteHandle>(-1, gc);
    }

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var content = L.CheckString(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(content);

        return 0;
    }

    private static int L_WriteLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var content = L.CheckString(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.WriteLine(content);

        return 0;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Flush();

        return 0;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, true);

        if (h is null || h.IsClosed)
            return 0;

        h.Stream.Close();

        h.IsClosed = true;

        return 0;
    }
}
