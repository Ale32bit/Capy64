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

using Capy64.API;
using Capy64.Core;
using Capy64.Eventing;
using Capy64.Integrations;
using Capy64.Runtime;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace Capy64;

public interface IGame
{
    Capy64 Game { get; }
    IList<IPlugin> NativePlugins { get; }
    IList<IPlugin> Plugins { get; }
    GameWindow Window { get; }
    Drawing Drawing { get; }
    Audio Audio { get; }
    LuaState LuaRuntime { get; set; }
    Eventing.EventEmitter EventEmitter { get; }
    void ConfigureServices(IServiceProvider serviceProvider);

    int Width { get; set; }
    int Height { get; set; }
    float Scale { get; set; }
    void UpdateSize(bool resize = true);

    event EventHandler<EventArgs> Exiting;
    void Run();
    void Exit();

    // Integrations
    DiscordIntegration Discord { get; }
}
