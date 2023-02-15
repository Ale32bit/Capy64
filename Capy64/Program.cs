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

using Capy64;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO;

using var game = new Capy64.Capy64();
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, c) =>
    {
        var settingsPath = Path.Combine(Capy64.Capy64.AppDataPath, "settings.json");
        if(!Directory.Exists(Capy64.Capy64.AppDataPath))
        {
            Directory.CreateDirectory(Capy64.Capy64.AppDataPath);
        }
        if (!File.Exists(settingsPath))
        {
            File.Copy("Assets/default.json", settingsPath);
        }

        c.AddJsonFile("Assets/default.json", false);
        c.AddJsonFile(settingsPath, false);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IGame>(game);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();