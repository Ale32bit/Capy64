using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Libraries;

public class OS : IPlugin
{
    private static IGame _game;
    public OS(IGame game)
    {
        _game = game;
    }


    public void LuaInit(Lua state)
    {
        state.GetGlobal("os");

        state.PushString("version");
        state.PushCFunction(L_Version);
        state.SetTable(-3);

        state.PushString("shutdown");
        state.PushCFunction(L_Shutdown);
        state.SetTable(-3);
    }

    private static int L_Version(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        L.PushString(Capy64.Version);

        return 1;
    }

    private static int L_Shutdown(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var doReboot = false;
        if (L.IsBoolean(1))
        {
            doReboot = L.ToBoolean(1);
        }

        if (doReboot)
        {
            BIOS.Bios.Reboot();
        }
        else
        {
            BIOS.Bios.Shutdown();
        }

        return 0;
    }
}
