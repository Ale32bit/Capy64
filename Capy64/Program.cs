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