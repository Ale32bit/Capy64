using Capy64.API;
using Capy64.Eventing.Events;
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Capy64.Runtime;

internal class RuntimeManager : IPlugin
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

        luaState = new LuaState();
        _game.LuaRuntime = luaState;

        emitter = new(_game.EventEmitter, luaState);

        luaState.QueueEvent("init", LK =>
        {
            LK.PushInteger(step);
            return 1;
        });

        emitter.Register();

        luaState.Thread.PushCFunction(L_OpenDataFolder);
        luaState.Thread.SetGlobal("openDataFolder");

        luaState.Thread.PushCFunction(L_ShouldInstallOS);
        luaState.Thread.SetGlobal("shouldInstallOS");

        luaState.Thread.PushCFunction(L_FlagInstalled);
        luaState.Thread.SetGlobal("flagInstalled");

        luaState.Thread.PushCFunction(L_Exit);
        luaState.Thread.SetGlobal("exit");

        var status = luaState.Thread.LoadFile("Assets/bios.lua");
        if (status != LuaStatus.OK)
        {
            throw new LuaException(luaState.Thread.ToString(-1));
        }
    }

    private void InitOS()
    {
        _game.Discord.SetPresence("On CapyOS");

        luaState = new LuaState();
        _game.LuaRuntime = luaState;

        emitter = new(_game.EventEmitter, luaState);

        luaState.QueueEvent("init", LK =>
        {
            LK.PushInteger(step);
            return 1;
        });

        emitter.Register();

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

    private static int L_ShouldInstallOS(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var installedFilePath = Path.Combine(Capy64.AppDataPath, ".installed");

        L.PushBoolean(!File.Exists(installedFilePath));

        return 1;
    }

    private static int L_FlagInstalled(IntPtr state)
    {
        var installedFilePath = Path.Combine(Capy64.AppDataPath, ".installed");
        if (!File.Exists(installedFilePath))
            File.Create(installedFilePath).Dispose();

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
        Resume();
    }
}
