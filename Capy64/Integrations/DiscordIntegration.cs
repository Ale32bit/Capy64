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
using DiscordRPC;
using DiscordRPC.Logging;
using DiscordRPC.Message;
using Microsoft.Extensions.Configuration;
using System;

namespace Capy64.Integrations;

public class DiscordIntegration : IComponent
{
    public DiscordRpcClient Client { get; private set; }
    public readonly bool Enabled;
    private readonly IConfiguration _configuration;

    public DiscordIntegration(Capy64 game)
    {
        _configuration = game.Configuration;

        var discordConfig = _configuration.GetSection("Integrations:Discord");
        Enabled = discordConfig.GetValue("Enable", false);

        Client = new(discordConfig["ApplicationId"])
        {
            Logger = new ConsoleLogger() { Level = LogLevel.Warning }
        };

        Client.OnReady += OnReady;

        Capy64.Instance.Discord = this;

        if (Enabled)
            Client.Initialize();
    }

#nullable enable
    public void SetPresence(string details, string? state = null)
    {
        if (!Enabled)
            return;

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
