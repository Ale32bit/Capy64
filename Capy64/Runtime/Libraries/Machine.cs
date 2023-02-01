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

    // todo list
    // shutdown, reboot
    // set, get title
    // get proper version function
    // set discord rpc
    // 
    private static LuaRegister[] MachineLib = new LuaRegister[]
    {
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


}
