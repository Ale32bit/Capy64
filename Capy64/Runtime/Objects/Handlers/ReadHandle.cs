using KeraLua;
using System;
using System.Collections.Generic;
using System.IO;

namespace Capy64.Runtime.Objects.Handlers;

public class ReadHandle
{
    public const string ObjectType = "ReadHandle";

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["readAll"] = L_ReadAll,
        ["readLine"] = L_ReadLine,
        ["read"] = L_Read,
        ["close"] = L_Close,
    };

    public static void Push(Lua L, StreamReader stream)
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

        var handle = L.CheckObject<StreamReader>(1, ObjectType, false);

        if (handle is null)
            L.Error("handle is closed");

        if (handle.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var content = handle.ReadToEnd();
        L.PushString(content);

        return 1;
    }

    private static int L_ReadLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamReader>(1, ObjectType, false);

        if (handle is null)
            L.Error("handle is closed");

        var line = handle.ReadLine();

        if (line is null)
            L.PushNil();
        else
            L.PushString(line);

        return 1;
    }

    private static int L_Read(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var count = (int)L.OptNumber(2, 1);
        L.ArgumentCheck(count >= 1, 2, "count must be a positive integer");

        var handle = L.CheckObject<StreamReader>(1, ObjectType, false);

        if (handle is null)
            L.Error("handle is closed");

        if (handle.EndOfStream)
        {
            L.PushNil();
            return 1;
        }

        var chunk = new char[count];

        handle.Read(chunk, 0, count);

        L.PushString(new string(chunk));

        return 1;
    }

    private static int L_Close(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var handle = L.CheckObject<StreamReader>(1, ObjectType, true);

        if (handle is null)
            return 0;

        handle.Dispose();

        return 0;
    }
}
