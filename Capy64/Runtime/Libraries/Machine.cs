using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.Runtime.Libraries;

public class Machine : IPlugin
{
    private static IGame _game;
    public Machine(IGame game)
    {
        _game = game;
    }

    private static LuaRegister[] MachineLib = new LuaRegister[]
    {
        new()
        {
            name = "shutdown",
            function = L_Shutdown,
        },
        new()
        {
            name = "reboot",
            function = L_Reboot,
        },
        new()
        {
            name = "title",
            function = L_Title,
        },
        new()
        {
            name = "version",
            function = L_Version,
        },
        new()
        {
            name = "setRPC",
            function = L_SetRPC,
        },
        new(),
    };

    public void LuaInit(Lua L)
    {
        L.RequireF("machine", OpenLib, false);
    }

    private static int OpenLib(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(MachineLib);
        return 1;
    }

    private static int L_Shutdown(IntPtr _)
    {
        RuntimeManager.Shutdown();

        return 0;
    }

    private static int L_Reboot(IntPtr _)
    {
        RuntimeManager.Reboot();

        return 0;
    }

    private static int L_Title(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var currentTitle = Capy64.Instance.Window.Title;

        if(!L.IsNoneOrNil(1))
        {
            var newTitle = L.CheckString(1);

            Capy64.Instance.Window.Title = newTitle;
        }

        L.PushString(currentTitle);

        return 1;
    }

    private static int L_Version(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushString("Capy64 " + Capy64.Version);

        return 1;
    }

    private static int L_SetRPC(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var details = L.CheckString(1);
        var dstate = L.OptString(2, null);

        Capy64.Instance.Discord.SetPresence(details, dstate);

        return 0;
    }

}
