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
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Integrations;

public class DiscordIntegration : IComponent
{
    public DiscordRpcClient Client { get; private set; }
    private readonly IConfiguration _configuration;

    public DiscordIntegration(IConfiguration configuration)
    {
        _configuration = configuration;

        var discordConfig = _configuration.GetSection("Integrations:Discord");
        Client = new(discordConfig["ApplicationId"]);

        Client.Logger = new ConsoleLogger() { Level = DiscordRPC.Logging.LogLevel.Warning };

        Client.OnReady += OnReady;

        Capy64.Instance.Discord = this;

        if (discordConfig.GetValue("Enable", false))
        {
            Client.Initialize();
        }
    }

    public void SetPresence(string details, string? state = null)
    {
        Client.SetPresence(new RichPresence()
        {
            Details = details,
            State = state,
            Timestamps = Timestamps.Now,
            Assets = new Assets()
            {
                LargeImageKey = "image_large",
                LargeImageText = "Capy64 " + Capy64.Version,
                SmallImageKey = "image_small"
            }
        });
    }

    private void OnReady(object sender, ReadyMessage e)
    {
        Console.WriteLine("Discord RPC: Received Ready from user {0}", e.User.Username);
    }
}
