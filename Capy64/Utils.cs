// This file is part of Capy64 - https://github.com/Capy64/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
//    Licensed under the Apache License, Version 2.0 (the "License").
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Xna.Framework;

namespace Capy64;

public static class Utils
{
    public struct Borders
    {
        public int Top, Bottom, Left, Right;
    }

    /// <summary>
    /// Return the sane 0xRRGGBB format
    /// </summary>
    /// <param name="color"></param>
    public static int PackRGB(Color color)
    {
        return
            (color.R << 16) +
            (color.G << 8) +
            (color.B);
    }

    public static void UnpackRGB(uint packed, out byte r, out byte g, out byte b)
    {
        b = (byte)(packed & 0xff);
        g = (byte)((packed >> 8) & 0xff);
        r = (byte)((packed >> 16) & 0xff);
    }
}
