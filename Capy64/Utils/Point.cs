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
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Utils;

public struct Point : IEquatable<Point>
{
    private static readonly Point zeroPoint;

    [DataMember]
    public int X;

    [DataMember]
    public int Y;

    public static Point Zero => zeroPoint;

    internal string DebugDisplayString => X + "  " + Y;

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Point(int value)
    {
        X = value;
        Y = value;
    }

    public static Point operator +(Point value1, Point value2)
    {
        return new Point(value1.X + value2.X, value1.Y + value2.Y);
    }

    public static Point operator -(Point value1, Point value2)
    {
        return new Point(value1.X - value2.X, value1.Y - value2.Y);
    }

    public static Point operator *(Point value1, Point value2)
    {
        return new Point(value1.X * value2.X, value1.Y * value2.Y);
    }

    public static Point operator /(Point source, Point divisor)
    {
        return new Point(source.X / divisor.X, source.Y / divisor.Y);
    }

    public static bool operator ==(Point a, Point b)
    {
        return a.Equals(b);
    }

    public static bool operator !=(Point a, Point b)
    {
        return !a.Equals(b);
    }

    public override bool Equals(object obj)
    {
        if (obj is Point)
        {
            return Equals((Point)obj);
        }

        return false;
    }

    public bool Equals(Point other)
    {
        if (X == other.X)
        {
            return Y == other.Y;
        }

        return false;
    }

    public override int GetHashCode()
    {
        return (17 * 23 + X.GetHashCode()) * 23 + Y.GetHashCode();
    }

    public override string ToString()
    {
        return "{X:" + X + " Y:" + Y + "}";
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 ToVector2()
    {
        return new Vector2(X, Y);
    }

    public void Deconstruct(out int x, out int y)
    {
        x = X;
        y = Y;
    }
}