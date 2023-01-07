using Capy64;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

using var game = new Capy64.Capy64();
using IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.AddSingleton<IGame>(game);
        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();