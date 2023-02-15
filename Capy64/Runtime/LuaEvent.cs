// This file is part of Capy64 - https://github.com/Capy64/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
//    Licensed under the Apache License, Version 2.0 (the "License").
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0

using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime;

public class LuaEvent
{
    public LuaEvent(string name, Func<Lua, int> handler)
    {
        Name = name;
        Handler = handler;
    }

    public string Name { get; set; }
    public Func<Lua, int> Handler { get; set; }
}
