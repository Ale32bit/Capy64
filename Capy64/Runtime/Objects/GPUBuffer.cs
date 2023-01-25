using KeraLua;
using System;

namespace Capy64.Runtime.Objects;

public class GPUBuffer
{
    public const string ObjectType = "GPUBuffer";

    private uint[] _buffer;
    public GPUBuffer(uint[] buffer)
    {
        _buffer = buffer;
    }

    public void Push(Lua L)
    {
        if (L.NewMetaTable(ObjectType))
        {
            L.PushString("__index");
            L.PushCFunction(LM_Index);
            L.SetTable(-3);

            L.PushString("__newindex");
            L.PushCFunction(LM_NewIndex);
            L.SetTable(-3);

            L.PushString("__len");
            L.PushCFunction(LM_Length);
            L.SetTable(-3);

            L.PushString("__gc");
            L.PushCFunction(LM_GC);
            L.SetTable(-3);

            L.PushString("__close");
            L.PushCFunction(LM_GC);
            L.SetTable(-3);
        }

        L.PushObject(_buffer);
        L.SetMetaTable(ObjectType);
    }

    private static int LM_Index(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = L.CheckObject<uint[]>(1, ObjectType, false);

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

        var buffer = L.CheckObject<uint[]>(1, ObjectType, false);
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

        L.CheckObject<uint[]>(1, ObjectType, true);

        return 0;
    }

    private static int LM_Length(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = L.CheckObject<uint[]>(1, ObjectType, false);

        L.PushInteger(buffer.LongLength);

        return 1;
    }
}
