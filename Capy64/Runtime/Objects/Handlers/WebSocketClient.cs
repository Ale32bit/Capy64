using Capy64.Runtime.Libraries;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Threading;

namespace Capy64.Runtime.Objects.Handlers;

public class WebSocketClient
{
    public const string ObjectType = "WebSocketClient";
    private readonly ClientWebSocket _socket;
    private readonly long _requestId;
    public WebSocketClient(ClientWebSocket socket, long requestId)
    {
        _socket = socket;
        _requestId = requestId;
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["getRequestID"] = L_GetRequestId,
        ["send"] = L_Send,
        ["closeAsync"] = L_CloseAsync,
    };

    public void Push(Lua L)
    {
        L.PushObject(this);

        if (L.NewMetaTable(ObjectType))
        {
            L.PushString("__index");
            L.NewTable();
            foreach (var pair in functions)
            {
                L.PushString(pair.Key);
                L.PushCFunction(pair.Value);
                L.SetTable(-3);
            }
            L.SetTable(-3);

            L.PushString("__close");
            L.PushCFunction(L_CloseAsync);
            L.SetTable(-3);

            L.PushString("__gc");
            L.PushCFunction(L_CloseAsync);
            L.SetTable(-3);
        }

        L.SetMetaTable(-2);
    }

    private static int L_GetRequestId(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = L.CheckObject<WebSocketClient>(1, ObjectType, false);

        L.PushInteger(client._requestId);

        return 1;
    }

    private static int L_Send(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = L.CheckObject<WebSocketClient>(1, ObjectType, false);

        var data = L.CheckBuffer(2);

        if (client is null || client._socket.State == WebSocketState.Closed)
            L.Error("connection is closed");

        client._socket.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

        return 0;
    }

    private static int L_CloseAsync(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var client = L.CheckObject<WebSocketClient>(1, ObjectType, true);

        if (client is null || client._socket.State == WebSocketState.Closed)
            return 0;

        client._socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None)
            .ContinueWith(async task =>
            {
                await task;
                Capy64.Instance.LuaRuntime.QueueEvent("websocket_close", LK =>
                {
                    LK.PushInteger(client._requestId);

                    return 1;
                });
            });

        HTTP.WebSocketConnections.Remove(client);

        return 0;
    }
}
