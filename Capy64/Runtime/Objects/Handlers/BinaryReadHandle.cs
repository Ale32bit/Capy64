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
        ["readByte"] = L_ReadByte,
        ["readShort"] = L_ReadShort,
        ["readInt"] = L_ReadInt,
        ["readLong"] = L_ReadLong,
        ["readSByte"] = L_ReadSByte,
        ["readUShort"] = L_ReadUShort,
        ["readUInt"] = L_ReadUInt,
        ["readULong"] = L_ReadULong,
        ["readHalf"] = L_ReadHalf,
        ["readFloat"] = L_ReadFloat,
        ["readDouble"] = L_ReadDouble,
        ["readChar"] = L_ReadChar,
        ["readString"] = L_ReadString,
        ["readBoolean"] = L_ReadBoolean,
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

    private static int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = L.CheckObject<BinaryReader>(1, ObjectType, false);

        if (stream is null)
            L.Error("handle is closed");

        if (L.IsInteger(2))
        {
            if (stream.BaseStream.Position >= stream.BaseStream.Length)
            {
                L.PushNil();
                return 0;
            }
            stream.ReadChars((int)L.ToInteger(2));
        }
        else if (L.IsString(2))
        {
            var option = L.ToString(2);
            option = option.TrimStart('*');
            if (option.Length == 0)
            {
                L.ArgumentError(2, "invalid option");
                return 0;
            }

            if (stream.BaseStream.Position >= stream.BaseStream.Length)
            {
                L.PushNil();
                return 0;
            }

            var reader = new StreamReader(stream.BaseStream);

            switch (option[0])
            {
                case 'a':
                    L.PushString(reader.ReadToEnd());
                    break;
                case 'l':
                    L.PushString(reader.ReadLine());
                    break;
                case 'n':
                    L.Error("Not yet implemented!");
                    break;
                default:
                    L.ArgumentError(2, "invalid option");
                    break;
            }
        }
        else
        {
            L.ArgumentError(2, "number or string expected");
            return 0;
        }

        return 1;
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

    private static int L_ReadByte(IntPtr state)
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

    private static int L_ReadShort(IntPtr state)
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

    private static int L_ReadInt(IntPtr state)
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

    private static int L_ReadLong(IntPtr state)
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

    private static int L_ReadSByte(IntPtr state)
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

    private static int L_ReadUShort(IntPtr state)
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

    private static int L_ReadUInt(IntPtr state)
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

    private static int L_ReadULong(IntPtr state)
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

    private static int L_ReadHalf(IntPtr state)
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

    private static int L_ReadFloat(IntPtr state)
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

    private static int L_ReadDouble(IntPtr state)
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

    private static int L_ReadChar(IntPtr state)
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

    private static int L_ReadString(IntPtr state)
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

    private static int L_ReadBoolean(IntPtr state)
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
