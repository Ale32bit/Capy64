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
using KeraLua;
using System;

namespace Capy64.Runtime.Objects;

public class Socket : IDisposable
{
    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}

public class SocketLib : IComponent
{
    private static Capy64 _game = null!;
    public SocketLib(Capy64 game)
    {
        _game = game;
    }

    private static readonly LuaRegister[] Methods = new LuaRegister[] {

        new(),
    };

    private static readonly LuaRegister[] MetaMethods = new LuaRegister[] {
        new()
        {
            name = "__index",
        },
        new(),
    };

    public void LuaInit(Lua L)
    {

    }
}