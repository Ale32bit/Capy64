using Capy64.LuaRuntime.Libraries;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Capy64.LuaRuntime.Handlers;

public class WebSocketHandle : IHandle
{
    private ClientWebSocket _client;
    private long _requestId;
    private static IGame _game;
    public WebSocketHandle(ClientWebSocket client, long requestId, IGame game)
    {
        _client = client;
        _requestId = requestId;
        _game = game;
    }

    private static readonly Dictionary<string, LuaFunction> functions = new()
    {
        ["send"] = L_Send,
        ["closeAsync"] = L_CloseAsync,
    };

    public void Push(Lua L, bool newTable = true)
    {
        if (newTable)
            L.NewTable();

        // metatable
        L.NewTable();
        L.PushString("__close");
        L.PushCFunction(L_CloseAsync);
        L.SetTable(-3);
        L.PushString("__gc");
        L.PushCFunction(L_CloseAsync);
        L.SetTable(-3);
        L.SetMetaTable(-2);

        foreach (var pair in functions)
        {
            L.PushString(pair.Key);
            L.PushCFunction(pair.Value);
            L.SetTable(-3);
        }

        L.PushString("_handle");
        L.PushObject(this);
        L.SetTable(-3);
    }

    private static WebSocketHandle GetHandle(Lua L, bool gc = true)
    {
        L.CheckType(1, LuaType.Table);
        L.PushString("_handle");
        L.GetTable(1);
        return L.ToObject<WebSocketHandle>(-1, gc);
    }

    private static int L_Send(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var data = L.CheckBuffer(2);

        var h = GetHandle(L, false);

        if (h is null || h._client.State == WebSocketState.Closed)
            L.Error("connection is closed");

        h._client.SendAsync(data, WebSocketMessageType.Text, true, CancellationToken.None);

        return 0;
    }

    private static int L_CloseAsync(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var h = GetHandle(L, true);

        if (h is null || h._client.State == WebSocketState.Closed)
            return 0;

        h._client.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None)
            .ContinueWith(async task =>
            {
                await task;
                _game.LuaRuntime.PushEvent("websocket_close", h._requestId);
            });

        HTTP.WebSocketConnections.Remove(h);

        return 0;
    }
}
