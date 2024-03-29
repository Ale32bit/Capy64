﻿// This file is part of Capy64 - https://github.com/Ale32bit/Capy64
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
using Capy64.Runtime.Objects;
using KeraLua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Capy64.Runtime.Libraries;

public class GPULib : IComponent
{

    private static Capy64 _game;
    public GPULib(Capy64 game)
    {
        _game = game;
    }

    private readonly LuaRegister[] Library = new LuaRegister[] {
        new()
        {
            name = "getSize",
            function = L_GetSize,
        },
        new()
        {
            name = "setSize",
            function = L_SetSize,
        },
        new()
        {
            name = "isResizable",
            function = L_IsResizable,
        },
        new()
        {
            name = "getPixel",
            function = L_GetPixel,
        },
        new()
        {
            name = "plot",
            function = L_Plot,
        },
        new()
        {
            name = "plots",
            function = L_Plots,
        },
        new()
        {
            name = "drawPoint",
            function = L_DrawPoint,
        },
        new()
        {
            name = "drawCircle",
            function = L_DrawCircle
        },
        new()
        {
            name = "drawLine",
            function = L_DrawLine
        },
        new()
        {
            name = "drawRectangle",
            function = L_DrawRectangle
        },
        new()
        {
            name = "drawPolygon",
            function = L_DrawPolygon
        },
        new()
        {
            name = "drawEllipse",
            function = L_DrawEllipse
        },
        new()
        {
            name = "drawString",
            function = L_DrawString
        },
        new()
        {
            name = "measureString",
            function = L_MeasureString
        },
        new()
        {
            name = "getBuffer",
            function = L_GetBuffer,
        },
        new()
        {
            name = "setBuffer",
            function = L_SetBuffer,
        },
        new()
        {
            name = "newBuffer",
            function = L_NewBuffer,
        },
        new()
        {
            name = "bufferFrom",
            function = L_BufferFrom,
        },
        new()
        {
            name = "drawBuffer",
            function = L_DrawBuffer,
        },
        new()
        {
            name = "loadImageAsync",
            function = L_LoadImageAsync,
        },
        new()
        {
            name = "clear",
            function = L_Clear,
        },
        new(), // NULL
    };

    public void LuaInit(Lua state)
    {
        state.RequireF("gpu", OpenLib, false);
    }

    public int OpenLib(IntPtr state)
    {
        var l = Lua.FromIntPtr(state);
        l.NewLib(Library);
        return 1;
    }

    public static void GetColor(uint c, out byte r, out byte g, out byte b)
    {
        Utils.UnpackRGB(c, out r, out g, out b);
    }

    private static int L_GetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushInteger(_game.Width);
        L.PushInteger(_game.Height);

