// This file is part of Capy64 - https://github.com/Capy64/Capy64
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
using KeraLua;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
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
        new()
        {
            name = "vibrate",
            function = L_Vibrate,
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

        if (!L.IsNoneOrNil(1))
        {
            var newTitle = L.CheckString(1);
            
            if (string.IsNullOrEmpty(newTitle))
            {
                newTitle = "Capy64 " + Capy64.Version;
            }

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

        try
        {
            Capy64.Instance.Discord.SetPresence(details, dstate);
            L.PushBoolean(true);
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
            L.PushBoolean(false);
        }



        return 1;
    }

    private static int L_Vibrate(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var left = (float)L.CheckNumber(1);
        var right = (float)L.OptNumber(2, left);

        left = Math.Clamp(left, 0, 1);
        right = Math.Clamp(right, 0, 1);

        GamePad.SetVibration(PlayerIndex.One, left, right);

        return 0;
    }
}
