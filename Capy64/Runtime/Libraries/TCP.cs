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
using KeraLua;
using System.Net.Sockets;

namespace Capy64.Runtime.Libraries;

public class TCP : IComponent
{
    private static int Counter = 0;
    private static IGame _game;
    public TCP(IGame game)
    {
        _game = game;
        Counter = 0;
    }

    private static readonly LuaRegister[] TCPLib = new LuaRegister[]
    {
        new()
        {
            name = "connectAsync",
            function = L_Connect,
        },
        new(),
    };

    public void LuaInit(Lua L)
    {
        //L.RequireF("tcp", OpenLib, false);
    }

    public int OpenLib(nint state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(TCPLib);
        return 1;
    }

    private static int L_Connect(nint state)
    {
        var L = Lua.FromIntPtr(state);

        var host = L.CheckString(1);
        var port = (int)L.CheckInteger(2);
        L.ArgumentCheck(port >= 0 && port <= 0xffff, 2, "port must be in range 0-65535");

        var client = new TcpClient();
        var id = Counter++;
        var task = client.ConnectAsync(host, port);

        task.ContinueWith(t =>
        {
            if (client.Connected)
            {
                _game.LuaRuntime.QueueEvent("tcp_connect", LK =>
                {
                    /*var handle = new FileHandle
                    {
                        Stream = client.GetStream(),
                        DefaultSize = client.ReceiveBufferSize,
                    };

                    LK.PushInteger(id);
                    ObjectManager.PushObject(L, handle);
                    L.SetMetaTable("file");*/
                    return 2;
                });
            }
            else
            {
                _game.LuaRuntime.QueueEvent("tcp_failure", LK =>
                {
                    LK.PushInteger(id);
                    LK.PushString(t.Exception.Message);
                    return 2;
                });
            }
        });

        L.PushInteger(id);

        return 1;
    }

}
