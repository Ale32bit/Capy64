using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

public class WriteHandle
{
    private readonly StreamWriter _stream;
    private bool isClosed = false;
    public WriteHandle(Stream stream)
    {
        _stream = new StreamWriter(stream);
    }

    public WriteHandle(StreamWriter stream)
    {
        _stream = stream;
    }

    public void Push(Lua L)
    {
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
    }

    private int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            L.Error("handle is closed");

        var content = L.CheckString(1);

        _stream.Write(content);

        return 0;
    }

    private int L_WriteLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            L.Error("handle is closed");

        var content = L.CheckString(1);

        _stream.WriteLine(content);

        return 0;
    }

    private int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var count = (int)L.OptNumber(1, 1);

        L.ArgumentCheck(count < 1, 1, "count must be a positive integer");

        if (isClosed)
            L.Error("handle is closed");

        _stream.Flush();

        return 0;
    }

    private int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            return 0;

        _stream.Close();

        isClosed = true;

        return 0;
    }
}