        return 2;
    }

    private static int L_SetSize(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        if (_game.EngineMode == EngineMode.Classic)
        {
            return L.Error("Screen is not resizable");
        }

        var w = L.CheckInteger(1);
        var h = L.CheckInteger(2);

        _game.Width = (int)w;
        _game.Height = (int)h;

        _game.UpdateSize();

        L.PushBoolean(true);

        return 1;
    }

    private static int L_IsResizable(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushBoolean(_game.EngineMode != EngineMode.Classic);

        return 1;
    }

    private static int L_GetPixel(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;

        var c = _game.Drawing.GetPixel(new(x, y));

        L.PushInteger(Utils.PackRGB(c));

        return 1;
    }

    private static int L_Plot(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.Plot(new Point(x, y), new Color(r, g, b));

        return 0;
    }

    private static int L_Plots(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        var c = L.CheckInteger(2);

        L.SetTop(1);

        int size = (int)L.Length(1);

        L.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

        List<Point> pts = new();
        for (int i = 1; i <= size; i += 2)
        {
            L.GetInteger(1, i);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + i);
            var x = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            L.GetInteger(1, i + 1);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + (i + 1));
            var y = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            pts.Add(new Point(x, y));
        }

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.Plot(pts, new(r, g, b));

        return 0;
    }

    private static int L_DrawPoint(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);
        var s = (int)L.OptNumber(4, 1);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawPoint(new(x, y), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawCircle(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var rad = (int)L.CheckNumber(3);
        var c = L.CheckInteger(4);
        var t = (int)L.OptNumber(5, 1);
        var s = (int)L.OptInteger(6, -1);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawCircle(new(x, y), rad, new Color(r, g, b), t, s);

        return 0;
    }

    private static int L_DrawLine(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x1 = (int)L.CheckNumber(1) - 1; // do not question, please
        var y1 = (int)L.CheckNumber(2);
        var x2 = (int)L.CheckNumber(3);
        var y2 = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawLine(new(x1, y1), new(x2, y2), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawRectangle(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var w = (int)L.CheckNumber(3);
        var h = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawRectangle(new(x, y), new(w, h), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawPolygon(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        L.CheckType(3, LuaType.Table);
        var c = L.CheckInteger(4);
        var s = (int)L.OptNumber(5, 1);

        int size = (int)L.Length(3);

        L.ArgumentCheck(size % 2 == 0, 1, "expected an even table");

        List<Vector2> pts = new();
        for (int i = 1; i <= size; i += 2)
        {
            L.GetInteger(3, i);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + i);
            var xp = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            L.GetInteger(3, i + 1);
            L.ArgumentCheck(L.IsNumber(-1), 1, "expected number at index " + (i + 1));
            var yp = (int)L.ToNumber(-1) - 1;
            L.Pop(1);

            pts.Add(new(xp, yp));
        }

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawPolygon(new(x, y), pts.ToArray(), new(r, g, b), s);

        return 0;
    }

    private static int L_DrawEllipse(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var rx = (int)L.CheckNumber(3);
        var ry = (int)L.CheckNumber(4);
        var c = L.CheckInteger(5);
        var s = (int)L.OptNumber(6, 1);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.DrawEllipse(new(x, y), new(rx, ry), new Color(r, g, b), s);

        return 0;
    }

    private static int L_DrawString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var x = (int)L.CheckNumber(1) - 1;
        var y = (int)L.CheckNumber(2) - 1;
        var c = L.CheckInteger(3);
        var t = L.CheckString(4);

        GetColor((uint)c, out var r, out var g, out var b);
        try
        {
            _game.Drawing.DrawString(new Vector2(x, y), t, new Color(r, g, b));
        }
        catch (ArgumentException ex) // UTF-16 fuckery
        {
            L.Error(ex.Message);
        }

        return 0;
    }

    private static int L_MeasureString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var t = L.CheckString(1);

        var sizes = _game.Drawing.MeasureString(t);

        L.PushNumber((int)sizes.X);
        L.PushNumber((int)sizes.Y);

        return 2;
    }

    private static int L_GetBuffer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);


        var buffer = new uint[_game.Width * _game.Height];
        _game.Drawing.Canvas.GetData(buffer);

        var gpuBuffer = new GPUBufferMeta.GPUBuffer
        {
            Buffer = buffer,
            Width = _game.Width,
            Height = _game.Height,
        };

        ObjectManager.PushObject(L, gpuBuffer);
        L.SetMetaTable(GPUBufferMeta.ObjectType);

        return 1;
    }

    private static int L_SetBuffer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = GPUBufferMeta.CheckBuffer(L, false);

        _game.Drawing.Canvas.SetData(buffer.Buffer);

        return 0;
    }

    private static int L_NewBuffer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var width = (int)L.OptInteger(1, _game.Width);
        var height = (int)L.OptInteger(2, _game.Height);

        var buffer = new uint[width * height];

        var gpuBuffer = new GPUBufferMeta.GPUBuffer
        {
            Buffer = buffer,
            Width = width,
            Height = height,
        };

        ObjectManager.PushObject(L, gpuBuffer);
        L.SetMetaTable(GPUBufferMeta.ObjectType);

        return 1;
    }

    private static int L_BufferFrom(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.CheckType(1, LuaType.Table);
        var width = (int)L.CheckInteger(2);
        var height = (int)L.CheckInteger(3);

        if (width <= 0)
        {
            return L.ArgumentError(2, "width must be a positive integer.");
        }

        if (height <= 0)
        {
            return L.ArgumentError(3, "height must be a positive integer.");
        }

        var buffer = new uint[width * height];

        var tableSize = L.RawLen(1);
        L.ArgumentCheck(tableSize == buffer.Length, 1, "table length does not match buffer size");

        for (int i = 1; i <= tableSize; i++)
        {
            L.GetInteger(1, i);
            var value = (uint)L.CheckInteger(-1);
            L.Pop(1);
            // ARGB to ABGR
            value =
                (value & 0xFF_00_00_00U) |
                ((value & 0x00_FF_00_00U) >> 16) | // move R
                (value & 0x00_00_FF_00U) |       // move G
                ((value & 0x00_00_00_FFU) << 16); // move B

            buffer[i - 1] = value;
        }

        var gpuBuffer = new GPUBufferMeta.GPUBuffer
        {
            Buffer = buffer,
            Width = width,
            Height = height,
        };

        ObjectManager.PushObject(L, gpuBuffer);
        L.SetMetaTable(GPUBufferMeta.ObjectType);

        return 1;
    }

    private static int L_DrawBuffer(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var buffer = GPUBufferMeta.CheckBuffer(L, false);

        var x = (int)L.CheckInteger(2) - 1;
        var y = (int)L.CheckInteger(3) - 1;

        Rectangle? source = null;
        Color color = Color.White;
        float rotation = 0;
        Vector2 origin = Vector2.Zero;
        Vector2 scale = Vector2.One;
        SpriteEffects effects = SpriteEffects.None;

        if (L.IsTable(4))
        {
            if (L.GetField(-1, "source") == LuaType.Table)
            {
                int sx, sy, sw, sh;

                if (L.GetInteger(-1, 1) != LuaType.Number)
                    L.CheckNumber(-1);
                sx = (int)L.ToNumber(-1);
                L.Pop(1);

                if (L.GetInteger(-1, 2) != LuaType.Number)
                    L.CheckNumber(-1);
                sy = (int)L.ToNumber(-1);
                L.Pop(1);

                if (L.GetInteger(-1, 3) != LuaType.Number)
                    L.CheckNumber(-1);
                sw = (int)L.ToNumber(-1);
                L.Pop(1);

                if (L.GetInteger(-1, 4) != LuaType.Number)
                    L.CheckNumber(-1);
                sh = (int)L.ToNumber(-1);
                L.Pop(1);

                source = new(sx, sy, sw, sh);
            }
            L.Pop(1);

            if (L.GetField(-1, "color") == LuaType.Number)
            {
                var c = (uint)L.ToNumber(-1);
                GetColor(c, out var r, out var g, out var b);
                color = new Color(r, g, b);
            }
            L.Pop(1);

            if (L.GetField(-1, "rotation") == LuaType.Number)
            {
                rotation = (float)L.ToNumber(-1);
            }
            L.Pop(1);

            if (L.GetField(-1, "origin") == LuaType.Table)
            {
                int ox, oy;

                if (L.GetInteger(-1, 1) != LuaType.Number)
                    L.CheckNumber(-1);
                ox = (int)L.ToNumber(-1);
                L.Pop(1);

                if (L.GetInteger(-1, 2) != LuaType.Number)
                    L.CheckNumber(-1);
                oy = (int)L.ToNumber(-1);
                L.Pop(1);

                origin = new Vector2(ox, oy);
            }
            L.Pop(1);

            if (L.GetField(-1, "scale") == LuaType.Table)
            {
                float sx = 1;
                float sy = 1;

                if (L.GetInteger(-1, 1) == LuaType.Number)
                    sx = (float)L.ToNumber(-1);
                L.Pop(1);

                if (L.GetInteger(-1, 2) == LuaType.Number)
                    sy = (float)L.ToNumber(-1);
                L.Pop(1);

                scale = new(sx, sy);
            }
            L.Pop(1);

            if (L.GetField(-1, "effects") == LuaType.Number)
            {
                var flags = L.CheckInteger(-1);
                effects = (SpriteEffects)flags;
            }
            L.Pop(1);
        }

        _game.Drawing.DrawBuffer(buffer.Buffer, new()
        {
            X = x,
            Y = y,
            Width = buffer.Width,
            Height = buffer.Height,
        }, source, color, rotation, origin, scale, effects);

        return 0;
    }

    private static int L_LoadImageAsync(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var path = L.CheckString(1);

        path = FileSystemLib.Resolve(path);

        if (!File.Exists(path))
        {
            L.Error("file not found");
            return 0;
        }

        var task = TaskMeta.Push(L, GPUBufferMeta.ObjectType);

        Texture2D texture;
        try
        {
            texture = Texture2D.FromFile(Capy64.Instance.Drawing.Canvas.GraphicsDevice, path);
        }
        catch (Exception e)
        {
            task.Reject(e.Message);
            return 1;
        }

        var data = new uint[texture.Width * texture.Height];
        texture.GetData(data);

        Task.Run(() =>
        {
            /*if (_game.EngineMode == EngineMode.Classic)
            {
                for (int i = 0; i < data.Length; i++)
                {
                    var value = data[i];

                    // ABGR to RGB
                    value =
                        ((value & 0x00_00_00_FFU) << 16) | // move R
                        (value & 0x00_00_FF_00U) |       // move G
                        ((value & 0x00_FF_00_00U) >> 16);  // move B

                    value = ColorPalette.GetColor(value);

                    // RGB to ABGR
                    value =
                        ((value & 0x00_FF_00_00U) >> 16) | // move R
                        (value & 0x00_00_FF_00U) |       // move G
                        ((value & 0x00_00_00_FFU) << 16) | // move B
                        0xFF_00_00_00U;

                    data[i] = value;
                }
            }*/

            var buffer = new GPUBufferMeta.GPUBuffer
            {
                Buffer = data,
                Height = texture.Height,
                Width = texture.Width,
            };

            task.Fulfill(lk =>
            {
                ObjectManager.PushObject(lk, buffer);
                lk.SetMetaTable(GPUBufferMeta.ObjectType);
            });

            texture.Dispose();
        });

        return 1;
    }

    private static int L_Clear(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var c = L.OptInteger(1, 0x000000);

        GetColor((uint)c, out var r, out var g, out var b);
        _game.Drawing.Clear(new Color(r, g, b));

        return 0;
    }
}
