// This file is part of Capy64 - https://github.com/Capy64/Capy64
// Copyright 2023 Alessandro "AlexDevs" Proto
//
//    Licensed under the Apache License, Version 2.0 (the "License").
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0

using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Capy64;

public class Worker : IHostedService
{
    private readonly IGame _game;
    private readonly IHostApplicationLifetime _appLifetime;
    private readonly IServiceProvider _serviceProvider;

    public Worker(IGame game, IHostApplicationLifetime appLifetime, IServiceProvider serviceProvider)
    {
        _game = game;
        _appLifetime = appLifetime;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _appLifetime.ApplicationStarted.Register(OnStarted);
        _appLifetime.ApplicationStopping.Register(OnStopping);
        _appLifetime.ApplicationStopped.Register(OnStopped);

        _game.Exiting += OnGameExiting;

        _game.ConfigureServices(_serviceProvider);

        return Task.CompletedTask;
    }

    private void OnGameExiting(object sender, EventArgs e)
    {
        StopAsync(new CancellationToken());
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _appLifetime.StopApplication();

        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        _game.Run();
    }

    private void OnStopping()
    {
    }

    private void OnStopped()
    {
    }
}