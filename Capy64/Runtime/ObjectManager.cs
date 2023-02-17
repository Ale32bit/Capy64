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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class ObjectManager : IComponent
{
    private static ConcurrentDictionary<nint, object> _objects = new();

    private static IGame _game;
    public ObjectManager(IGame game)
    {
        _game = game;
        _game.EventEmitter.OnClose += OnClose;
    }

    public static void PushObject<T>(Lua L, T obj)
    {
        if (obj == null)
        {
            L.PushNil();
            return;
        }

        var p = L.NewUserData(1);
        _objects[p] = obj;
    }

    public static T ToObject<T>(Lua L, int index, bool freeGCHandle = true)
    {
        if (L.IsNil(index) || !L.IsUserData(index))
            return default(T);

        var data = L.ToUserData(index);
        if (data == IntPtr.Zero)
            return default(T);

        if (!_objects.ContainsKey(data))
            return default(T);

        var reference = (T)_objects[data];

        if (freeGCHandle)
            _objects.Remove(data, out _);

        return reference;
    }

    public static T CheckObject<T>(Lua L, int argument, string typeName, bool freeGCHandle = true)
    {
        if (L.IsNil(argument) || !L.IsUserData(argument))
            return default(T);

        IntPtr data = L.CheckUserData(argument, typeName);
        if (data == IntPtr.Zero)
            return default(T);

        if (!_objects.ContainsKey(data))
            return default(T);

        var reference = (T)_objects[data];

        if (freeGCHandle)
            _objects.Remove(data, out _);

        return reference;
    }

    private void OnClose(object sender, EventArgs e)
    {
        foreach (var pair in _objects)
        {
            if (pair.Value is IDisposable disposable)
                disposable.Dispose();
        }

        _objects.Clear();
    }
}
