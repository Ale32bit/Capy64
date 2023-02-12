﻿using Capy64.API;
using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

namespace Capy64.Runtime.Objects;

public class WebSocketClient : IPlugin
{
    public const string ObjectType = "WebSocketClient";

    public record Client(ClientWebSocket Socket, long RequestId);

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

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["getRequestID"] = L_GetRequestId,
        ["send"] = L_Send,
        ["closeAsync"] = L_CloseAsync,
    };

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
                Capy64.Instance.LuaRuntime.QueueEvent("websocket_close", LK =>
                {
                    LK.PushInteger(client.RequestId);

                    return 1;
                });
            });

        HTTP.WebSocketConnections.Remove(client);

        return 0;
    }

    private static unsafe int LM_ToString(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        var buffer = ToObject(L);
        if (buffer is not null)
        {
            L.PushString("GPUBuffer ({0:X})", (ulong)&buffer);
        }
        else
        {
            L.PushString("GPUBuffer (closed)");
        }
        return 1;
    }
}
