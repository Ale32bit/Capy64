using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Capy64.Runtime.Objects.Handlers;

public class BinaryReadHandle
{
    public const string ObjectType = "BinaryReadHandle";

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["readAll"] = L_ReadAll,
        ["seek"] = L_Seek,
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

    public static void Push(Lua L, BinaryReader stream)
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

    private static int L_ReadAll(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position >= stream.BaseStream.Length)
        {
            L.PushNil();
            return 0;
        }

        var chars = stream.ReadChars((int)stream.BaseStream.Length);
        var buffer = Encoding.ASCII.GetBytes(chars);

        L.PushBuffer(buffer);
        return 1;
    }

    private static int L_Seek(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        var whence = L.CheckOption(2, "cur", new[]
        {
            "set", // begin 0
            "cur", // current 1
            "end", // end 2
            null,
        });

        var offset = L.OptInteger(3, 0);

        var newPosition = stream.BaseStream.Seek(offset, (SeekOrigin)whence);

        L.PushInteger(newPosition);

        return 1;
    }

    private static int L_NextByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position >= stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        if (count == 1)
        {
            var b = stream.ReadByte();
            L.PushInteger(b);
            return 1;

        }
        else
        {
            var bs = stream.ReadBytes(count);
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

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position >= stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadInt16();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadInt32();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextLong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadInt64();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextSByte(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadSByte();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextUShort(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadUInt16();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextUInt(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadUInt32();
        L.PushInteger(b);
        return 1;
    }

    private static int L_NextULong(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadUInt64();
        L.PushInteger((long)b);
        return 1;
    }

    private static int L_NextHalf(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadHalf();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextFloat(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadSingle();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextDouble(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadDouble();
        L.PushNumber((double)b);
        return 1;
    }

    private static int L_NextChar(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position >= stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        if (count == 1)
        {
            var b = stream.ReadChar();
            L.PushString(b.ToString());
            return 1;

        }
        else
        {
            var bs = stream.ReadChars(count);
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

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position >= stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        if (count == 1)
        {
            var b = stream.ReadChar();
            L.PushString(b.ToString());
            return 1;

        }
        else
        {
            var bs = stream.ReadBytes(count);
            L.PushBuffer(bs);
            return 1;
        }
    }

    private static int L_NextBoolean(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (stream.BaseStream.Position > stream.BaseStream.Length)
        {
            L.PushNil();
            return 1;
        }

        var b = stream.ReadBoolean();
        L.PushBoolean(b);
        return 1;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, true);

        if (stream is null)
            return 0;

        stream.Dispose();

        return 0;
    }
}
