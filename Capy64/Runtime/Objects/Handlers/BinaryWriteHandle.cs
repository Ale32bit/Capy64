using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Capy64.Runtime.Objects.Handlers;

public class BinaryWriteHandle
{
    public const string ObjectType = "BinaryWriteHandle";

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

    public static void Push(Lua L, BinaryWriter stream)
    {
        L.PushObject(stream);

        if (L.NewMetaTable(ObjectType))
        {
            L.PushString("__index");
            L.NewTable();
            foreach (var pair in functions)
            {
                L.PushString(pair.Key);
                L.PushCFunction(pair.Value);
                L.SetTable(-3);
            }
            L.SetTable(-3);

            L.PushString("__close");
            L.PushCFunction(L_Close);
            L.SetTable(-3);
            L.PushString("__gc");
            L.PushCFunction(L_Close);
            L.SetTable(-3);
        }

        L.SetMetaTable(-2);
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

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write(buffer);

        return 0;
    }

    private static int L_WriteShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");
        
        stream.Write((short)b);

        return 0;
    }

    private static int L_WriteInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((int)b);

        return 0;
    }

    private static int L_WriteLong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write(b);

        return 0;
    }

    private static int L_WriteSByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((sbyte)b);

        return 0;
    }

    private static int L_WriteUShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((ushort)b);

        return 0;
    }

    private static int L_WriteUInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((uint)b);

        return 0;
    }

    private static int L_WriteULong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckInteger(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((ulong)b);

        return 0;
    }

    private static int L_WriteHalf(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((Half)b);

        return 0;
    }

    private static int L_WriteFloat(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((float)b);

        return 0;
    }

    private static int L_WriteDouble(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckNumber(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write((double)b);

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

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write(buffer);

        return 0;
    }

    private static int L_WriteString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var b = L.CheckString(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write(b);

        return 0;
    }

    private static int L_WriteBoolean(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(2, LuaType.Boolean);
        var b = L.ToBoolean(2);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Write(b);

        return 0;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        stream.Flush();

        return 0;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryWriter>(1, ObjectType, true);

        if (stream is null)
            L.Error("handle is closed");

        stream.Dispose();

        return 0;
    }
}
