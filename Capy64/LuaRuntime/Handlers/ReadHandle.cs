using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

public class ReadHandle : IHandle
{
    private readonly StreamReader Stream;
    private bool IsClosed = false;
    public ReadHandle(Stream stream)
    {
        Stream = new StreamReader(stream);
    }

    public void Push(Lua L, bool newTable = true)
    {
        if (newTable)
            L.NewTable();

        L.PushString("readAll");
        L.PushCFunction(L_ReadAll);
        L.SetTable(-3);

        L.PushString("readLine");
        L.PushCFunction(L_ReadLine);
        L.SetTable(-3);

        L.PushString("read");
        L.PushCFunction(L_Read);
        L.SetTable(-3);

        L.PushString("close");
        L.PushCFunction(L_Close);
        L.SetTable(-3);

        L.PushString("_handle");
        L.PushObject(this);
        L.SetTable(-3);
    }

    private static int L_ReadAll(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<ReadHandle>(-1, false);

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

        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<ReadHandle>(-1, false);

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

        L.CheckType(1, LuaType.Table);
        var count = (int)L.OptNumber(2, 1);

        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<ReadHandle>(-1, false);


        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

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

        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<ReadHandle>(-1, true);

        if (h is null || h.IsClosed)
            return 0;

        h.Stream.Close();

        h.IsClosed = true;

        return 0;
    }
}
