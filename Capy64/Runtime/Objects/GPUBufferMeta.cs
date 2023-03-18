// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
// Licensed under the Apache License, Version 2.0 (the "License").
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using Capy64.API;
using KeraLua;
using System;

namespace Capy64.Runtime.Objects;

public class GPUBufferMeta : IComponent
{
    public const string ObjectType = "GPUBuffer";

    public struct GPUBuffer
    {
        public uint[] Buffer { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    private static readonly LuaRegister[] MetaMethods = new LuaRegister[]
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

    private static IGame _game;
    public GPUBufferMeta(IGame game)
    {
        _game = game;
    }

    public void LuaInit(Lua L)
    {
        CreateMeta(L);
    }

    public static uint GetColor(uint color)
    {
        /*if (_game.EngineMode == EngineMode.Classic)
            return ColorPalette.GetColor(color);*/

        return color;
    }

    public static void CreateMeta(Lua L)
    {
        L.NewMetaTable(ObjectType);
        L.SetFuncs(MetaMethods, 0);
    }

    public static GPUBuffer ToBuffer(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<GPUBuffer>(L, 1, gc);
    }

    public static GPUBuffer CheckBuffer(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<GPUBuffer>(L, 1, ObjectType, gc);
        if (obj.Buffer is null)
        {
            L.Error("attempt to use a closed buffer");
            return default;
        }
        return obj;
    }

    private static int LM_Index(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = CheckBuffer(L, false);

        if (!L.IsInteger(2))
        {
            var vkey = L.ToString(2);

            if (vkey == "width")
                L.PushInteger(buffer.Width);
            else if (vkey == "height")
                L.PushInteger(buffer.Height);
            else
                L.PushNil();

            return 1;
        }

        var key = L.ToInteger(2);

        if (key < 0 || key >= buffer.Buffer.Length)
        {
            L.PushNil();
            return 1;
        }

        var value = buffer.Buffer[key];

        // ABGR to RGB
        value =
            ((value & 0x00_00_00_FFU) << 16) | // move R
            (value & 0x00_00_FF_00U) |       // move G
            ((value & 0x00_FF_00_00U) >> 16);  // move B

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

        if (key < 0 || key >= buffer.Buffer.Length)
        {
            return 0;
        }

        if (!L.IsInteger(3))
        {
            return 0;
        }

        var value = (uint)L.ToInteger(3);
        value = GetColor(value);

        // RGB to ABGR
        value =
            ((value & 0x00_FF_00_00U) >> 16) | // move R
            (value & 0x00_00_FF_00U) |       // move G
            ((value & 0x00_00_00_FFU) << 16) | // move B
            0xFF_00_00_00U;


        buffer.Buffer[key] = value;

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

        L.PushInteger(buffer.Buffer.LongLength);

        return 1;
    }

    private static unsafe int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var buffer = ToBuffer(L);
        if (buffer.Buffer is not null)
        {
            L.PushString("GPUBuffer ({0:X})", (ulong)&buffer);
        }
        else
        {
            L.PushString("GPUBuffer (closed)");
        }
        return 1;
    }
}
