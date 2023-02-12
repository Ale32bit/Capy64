using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime.Objects;

public class FileHandle : IPlugin
{
    public const string ObjectType = "file";

    private static LuaRegister[] Methods = new LuaRegister[]
    {
        new()
        {
            name = "read",
            function = L_Read,
        },
        new()
        {
            name = "write",
            function = L_Write,
        },
        new()
        {
            name = "lines",
            function = L_Lines,
        },
        new()
        {
            name = "flush",
            function = L_Flush,
        },
        new()
        {
            name = "seek",
            function = L_Seek,
        },
        new()
        {
            name = "close",
            function = L_Close,
        },
        new(),
    };

    private static LuaRegister[] MetaMethods = new LuaRegister[]
    {
        new()
        {
            name = "__index",
        },
        new()
        {
            name = "__gc",
            function = LM_GC,
        },
        new()
        {
            name = "__close",
            function = LM_GC,
        },
        new()
        {
            name = "__tostring",
            function = LM_ToString,
        },

        new(),
    };

    public void LuaInit(Lua L)
    {
        CreateMeta(L);
    }

    public static void CreateMeta(Lua L)
    {
        L.NewMetaTable(ObjectType);
        L.SetFuncs(MetaMethods, 0);
        L.NewLibTable(Methods);
        L.SetFuncs(Methods, 0);
        L.SetField(-2, "__index");
        L.Pop(1);
    }

    private static char CheckMode(string mode)
    {
        var modes = "nlLa";
        mode = mode.TrimStart('*');
        if(string.IsNullOrEmpty(mode)) {
            return '\0';
        }

        var i = modes.IndexOf(mode[0]);
        if(i == -1)
            return '\0';

        return modes[i];
    }

    private static Stream ToStream(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<Stream>(L, 1, gc);
    }

    private static Stream CheckStream(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<Stream>(L, 1, ObjectType, gc);
        if (obj is null)
        {
            L.Error("attempt to use a closed file");
            return null;
        }
        return obj;
    }

    private static bool ReadNumber(Lua L, Stream stream)
    {
        return false;
    }

    private static bool ReadLine(Lua L, Stream stream, bool chop)
    {
        var buffer = new byte[stream.Length];
        int i = 0;
        int c = 0;
        for (; i < stream.Length; i++)
        {
            c = stream.ReadByte();
            if (c == -1 || c == '\n')
                break;
            buffer[i] = (byte)c;
        }

        if (!chop && c == '\n')
            buffer[i++] = (byte)c;

        Array.Resize(ref buffer, i);
        L.PushBuffer(buffer);

        return c == '\n' || L.RawLen(-1) > 0;
    }

    private static void ReadAll(Lua L, Stream stream)
    {
        var buffer = new byte[stream.Length];
        var read = stream.Read(buffer, 0, buffer.Length);
        Array.Resize(ref buffer, read);
        L.PushBuffer(buffer);
    }

    private static bool ReadChars(Lua L, Stream stream, int n)
    {
        var buffer = new byte[n];
        var read = stream.Read(buffer, 0, n);
        Array.Resize(ref buffer, read);
        L.PushBuffer(buffer);
        return read != 0;
    }

    private static int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var stream = CheckStream(L);

        if (!stream.CanRead)
        {
            L.PushNil();
            L.PushString("Attempt to read from a write-only stream");
            return 2;
        }

        var nargs = L.GetTop() - 1;
        if (nargs == 0)
        {
            L.PushString("l");
        }

        bool success;
        if (L.Type(2) == LuaType.Number)
        {
            success = ReadChars(L, stream, (int)L.ToNumber(2));
        }
        else
        {
            var p = L.CheckString(2);
            var mode = CheckMode(p);
            switch (mode)
            {
                case 'n':
                    success = ReadNumber(L, stream);
                    break;
                case 'l':
                    success = ReadLine(L, stream, true);
                    break;
                case 'L':
                    success = ReadLine(L, stream, false);
                    break;
                case 'a':
                    ReadAll(L, stream);
                    success = true;
                    break;
                default:
                    return L.ArgumentError(2, "invalid format");
            }

        }

        if (!success)
        {
            L.Pop(1);
            L.PushNil();
        }

        return 1;
    }

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var nargs = L.GetTop();

        var stream = CheckStream(L);
        if (!stream.CanWrite)
        {
            L.PushNil();
            L.PushString("Attempt to write to a read-only stream");
            return 2;
        }

        for (int arg = 2; arg <= nargs; arg++)
        {
            if (L.Type(arg) == LuaType.Number)
            {
                stream.WriteByte((byte)L.ToNumber(arg));
            }
            else
            {
                var buffer = L.CheckBuffer(arg);
                stream.Write(buffer);
            }
        }

        return L.FileResult(1, null);
    }

    private static int L_Lines(IntPtr state)
    {
        return 0;
        var L = Lua.FromIntPtr(state);
        var maxargn = 250;

        var n = L.GetTop() - 1;
        var stream = CheckStream(L, false);
        L.ArgumentCheck(n <= maxargn, maxargn + 2, "too many arguments");
        L.PushCopy(1);  
        L.PushInteger(n);
        L.PushBoolean(false);
        L.Rotate(2, 3);
        L.PushCClosure(null, 3 + n); // todo

        return 1;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, false);

        stream.Flush();

        return 0;
    }

    private static int L_Seek(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, false);

        var whence = L.CheckOption(2, "cur", new[]
        {
            "set", // begin 0
            "cur", // current 1
            "end", // end 2
            null,
        });

        var offset = L.OptInteger(3, 0);

        var newPosition = stream.Seek(offset, (SeekOrigin)whence);

        L.PushInteger(newPosition);

        return 1;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = CheckStream(L, true);
        stream.Close();

        return 0;
    }

    private static unsafe int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var stream = ToStream(L);
        if (stream is not null)
        {
            L.PushString("file ({0:X})", (ulong)&stream);
        }
        else
        {
            L.PushString("file (closed)");
        }
        return 1;
    }

    private static int LM_GC(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var stream = ToStream(L, true);
        if (stream is not null)
            stream.Close();

        return 0;
    }
}
