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
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Net.WebSockets;
using System.Threading;

namespace Capy64.Runtime.Objects;

public class WebSocketClient : IComponent
{
    public const string ObjectType = "WebSocketClient";

    public record Client(ClientWebSocket Socket, long RequestId);

    internal static LuaRegister[] Methods = new LuaRegister[]
    {
        new()
        {
            name = "getRequestID",
            function = L_GetRequestId,
        },
        new()
        {
            name = "send",
            function = L_Send,
        },
        new()
        {
            name = "closeAsync",
            function = L_CloseAsync,
        },
        new(),
    };

    internal static LuaRegister[] MetaMethods = new LuaRegister[]
    {
        new()
        {
            name = "__index",
        },
        new()
        {
            name = "__gc",
            function = L_CloseAsync,
        },
        new()
        {
            name = "__close",
            function = L_CloseAsync,
        },
        new()
        {
            name = "__tostring",
            function = LM_ToString,
        },

        new(),
    };

    public WebSocketClient(Game _) { }

    public void LuaInit(Lua L)
    {
        CreateMeta(L);
    }

    public static void CreateMeta(Lua L)
    {
        L.NewMetaTable(ObjectType);
        L.SetFuncs(MetaMethods, 0);
        L.NewLibTable(Methods);
        L.SetFuncs(Methods, 0);
        L.SetField(-2, "__index");
        L.Pop(1);
    }

    public static Client ToObject(Lua L, bool gc = false)
    {
        return ObjectManager.ToObject<Client>(L, 1, gc);
    }

    public static Client CheckObject(Lua L, bool gc = false)
    {
        var obj = ObjectManager.CheckObject<Client>(L, 1, ObjectType, gc);
        if (obj is null)
        {
            L.Error("attempt to use a closed object");
            return null;
        }
        return obj;
    }

    private static int L_GetRequestId(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = CheckObject(L, false);

        L.PushInteger(client.RequestId);

        return 1;
    }

    private static int L_Send(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = CheckObject(L, false);

        var data = L.CheckBuffer(2);

        if (client is null || client.Socket.State == WebSocketState.Closed)
            L.Error("connection is closed");

        client.Socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

        return 0;
    }

    private static int L_CloseAsync(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = ToObject(L, true);

        if (client is null || client.Socket.State == WebSocketState.Closed)
            return 0;

        client.Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None)
            .ContinueWith(async task =>
            {
                await task;
                LegacyEntry.Instance.LuaRuntime.QueueEvent("websocket_close", LK =>
                {
                    LK.PushInteger(client.RequestId);

                    return 1;
                });
            });

        HTTPLib.WebSocketConnections.Remove(client);

        return 0;
    }

    private static unsafe int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var buffer = ToObject(L);
        if (buffer is not null)
        {
            L.PushString("WebSocket ({0:X})", (ulong)&buffer);
        }
        else
        {
            L.PushString("WebSocket (closed)");
        }
        return 1;
    }
}
