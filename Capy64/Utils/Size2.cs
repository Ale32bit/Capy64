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

using MonoGame.Extended;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Utils;

public struct Size2 : IEquatable<Size2>, IEquatableByRef<Size2>
{
    public static readonly Size2 Empty;

    public float Width;

    public float Height;

    public bool IsEmpty
    {
        get
        {
            if (Width == 0f)
            {
                return Height == 0f;
            }

            return false;
        }
    }

    internal string DebugDisplayString => ToString();

    public Size2(float width, float height)
    {
        Width = width;
        Height = height;
    }

    public static bool operator ==(Size2 first, Size2 second)
    {
        return first.Equals(ref second);
    }

    public bool Equals(Size2 size)
    {
        return Equals(ref size);
    }

    public bool Equals(ref Size2 size)
    {
        if (Width == size.Width)
        {
            return Height == size.Height;
        }

        return false;
    }

    public override bool Equals(object obj)
    {
        if (obj is Size2)
        {
            return Equals((Size2)obj);
        }

        return false;
    }

    public static bool operator !=(Size2 first, Size2 second)
    {
        return !(first == second);
    }

    public static Size2 operator +(Size2 first, Size2 second)
    {
        return Add(first, second);
    }

    public static Size2 Add(Size2 first, Size2 second)
    {
        Size2 result = default(Size2);
        result.Width = first.Width + second.Width;
        result.Height = first.Height + second.Height;
        return result;
    }

    public static Size2 operator -(Size2 first, Size2 second)
    {
        return Subtract(first, second);
    }

    public static Size2 operator /(Size2 size, float value)
    {
        return new Size2(size.Width / value, size.Height / value);
    }

    public static Size2 operator *(Size2 size, float value)
    {
        return new Size2(size.Width * value, size.Height * value);
    }

    public static Size2 Subtract(Size2 first, Size2 second)
    {
        Size2 result = default(Size2);
        result.Width = first.Width - second.Width;
        result.Height = first.Height - second.Height;
        return result;
    }

    public override int GetHashCode()
    {
        return (Width.GetHashCode() * 397) ^ Height.GetHashCode();
    }

    public static implicit operator Size2(Point2 point)
    {
        return new Size2(point.X, point.Y);
    }

    public static implicit operator Size2(Point point)
    {
        return new Size2(point.X, point.Y);
    }

    public static implicit operator Point2(Size2 size)
    {
        return new Point2(size.Width, size.Height);
    }

    public static implicit operator Vector2(Size2 size)
    {
        return new Vector2(size.Width, size.Height);
    }

    public static implicit operator Size2(Vector2 vector)
    {
        return new Size2(vector.X, vector.Y);
    }

    public static explicit operator Point(Size2 size)
    {
        return new Point((int)size.Width, (int)size.Height);
    }

    public override string ToString()
    {
        return $"Width: {Width}, Height: {Height}";
    }
}