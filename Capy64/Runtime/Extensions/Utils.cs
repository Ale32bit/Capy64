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

using KeraLua;
using System;
using System.Collections;
using System.Linq;
using System.Reflection;

namespace Capy64.Runtime.Extensions;
public static class Utils
{
    public static void PushArray(this Lua L, object obj)
    {
        var iterable = obj as IEnumerable;

        L.NewTable();
        long i = 1;
        foreach (var item in iterable)
        {
            L.PushValue(item);
            L.RawSetInteger(-2, i++);
        }
        L.SetTop(-1);
    }
#nullable enable
    public static int PushValue(this Lua L, object? obj)
    {
        var type = obj?.GetType();
        switch (obj)
        {
            case string str:
                L.PushString(str);
                break;

            case char:
                L.PushString(obj.ToString());
                break;

            case byte:
            case sbyte:
            case short:
            case ushort:
            case int:
            case uint:
            case double:
                L.PushNumber(Convert.ToDouble(obj));
                break;

            case long l:
                L.PushInteger(l);
                break;

            case bool b:
                L.PushBoolean(b);
                break;

            case null:
                L.PushNil();
                break;

            case byte[] b:
                L.PushBuffer(b);
                break;

            case LuaFunction func:
                L.PushCFunction(func);
                break;

            case IntPtr ptr:
                L.PushLightUserData(ptr);
                break;

            default:
                if (type is not null && type.IsArray)
                {
                    L.PushArray(obj);
                }
                else
                {
                    throw new Exception("Invalid type provided");
                }
                break;

        }
        return 1;
    }
}