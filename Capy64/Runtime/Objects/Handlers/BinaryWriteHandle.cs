using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Capy64.Runtime.Objects.Handlers;

public class BinaryWriteHandle : IHandle
{
    private readonly BinaryWriter Stream;
    private bool IsClosed = false;
    public BinaryWriteHandle(Stream stream)
    {
        Stream = new BinaryWriter(stream, Encoding.ASCII);
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["writeByte"] = L_WriteByte,
        ["writeShort"] = L_WriteShort,
        ["writeInt"] = L_WriteInt,
        ["writeLong"] = L_WriteLong,
        ["writeSByte"] = L_WriteSByte,
        ["writeUShort"] = L_WriteUShort,
        ["writeUInt"] = L_WriteUInt,
        ["writeULong"] = L_WriteULong,
        ["writeHalf"] = L_WriteHalf,
        ["writeFloat"] = L_WriteFloat,
        ["writeDouble"] = L_WriteDouble,
        ["writeChar"] = L_WriteChar,
        ["writeString"] = L_WriteString,
        ["writeBoolean"] = L_WriteBoolean,
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

    private static BinaryWriteHandle GetHandle(Lua L, bool gc = true)
    {
        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        return L.ToObject<BinaryWriteHandle>(-1, gc);
    }

    private static int L_WriteByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        byte[] buffer;
        if (L.IsInteger(2))
        {
            buffer = new[]
            {
                (byte)L.ToInteger(2),
            };
        }
        else if (L.IsTable(2))
        {
            var len = L.RawLen(2);
            buffer = new byte[len];

            for (int i = 1; i <= len; i++)
            {
                L.PushInteger(i);
                L.GetTable(2);
                var b = L.CheckInteger(-1);
                L.Pop(1);
                buffer[i - 1] = (byte)b;
            }
        }
        else
        {
            L.ArgumentError(2, "integer or table expected, got " + L.Type(2));
            return 0;
        }

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(buffer);

        return 0;
    }

    private static int L_WriteShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((short)b);

        return 0;
    }

    private static int L_WriteInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((int)b);

        return 0;
    }

    private static int L_WriteLong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(b);

        return 0;
    }

    private static int L_WriteSByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((sbyte)b);

        return 0;
    }

    private static int L_WriteUShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((ushort)b);

        return 0;
    }

    private static int L_WriteUInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((uint)b);

        return 0;
    }

    private static int L_WriteULong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((ulong)b);

        return 0;
    }

    private static int L_WriteHalf(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((Half)b);

        return 0;
    }

    private static int L_WriteFloat(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((float)b);

        return 0;
    }

    private static int L_WriteDouble(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write((double)b);

        return 0;
    }

    private static int L_WriteChar(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        byte[] buffer;
        if (L.IsString(2))
        {
            var str = L.ToString(2);
            buffer = Encoding.ASCII.GetBytes(str);
        }
        else if (L.IsTable(2))
        {
            var len = L.RawLen(2);
            var tmpBuffer = new List<byte>();

            for (int i = 1; i <= len; i++)
            {
                L.PushInteger(i);
                L.GetTable(2);
                var b = L.CheckString(-1);
                L.Pop(1);
                var chunk = Encoding.ASCII.GetBytes(b);
                foreach (var c in chunk)
                {
                    tmpBuffer.Add(c);
                }
            }

            buffer = tmpBuffer.ToArray();
        }
        else
        {
            L.ArgumentError(2, "integer or table expected, got " + L.Type(2));
            return 0;
        }

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(buffer);

        return 0;
    }

    private static int L_WriteString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckString(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(b);

        return 0;
    }

    private static int L_WriteBoolean(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(2, LuaType.Boolean);
        var b = L.ToBoolean(2);

        var h = GetHandle(L, false);

        if (h is null || h.IsClosed)
            L.Error("handle is closed");

        h.Stream.Write(b);

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
