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
        state.PushString("shutdown");
        state.PushCFunction(L_Shutdown);
        state.SetTable(-3);
    }

    private static int L_Shutdown(IntPtr state)
    {
        BIOS.Bios.Shutdown();

        return 0;
    }
}
