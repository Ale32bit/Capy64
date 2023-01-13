using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

public class ReadHandle : IDisposable
{
    private readonly MemoryStream _memoryStream;
    private readonly StreamReader _stream;
    private bool isClosed = false;
    public ReadHandle(Stream stream)
    {
        _memoryStream = new MemoryStream(capacity: (int)stream.Length);
        stream.CopyTo(_memoryStream);
        _memoryStream.Position = 0;
        _stream = new StreamReader(_memoryStream);
    }

    public void Push(Lua L)
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

        if (_stream.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var content = _stream.ReadToEnd();
        L.PushString(content);

        return 1;
    }

    private int L_ReadLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (isClosed)
            L.Error("handle is closed");

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

        L.ArgumentCheck(count >= 1, 1, "count must be a positive integer");

        if (isClosed)
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
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
