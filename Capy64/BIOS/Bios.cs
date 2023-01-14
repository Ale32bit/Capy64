using Capy64.API;
using Capy64.Core;
using Capy64.Eventing;
using Capy64.Eventing.Events;
using Capy64.LuaRuntime;
using Capy64.LuaRuntime.Libraries;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Capy64.BIOS;

public class Bios : IPlugin
{
    private static IGame _game;
    private readonly EventEmitter _eventEmitter;
    private RuntimeInputEvents _runtimeInputEvents;
    private readonly Drawing _drawing;
    private static bool CloseRuntime = false;
    private static bool OpenBios = false;

    public Bios(IGame game)
    {
        _game = game;
        _eventEmitter = game.EventEmitter;
        _drawing = game.Drawing;
        _game.EventEmitter.OnInit += OnInit;
        _eventEmitter.OnTick += OnTick;
    }

    private void OnInit(object sender, EventArgs e)
    {
        RunBIOS();
    }

    private void OnTick(object sender, TickEvent e)
    {
        if (CloseRuntime)
        {
            _runtimeInputEvents.Unregister();
            _game.LuaRuntime.Close();
            CloseRuntime = false;

            if (OpenBios)
            {
                OpenBios = false;
                RunBIOS();
            }
            else
            {
                StartLuaOS();
            }
        }

        Resume();
    }

    private void RunBIOS()
    {
        _game.LuaRuntime = new();
        InitLuaPlugins();

        _game.LuaRuntime.Thread.PushCFunction(L_OpenDataFolder);
        _game.LuaRuntime.Thread.SetGlobal("openDataFolder");

        _game.LuaRuntime.Thread.PushCFunction(L_InstallOS);
        _game.LuaRuntime.Thread.SetGlobal("installOS");

        _game.LuaRuntime.Thread.PushCFunction(L_Exit);
        _game.LuaRuntime.Thread.SetGlobal("exit");


        var status = _game.LuaRuntime.Thread.LoadFile("Assets/bios.lua");
        if (status != LuaStatus.OK)
        {
            throw new LuaException(_game.LuaRuntime.Thread.ToString(-1));
        }

        _runtimeInputEvents = new RuntimeInputEvents(_eventEmitter, _game.LuaRuntime);
        _runtimeInputEvents.Register();
    }

    private void StartLuaOS()
    {
        InstallOS();

        try
        {
            _game.LuaRuntime = new Runtime();
            InitLuaPlugins();
            _game.LuaRuntime.Patch();
            _game.LuaRuntime.Init();
            _runtimeInputEvents = new(_eventEmitter, _game.LuaRuntime);
            _runtimeInputEvents.Register();

        }
        catch (LuaException ex)
        {
            var panic = new PanicScreen(_game.Drawing);
            _drawing.Begin();
            panic.Render("Cannot load operating system!", ex.Message);
            _drawing.End();
        }
    }

    public void Resume()
    {
        try
        {
            var yielded = _game.LuaRuntime.Resume();
            if (!yielded)
            {
                _game.Exit();
            }
        }
        catch (LuaException e)
        {
            Console.WriteLine(e);
            var panic = new PanicScreen(_game.Drawing);
            panic.Render(e.Message);
            _runtimeInputEvents.Unregister();
        }
    }

    private static void InitLuaPlugins()
    {
        var allPlugins = new List<IPlugin>(_game.NativePlugins);
        allPlugins.AddRange(_game.Plugins);
        foreach (var plugin in allPlugins)
        {
            plugin.LuaInit(_game.LuaRuntime.Thread);
        }
    }

    public static void InstallOS(bool force = false)
    {
        var installedFilePath = Path.Combine(Capy64.AppDataPath, ".installed");
        if (!File.Exists(installedFilePath) || force)
        {
            FileSystem.CopyDirectory("Assets/Lua", FileSystem.DataPath, true, true);
            File.Create(installedFilePath).Dispose();
        }
    }

    public static void Shutdown()
    {
        _game.Exit();
    }

    public static void Reboot()
    {
        CloseRuntime = true;
        OpenBios = true;
    }

    private int L_OpenDataFolder(IntPtr state)
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

    private int L_InstallOS(IntPtr state)
    {
        InstallOS(true);
        return 0;
    }

    private int L_Exit(IntPtr state)
    {
        CloseRuntime = true;
        return 0;
    }
}
