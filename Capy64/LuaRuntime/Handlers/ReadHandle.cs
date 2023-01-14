using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capy64.LuaRuntime.Handlers;

public class ReadHandle : IHandle
{
    private readonly StreamReader Stream;
    private bool IsClosed = false;
    public ReadHandle(Stream stream)
    {
        Stream = new StreamReader(stream);
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["readAll"] = L_ReadAll,
        ["readLine"] = L_ReadLine,
        ["read"] = L_Read,
        ["close"] = L_Close,
    };

    public void Push(Lua L, bool newTable = true)
    {
        if (newTable)
            L.NewTable();

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

    private static ReadHandle GetHandle(Lua L, bool gc = true)
    {
        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        return L.ToObject<ReadHandle>(-1, gc);
    }

    private static int L_ReadAll(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.Stream.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var content = h.Stream.ReadToEnd();
        L.PushString(content);

        return 1;
    }

    private static int L_ReadLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        var line = h.Stream.ReadLine();

        if (line is null)
            L.PushNil();
        else
            L.PushString(line);

        return 1;
    }

    private static int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.Stream.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var chunk = new char[count];

        h.Stream.Read(chunk, 0, count);

        L.PushString(new string(chunk));

        return 1;
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
