using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

[Obsolete("Work in progress")]
public class BinaryReadHandle
{
    private readonly BinaryReader _stream;
    private bool isClosed = false;
    private bool ended => _stream.BaseStream.Position == _stream.BaseStream.Length;
    public BinaryReadHandle(Stream stream)
    {
        _stream = new BinaryReader(stream);
    }

    public BinaryReadHandle(BinaryReader stream)
    {
        _stream = stream;
    }

    /*public void Push(Lua L)
    {
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
    }

    private int L_ReadAll(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            L.Error("handle is closed");

        if (ended)
        {
            L.PushNil();
            return 1;
        }

        //var content = _stream.read;
        L.PushString(content);

        return 1;
    }

    private int L_ReadLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            L.Error("handle is closed");

        if (ended)
        {
            L.PushNil();
            return 1;
        }

        var line = _stream.ReadLine();

        if (line is null)
            L.PushNil();
        else
            L.PushString(line);

        return 1;
    }

    private int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var count = (int)L.OptNumber(1, 1);

        L.ArgumentCheck(count < 1, 1, "count must be a positive integer");

        if (ended)
            L.Error("handle is closed");

        if (_stream.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var chunk = new char[count];

        _stream.Read(chunk, 0, count);

        L.PushString(new string(chunk));

        return 1;
    }

    private int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            return 0;

        _stream.Close();

        isClosed = true;

        return 0;
    }*/
}
