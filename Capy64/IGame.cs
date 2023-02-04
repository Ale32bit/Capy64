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
