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
using Capy64.Core;
using Capy64.Eventing;
using Capy64.PluginManager;
using Capy64.Runtime;
using Capy64.Utils;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Timers;
using static SDL2.SDL;

namespace Capy64;

public class Game : IDisposable
{
    public static Game Instance { get; private set; }
    public const string Version = "1.1.0-beta";
    public nint Window { get; private set; } = 0;
    public nint Renderer { get; private set; } = 0;
    public nint VideoSurface { get; private set; } = 0;
    public nint SurfaceRenderer { get; private set; } = 0;
    public int WindowWidth { get; private set; }
    public int WindowHeight { get; private set; }
    public int Width { get; set; } = DefaultParameters.Width;
    public int Height { get; set; } = DefaultParameters.Height;
    public float Scale { get; set; } = DefaultParameters.Scale;
    public int Tickrate { get; set; } = DefaultParameters.ClassicTickrate;
    public int Framerate { get; set; } = 60;

    public float Ticktime => 1000f / Tickrate;
    public float Frametime => 1000f / Framerate;
    public IList<IComponent> NativePlugins { get; private set; }
    public IList<IComponent> Plugins { get; private set; }

    public Color BorderColor { get; set; } = Color.Black;

    public Borders Borders
    {
        get => _borders;
        set
        {
            _borders = value;
            _outputRect = new()
            {
                x = value.Left,
                y = value.Top,
                w = (int)(Width * Scale),
                h = (int)(Height * Scale),
            };
        }
    }

    private SDL_Rect _outputRect;
    private Borders _borders;

    public static readonly string AssemblyPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
    public static readonly string AssetsPath = Path.Combine(AssemblyPath, "Assets");
    public IConfiguration Configuration { get; private set; }

    public static class DefaultParameters
    {
        public const int Width = 318;
        public const int Height = 240;
        public const float Scale = 2f;
        public const float BorderMultiplier = 1.5f;
        public static readonly EngineMode EngineMode = EngineMode.Classic;

        public const int ClassicTickrate = 30;
        public const int FreeTickrate = 60;
    }

