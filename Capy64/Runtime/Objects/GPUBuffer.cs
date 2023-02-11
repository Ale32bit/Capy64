using Capy64.API;
using KeraLua;
using System;
using System.IO;

namespace Capy64.Runtime.Objects;

public class GPUBuffer : IPlugin
{
    public const string ObjectType = "GPUBuffer";

    private static LuaRegister[] MetaMethods = new LuaRegister[]
    {
        new()
        {
            name = "__index",
            function = LM_Index,
        },
        new()
        {
            name = "__newindex",
            function = LM_NewIndex,
        },
        new()
        {
            name = "__len",
            function = LM_Length,
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
    }

    public static uint[] ToBuffer(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<uint[]>(L, 1, gc);
        //return L.CheckObject<uint[]>(1, ObjectType, gc);
    }

    public static uint[] CheckBuffer(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<uint[]>(L, 1, ObjectType, gc);
        //var obj = L.CheckObject<uint[]>(1, ObjectType, gc);
        if (obj is null)
        {
            L.Error("attempt to use a closed buffer");
            return null;
        }
        return obj;
    }

    private static int LM_Index(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = CheckBuffer(L, false);

        if (!L.IsInteger(2))
        {
            L.PushNil();
            return 1;
        }

        var key = L.ToInteger(2);

        if (key < 0 || key >= buffer.Length)
        {
            L.PushNil();
            return 1;
        }

        var value = buffer[key];

        // ABGR to RGB
        value =
            (value & 0x00_00_00_FFU) << 16 | // move R
            (value & 0x00_00_FF_00U) |       // move G
            (value & 0x00_FF_00_00U) >> 16;  // move B

        L.PushInteger(value);

        return 1;
    }

    private static int LM_NewIndex(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = CheckBuffer(L, false);
        if (!L.IsInteger(2))
        {
            return 0;
        }

        var key = L.ToInteger(2);

        if (key < 0 || key >= buffer.Length)
        {
            return 0;
        }

        if (!L.IsInteger(3))
        {
            return 0;
        }

        var value = (uint)L.ToInteger(3);

        // RGB to ABGR
        value =
            (value & 0x00_FF_00_00U) >> 16 | // move R
            (value & 0x00_00_FF_00U) |       // move G
            (value & 0x00_00_00_FFU) << 16 | // move B
            0xFF_00_00_00U;
        

        buffer[key] = value;

        return 0;
    }

    private static int LM_GC(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        CheckBuffer(L, true);

        return 0;
    }

    private static int LM_Length(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = CheckBuffer(L, false);

        L.PushInteger(buffer.LongLength);

        return 1;
    }

    private static int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = CheckBuffer(L, false);

        L.PushString(ObjectType);

        return 1;
    }
}
