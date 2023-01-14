using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

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

    public void Push(Lua L, bool newTable = true)
    {
        if (newTable)
            L.NewTable();

        L.PushString("write");
        L.PushCFunction(L_Write);
        L.SetTable(-3);

        L.PushString("writeLine");
        L.PushCFunction(L_WriteLine);
        L.SetTable(-3);

        L.PushString("flush");
        L.PushCFunction(L_Flush);
        L.SetTable(-3);

        L.PushString("close");
        L.PushCFunction(L_Close);
        L.SetTable(-3);

        L.PushString("_handle");
        L.PushObject(this);
        L.SetTable(-3);
    }

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        var content = L.CheckString(2);

        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<WriteHandle>(-1, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");


        h.Stream.Write(content);

        return 0;
    }

    private static int L_WriteLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        var content = L.CheckString(2);

        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<WriteHandle>(-1, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");


        h.Stream.WriteLine(content);

        return 0;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<WriteHandle>(-1, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Flush();

        return 0;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        var h = L.ToObject<WriteHandle>(-1, true);

        if (h is null || h.IsClosed)
            return 0;

        h.Stream.Close();

        h.IsClosed = true;

        return 0;
    }
}
