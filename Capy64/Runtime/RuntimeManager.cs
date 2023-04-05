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
using Capy64.Eventing.Events;
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Diagnostics;
using System.IO;

namespace Capy64.Runtime;

internal class RuntimeManager : IComponent
{
    private LuaState luaState;
    private EventEmitter emitter;

    private static int step = 0;
    private static bool close = false;
    private static bool inPanic = false;

    private static IGame _game;
    public RuntimeManager(IGame game)
    {
        _game = game;

        _game.EventEmitter.OnInit += OnInit;
        _game.EventEmitter.OnTick += OnTick;
    }

    /// <summary>
    /// Start Capy64 state.
    /// First BIOS, then OS
    /// </summary>
    private void Start()
    {
        emitter?.Unregister();

        luaState?.Dispose();
        luaState = null;
        close = false;

        switch (step++)
        {
            case 0:
                InitBIOS();
                break;
            case 1:
                InitOS();
                break;
            default:
                step = 0;
                Start();
                break;
        }
    }

    private void Resume()
    {
        if (inPanic) return;
        try
        {
            if (close || !luaState.ProcessQueue())
            {
                _game.EventEmitter.RaiseClose();
                Start();
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            inPanic = true;
            close = true;
            PanicScreen.Render(e.Message);
        }
    }

    public static void ResetPanic()
    {
        inPanic = false;
    }

    private void InitBIOS()
    {
        _game.Discord.SetPresence("Booting up...");

        InstallOS(false);

        luaState = new LuaState();
        _game.LuaRuntime = luaState;
        luaState.Init();

        emitter = new(_game.EventEmitter, luaState);

        luaState.QueueEvent("init", LK =>
        {
            LK.PushInteger(step);
            return 1;
        });

        emitter.Register();

        luaState.Thread.PushCFunction(L_OpenDataFolder);
        luaState.Thread.SetGlobal("openDataFolder");

        luaState.Thread.PushCFunction(L_InstallOS);
        luaState.Thread.SetGlobal("installOS");

        luaState.Thread.PushCFunction(L_Exit);
        luaState.Thread.SetGlobal("exit");

        var status = luaState.Thread.LoadFile("Assets/Lua/bios.lua");
        if (status != LuaStatus.OK)
        {
            throw new LuaException(luaState.Thread.ToString(-1));
        }
    }

    private void InitOS()
    {
        luaState = new LuaState();
        _game.LuaRuntime = luaState;
        luaState.Init();

        emitter = new(_game.EventEmitter, luaState);

        luaState.QueueEvent("init", LK =>
        {
            LK.PushInteger(step);
            return 1;
        });

        emitter.Register();

        if (!File.Exists(Path.Combine(FileSystem.DataPath, "init.lua")))
        {
            throw new LuaException("Operating System not found\nMissing init.lua");
        }

        var initContent = File.ReadAllText(Path.Combine(FileSystem.DataPath, "init.lua"));
        var status = luaState.Thread.LoadString(initContent, "=init.lua");
        if (status != LuaStatus.OK)
        {
            throw new LuaException(luaState.Thread.ToString(-1));
        }
    }

    public static void Reset()
    {
        close = true;
        step = 0;
    }

    public static void Reboot()
    {
        close = true;
    }

    public static void Shutdown()
    {
        close = true;
        _game.Exit();
    }

    public static void InstallOS(bool force = false)
    {
        var installedFilePath = Path.Combine(Capy64.AppDataPath, ".installed");
        if (!File.Exists(installedFilePath) || force)
        {
            FileSystem.CopyDirectory("Assets/Lua/CapyOS", FileSystem.DataPath, true, true);
            File.Create(installedFilePath).Dispose();
        }
    }

    private static int L_OpenDataFolder(IntPtr state)
    {
        var path = FileSystem.DataPath;
        switch (Environment.OSVersion.Platform)
        {
            case PlatformID.Win32NT:
                Process.Start("explorer.exe", path);
                break;
            case PlatformID.Unix:
                Process.Start("xdg-open", path);
                break;
        }

        return 0;
    }

    private static int L_InstallOS(IntPtr state)
    {
        InstallOS(true);
        return 0;
    }

    private static int L_Exit(IntPtr state)
    {
        close = true;
        return 0;
    }

    private void OnInit(object sender, EventArgs e)
    {
        Start();
    }

    private void OnTick(object sender, TickEvent e)
    {
        if (e.IsActiveTick)
            Resume();
    }
}
