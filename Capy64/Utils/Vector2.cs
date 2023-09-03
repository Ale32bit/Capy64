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
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Utils;

public struct Vector2 : IEquatable<Vector2>
{
    private static readonly Vector2 zeroVector = new(0f, 0f);
    private static readonly Vector2 unitVector = new(1f, 1f);
    private static readonly Vector2 unitXVector = new(1f, 0f);
    private static readonly Vector2 unitYVector = new(0f, 1f);
    public static Vector2 Zero => zeroVector;
    public static Vector2 One => unitVector;
    public static Vector2 UnitX => unitXVector;
    public static Vector2 UnitY => unitYVector;

    public float X { get; set; }
    public float Y { get; set; }

    public Vector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public Vector2(float value)
    {
        X = value;
        Y = value;
    }

    public static implicit operator Vector2(System.Numerics.Vector2 value)
    {
        return new Vector2(value.X, value.Y);
    }

    public static Vector2 operator -(Vector2 value)
    {
        value.X = 0f - value.X;
        value.Y = 0f - value.Y;
        return value;
    }

    public static Vector2 operator +(Vector2 value1, Vector2 value2)
    {
        value1.X += value2.X;
        value1.Y += value2.Y;
        return value1;
    }

    public static Vector2 operator -(Vector2 value1, Vector2 value2)
    {
        value1.X -= value2.X;
        value1.Y -= value2.Y;
        return value1;
    }

    public static Vector2 operator *(Vector2 value1, Vector2 value2)
    {
        value1.X *= value2.X;
        value1.Y *= value2.Y;
        return value1;
    }

    public static Vector2 operator *(Vector2 value, float scaleFactor)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    public static Vector2 operator *(float scaleFactor, Vector2 value)
    {
        value.X *= scaleFactor;
        value.Y *= scaleFactor;
        return value;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 value1, Vector2 value2)
    {
        value1.X /= value2.X;
        value1.Y /= value2.Y;
        return value1;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector2 operator /(Vector2 value1, float divider)
    {
        float num = 1f / divider;
        value1.X *= num;
        value1.Y *= num;
        return value1;
    }

    public static bool operator ==(Vector2 value1, Vector2 value2)
    {
        if (value1.X == value2.X)
        {
            return value1.Y == value2.Y;
        }

        return false;
    }

    public static bool operator !=(Vector2 value1, Vector2 value2)
    {
        if (value1.X == value2.X)
        {
            return value1.Y != value2.Y;
        }

        return true;
    }

    public override bool Equals(object obj)
    {
        if (obj is Vector2)
        {
            return Equals((Vector2)obj);
        }

        return false;
    }

    public bool Equals(Vector2 other)
    {
        return X == other.X && Y == other.Y;
    }
}
