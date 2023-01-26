using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capy64.Runtime.Objects.Handlers;

public class WriteHandle
{
    public const string ObjectType = "WriteHandle";

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["write"] = L_Write,
        ["writeLine"] = L_WriteLine,
        ["flush"] = L_Flush,
        ["close"] = L_Close,
    };

    public static void Push(Lua L, StreamWriter stream)
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

    private static int L_Write(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamWriter>(1, ObjectType, false);

        var content = L.CheckString(2);

        if (handle is null)
            L.Error("handle is closed");

        handle.Write(content);

        return 0;
    }

    private static int L_WriteLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamWriter>(1, ObjectType, false);

        var content = L.CheckString(2);

        if (handle is null)
            L.Error("handle is closed");

        handle.WriteLine(content);

        return 0;
    }

    private static int L_Flush(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamWriter>(1, ObjectType, false);

        if (handle is null)
            L.Error("handle is closed");

        handle.Flush();

        return 0;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamWriter>(1, ObjectType, true);

        if (handle is null)
            return 0;

        handle.Dispose();

        return 0;
    }
}
