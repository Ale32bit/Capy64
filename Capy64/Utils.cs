// This file is part of Capy64 - https://github.com/Capy64/Capy64
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
