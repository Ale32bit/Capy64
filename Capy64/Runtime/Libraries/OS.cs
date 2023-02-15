// This file is part of Capy64 - https://github.com/Capy64/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
//    Licensed under the Apache License, Version 2.0 (the "License").
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0

using Capy64.API;
using KeraLua;
using System;

namespace Capy64.Runtime.Libraries;

public class OS : IPlugin
{
    private static IGame _game;
    public OS(IGame game)
    {
        _game = game;
    }

    public void LuaInit(Lua state)
    {
        state.GetGlobal("os");
    }
}