    public static string AppDataPath
    {
        get
        {
            string baseDir =
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Environment.GetFolderPath(
                    Environment.SpecialFolder.ApplicationData,
                    Environment.SpecialFolderOption.Create) :
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.Create) :
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? Environment.GetFolderPath(
                    Environment.SpecialFolder.LocalApplicationData,
                    Environment.SpecialFolderOption.Create) :
                "./";

            return Path.Combine(baseDir, "Capy64");
        }
    }

    private bool _running = false;
    private InputManager _inputManager;
    public Eventing.EventEmitter EventEmitter = new();
    public readonly Canvas Canvas;
    public Audio Audio { get; private set; }
    public LuaState LuaRuntime { get; set; }

    public Game()
    {
        Instance = this;
        Canvas = new(this);
        ResetBorder();
        UpdateSize();

        _inputManager = new(this, EventEmitter);
    }

    public void Initialize()
    {
        var configBuilder = new ConfigurationBuilder();

        var settingsPath = Path.Combine(AppDataPath, "settings.json");
        if (!Directory.Exists(AppDataPath))
        {
            Directory.CreateDirectory(AppDataPath);
        }
        if (!File.Exists(settingsPath))
        {
            File.Copy(Path.Combine(AssetsPath, "default.json"), settingsPath);
        }

        configBuilder.AddJsonFile(Path.Combine(AssetsPath, "default.json"), false);
        configBuilder.AddJsonFile(settingsPath, false);

        Configuration = configBuilder.Build();

        Scale = Configuration.GetValue("Window:Scale", DefaultParameters.Scale);

        Audio = new Audio();

        NativePlugins = LoadNativePlugins();
        var safeMode = Configuration.GetValue("SafeMode", false);
        if (!safeMode)
            Plugins = PluginLoader.LoadAllPlugins(Path.Combine(AppDataPath, "plugins"));
    }

    private List<IComponent> LoadNativePlugins()
    {
        var iType = typeof(IComponent);
        var types = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(p => iType.IsAssignableFrom(p) && !p.IsInterface);

        var plugins = new List<IComponent>();

        foreach (var type in types)
        {
            var instance = (IComponent)Activator.CreateInstance(type, this);
            plugins.Add(instance);
        }

        return plugins;
    }

    private void ResetBorder()
    {
        var size = (int)(Scale * DefaultParameters.BorderMultiplier);
        Borders = new Borders
        {
            Top = size,
            Bottom = size,
            Left = size,
            Right = size
        };
    }

    public void UpdateSize(bool resize = true)
    {
        if (resize)
        {
            WindowWidth = (int)(Width * Scale) + Borders.Left + Borders.Right;
            WindowHeight = (int)(Height * Scale) + Borders.Top + Borders.Bottom;
            SDL_SetWindowSize(Window, WindowWidth, WindowHeight);
        }

        EventEmitter.RaiseScreenSizeChange();
    }

    public void Run()
    {
        Initialize();

#if DEBUG
        SDL_SetHint(SDL_HINT_WINDOWS_DISABLE_THREAD_NAMING, "1");
#endif

        if (SDL_Init(SDL_INIT_EVERYTHING) != 0)
        {
            Console.WriteLine(SDL_GetError());
            return;
        }

        Window = SDL_CreateWindow("Capy64 " + Version,
            SDL_WINDOWPOS_CENTERED, SDL_WINDOWPOS_CENTERED,
            WindowWidth, WindowHeight,
            SDL_WindowFlags.SDL_WINDOW_OPENGL);

        if (Window == nint.Zero)
        {
            Console.WriteLine(SDL_GetError());
            return;
        }

        var icon = SDL_LoadBMP("./Icon.bmp");
        if (icon != 0)
            SDL_SetWindowIcon(Window, icon);

        Renderer = SDL_CreateRenderer(Window,
            0,
            SDL_RendererFlags.SDL_RENDERER_ACCELERATED | SDL_RendererFlags.SDL_RENDERER_PRESENTVSYNC);

        if (Renderer == nint.Zero)
        {
            Console.WriteLine(SDL_GetError());
            return;
        }

        VideoSurface = SDL_CreateRGBSurfaceWithFormat(0, Width, Height, 32, SDL_PIXELFORMAT_ARGB8888);
        SurfaceRenderer = SDL_CreateSoftwareRenderer(VideoSurface);

        SDL_SetRenderDrawColor(SurfaceRenderer, 0, 0, 0, 255);
        SDL_RenderClear(SurfaceRenderer);

        EventEmitter.RaiseInit();

        ulong deltaEnd = 0;
        var perfFreq = SDL_GetPerformanceFrequency();

        ulong totalTicks = 0;

        var drawTimer = new Timer(TimeSpan.FromMilliseconds(1000d / Framerate));
        drawTimer.Elapsed += (s, e) =>
        {
            SDL_SetRenderDrawColor(Renderer, BorderColor.R, BorderColor.G, BorderColor.B, 255);
            SDL_RenderClear(Renderer);


            SDL_SetRenderDrawColor(Renderer, 255, 255, 255, 255);
            var texture = SDL_CreateTextureFromSurface(Renderer, VideoSurface);

            SDL_SetRenderDrawBlendMode(Renderer, SDL_BlendMode.SDL_BLENDMODE_BLEND);

            SDL_RenderCopy(Renderer, texture, 0, ref _outputRect);
            SDL_DestroyTexture(texture);

            SDL_RenderPresent(Renderer);
        };

        var updateTimer = new Timer(TimeSpan.FromMilliseconds(1000d / Tickrate));
        updateTimer.Elapsed += (s, e) => {
            // Register user input
            //_inputManager.Update(true);

            EventEmitter.RaiseTick(new()
            {
                GameTime = null,
                TotalTicks = totalTicks,
                IsActiveTick = true,
            });

            totalTicks++;
        };

        drawTimer.Start();
        updateTimer.Start();
        _running = true;
        while (_running)
        {
            var deltaStart = SDL_GetPerformanceCounter();
            var delta = deltaStart - deltaEnd;

            while (SDL_WaitEvent(out var ev) != 0)
            {
                ProcessEvent(ev);
            }

            if ((uint)(perfFreq / delta) < Tickrate)
            {

            }

            if ((uint)(perfFreq / delta) < Framerate)
            {
                deltaEnd = deltaStart;


            }
        }
    }

    private void ProcessEvent(SDL_Event ev)
    {
        switch (ev.type)
        {
            case SDL_EventType.SDL_QUIT:
                _running = false;
                Dispose();
                break;

            case SDL_EventType.SDL_KEYUP:
            case SDL_EventType.SDL_KEYDOWN:
                break;

            case SDL_EventType.SDL_MOUSEMOTION:
                _inputManager.UpdateMouseMove(ev);
                break;

            case SDL_EventType.SDL_MOUSEBUTTONUP:
            case SDL_EventType.SDL_MOUSEBUTTONDOWN:
                _inputManager.UpdateMouseClick(ev);
                break;

            case SDL_EventType.SDL_MOUSEWHEEL:
                _inputManager.UpdateMouseScroll(ev);
                break;
        }
    }

    public void Dispose()
    {
        SDL_DestroyRenderer(SurfaceRenderer);
        //SDL_FreeSurface(VideoSurface);
        SDL_DestroyRenderer(Renderer);
        SDL_DestroyWindow(Window);
        SDL_Quit();
    }
}