using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Capy64.LuaRuntime.Handlers;

public class BinaryReadHandle : IHandle
{
    private readonly BinaryReader Stream;
    private bool IsClosed = false;
    private bool EndOfStream => Stream.BaseStream.Position == Stream.BaseStream.Length;
    public BinaryReadHandle(Stream stream)
    {
        Stream = new BinaryReader(stream, Encoding.ASCII);
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["nextByte"] = L_NextByte,
        ["nextShort"] = L_NextShort,
        ["nextInt"] = L_NextInt,
        ["nextLong"] = L_NextLong,
        ["nextSByte"] = L_NextSByte,
        ["nextUShort"] = L_NextUShort,
        ["nextUInt"] = L_NextUInt,
        ["nextULong"] = L_NextULong,
        ["nextHalf"] = L_NextHalf,
        ["nextFloat"] = L_NextFloat,
        ["nextDouble"] = L_NextDouble,
        ["nextChar"] = L_NextChar,
        ["nextString"] = L_NextString,
        ["nextBoolean"] = L_NextBoolean,
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

    private static BinaryReadHandle GetHandle(Lua L, bool gc = true)
    {
        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        return L.ToObject<BinaryReadHandle>(-1, gc);
    }

    private static int L_NextByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        if (count == 1)
        {
            var b = h.Stream.ReadByte();
            L.PushInteger(b);
            return 1;

        }
        else
        {
            var bs = h.Stream.ReadBytes(count);
            foreach (var b in bs)
            {
                L.PushInteger(b);
            }
            return bs.Length;
        }
    }

    private static int L_NextShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadInt16();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadInt32();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextLong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadInt64();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextSByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadSByte();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextUShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadUInt16();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextUInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadUInt32();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextULong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadUInt64();
        L.PushInteger((long)b);
        return 1;
    }

    private static int L_NextHalf(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadHalf();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextFloat(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadSingle();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextDouble(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadDouble();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextChar(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        if (count == 1)
        {
            var b = h.Stream.ReadChar();
            L.PushString(b.ToString());
            return 1;

        }
        else
        {
            var bs = h.Stream.ReadChars(count);
            foreach (var b in bs)
            {
                L.PushString(b.ToString());
            }
            return bs.Length;
        }
    }

    private static int L_NextString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        if (count == 1)
        {
            var b = h.Stream.ReadChar();
            L.PushString(b.ToString());
            return 1;

        }
        else
        {
            var bs = h.Stream.ReadBytes(count);
            L.PushBuffer(bs);
            return 1;
        }
    }

    private static int L_NextBoolean(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        if (h.EndOfStream)
            return 0;

        var b = h.Stream.ReadBoolean();
        L.PushBoolean(b);
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
