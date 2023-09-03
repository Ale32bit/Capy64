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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static SDL2.SDL;

namespace Capy64.Utils;

public struct Color
{
    public byte R
    {
        get => (byte)(PackedValue >> 16);
        set => PackedValue = PackedValue & 0xFF00FFFFu | (uint)value << 16;
    }
    public byte G
    {
        get => (byte)(PackedValue >> 8);
        set => PackedValue = PackedValue & 0xFFFF00FFu | (uint)value << 8;
    }
    public byte B
    {
        get => (byte)PackedValue;
        set => PackedValue = PackedValue & 0xFFFFFF00u | value;
    }
    public byte A
    {
        get => (byte)(PackedValue >> 24);
        set => PackedValue = PackedValue & 0x00FFFFFFu | (uint)value << 24;
    }
    public uint PackedRGB => PackedValue & 0xFFFFFFu;
    public uint PackedValue { get; set; }

    public Color(byte r, byte g, byte b)
    {
        R = r;
        G = g;
        B = b;
        A = 255;
    }

    public Color(byte r, byte g, byte b, byte a)
    {
        R = r;
        G = g;
        B = b;
        A = a;
    }

    public Color(uint packedValue)
    {
        PackedValue = packedValue;
    }

    public SDL_Color ToSDL() => new()
    {
        r = R,
        g = G,
        b = B,
        a = A,
    };

    public static Color Transparent { get; private set; }
    public static Color Black { get; private set; }
    public static Color White { get; private set; }
    public static Color Gray { get; private set; }
    public static Color Red { get; private set; }
    public static Color Green { get; private set; }
    public static Color Blue { get; private set; }
    public static Color Yellow { get; private set; }
    public static Color Magenta { get; private set; }
    public static Color Cyan { get; private set; }

    static Color()
    {
        Transparent = new Color(0u);
        Black = new Color(0, 0, 0);
        White = new Color(255, 255, 255);
        Gray = new Color(128, 128, 128);
        Red = new Color(255, 0, 0);
        Green = new Color(0, 255, 0);
        Blue = new Color(0, 0, 255);
        Yellow = new Color(255, 255, 0);
        Magenta = new Color(255, 0, 255);
        Cyan = new Color(0, 255, 255);
    }
}
