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
using Capy64.Runtime.Extensions;
using Capy64.Runtime.Objects;
using KeraLua;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading;

namespace Capy64.Runtime.Libraries;
#nullable enable
public class HTTP : IComponent
{
    private static IGame _game;
    private static HttpClient _httpClient;
    private static long _requestId;
    public static readonly HashSet<WebSocketClient.Client> WebSocketConnections = new();

    public static readonly string UserAgent = $"Capy64/{Capy64.Version}";

    private static IConfiguration _configuration;
    private readonly LuaRegister[] HttpLib = new LuaRegister[]
    {
        new()
        {
            name = "checkURL",
            function = L_CheckUrl,
        },
        new()
        {
            name = "requestAsync",
            function = L_Request,
        },
        new()
        {
            name = "websocketAsync",
            function = L_WebsocketAsync,
        },
        new(),
    };
    public HTTP(IGame game, IConfiguration configuration)
    {
        _game = game;
        _requestId = 0;
        _httpClient = new();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", UserAgent);
        _configuration = configuration;
    }

    public void LuaInit(Lua L)
    {
        if (_configuration.GetValue<bool>("HTTP:Enable"))
            L.RequireF("http", Open, false);
    }

    private int Open(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(HttpLib);
        return 1;
    }

    private static readonly string[] _allowedSchemes = new[]
    {
        Uri.UriSchemeHttp,
        Uri.UriSchemeHttps,
        Uri.UriSchemeWs,
        Uri.UriSchemeWss,
    };
    public static bool TryGetUri(string url, out Uri uri)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out uri!) && _allowedSchemes.Contains(uri.Scheme);
    }

    private static int L_CheckUrl(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var url = L.CheckString(1);

        var isValid = TryGetUri(url, out _);

        L.PushBoolean(isValid);

        return 1;
    }

    private static int L_Request(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var request = new HttpRequestMessage();

        var url = L.CheckString(1);
        if (!TryGetUri(url, out Uri? uri) || uri is null)
        {
            L.ArgumentError(1, "invalid request url");
            return 0;
        }
        request.RequestUri = uri;

        if (L.IsTable(3)) // headers
        {
            L.PushCopy(3);
            L.PushNil();

            while (L.Next(-2))
            {
                L.PushCopy(-2);

                var k = L.CheckString(-1);
                if (L.IsStringOrNumber(-2))
                {
                    var v = L.ToString(-2);

                    request.Headers.Add(k, v);
                }
                else if (L.IsNil(-2))
                {
                    request.Headers.Remove(k);
                }
                else
                {
                    L.ArgumentError(3, "string, number or nil expected, got " + L.TypeName(L.Type(-2)) + " in field " + k);
                }

                L.Pop(2);
            }

            L.Pop(1);
        }

        var options = new Dictionary<string, object>
        {
            ["binary"] = false,
        };

        if (L.IsTable(4)) // other options?
        {
            L.PushCopy(4);
            L.PushNil();

            while (L.Next(-2))
            {
                L.PushCopy(-2);
                var k = L.CheckString(-1);

                switch (k)
                {
                    case "method":
                        options["method"] = L.CheckString(-2);
                        break;
                    case "binary":
                        options["binary"] = L.IsBoolean(-2) ? L.ToBoolean(-2) : false;
                        break;
                }

                L.Pop(2);
            }

            L.Pop(1);
        }

        if (!L.IsNoneOrNil(2))
        {
            if ((bool)options["binary"])
            {
                request.Content = new ByteArrayContent(L.CheckBuffer(2));
            }
            else
            {
                request.Content = new StringContent(L.CheckString(2));
            }
        }

        request.Method = options.TryGetValue("method", out var value)
                    ? new HttpMethod((string)value)
                    : request.Content is not null ? HttpMethod.Post : HttpMethod.Get;

        var requestId = _requestId++;

        var reqTask = _httpClient.SendAsync(request);
        reqTask.ContinueWith(async (task) =>
        {

            if (task.IsFaulted || task.IsCanceled)
            {
                _game.LuaRuntime.QueueEvent("http_failure", LK =>
                {
                    LK.PushInteger(requestId);
                    LK.PushString(task.Exception?.Message);

                    return 2;
                });
                return;
            }

            var response = await task;

            var stream = await response.Content.ReadAsStreamAsync();

            _game.LuaRuntime.QueueEvent("http_response", LK =>
            {
                // arg 1, request id
                LK.PushInteger(requestId);

                // arg 2, response data
                ObjectManager.PushObject(L, stream);
                //L.PushObject(stream);
                L.SetMetaTable(FileHandle.ObjectType);
                /*if ((bool)options["binary"])
                    BinaryReadHandle.Push(LK, new(stream));
                else
                    ReadHandle.Push(LK, new(stream));*/

                // arg 3, response info
                LK.NewTable();

                LK.PushString("success");
                LK.PushBoolean(response.IsSuccessStatusCode);
                LK.SetTable(-3);

                LK.PushString("statusCode");
                LK.PushNumber((int)response.StatusCode);
                LK.SetTable(-3);

                LK.PushString("reasonPhrase");
                LK.PushString(response.ReasonPhrase);
                LK.SetTable(-3);

                LK.PushString("headers");
                LK.NewTable();

                foreach (var header in response.Headers)
                {
                    LK.PushString(header.Key);
                    LK.PushArray(header.Value.ToArray());
                    LK.SetTable(-3);
                }

                LK.SetTable(-3);

                return 3;

            });

        });

        L.PushInteger(requestId);

        return 1;
    }

    private static int L_WebsocketAsync(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var wsSettings = _configuration.GetSection("HTTP:WebSockets");

        if (!wsSettings.GetValue<bool>("Enable"))
        {
            L.Error("WebSockets are disabled");
            return 0;
        }

        if (WebSocketConnections.Count >= wsSettings.GetValue<int>("MaxActiveConnections"))
        {
            L.Error("Max connections reached");
            return 0;
        }

        var url = L.CheckString(1);
        if (!TryGetUri(url, out var uri))
        {
            L.ArgumentError(1, "invalid request url");
            return 0;
        }

        var requestId = _requestId++;

        var wsClient = new ClientWebSocket();

        wsClient.Options.SetRequestHeader("User-Agent", UserAgent);

        if (L.IsTable(2)) // headers
        {
            L.PushCopy(2);
            L.PushNil();

            while (L.Next(-2))
            {
                L.PushCopy(-2);

                var k = L.CheckString(-1);
                if (L.IsStringOrNumber(-2))
                {
                    var v = L.ToString(-2);

                    wsClient.Options.SetRequestHeader(k, v);
                }
                else if (L.IsNil(-2))
                {
                    wsClient.Options.SetRequestHeader(k, null);
                }
                else
                {
                    L.ArgumentError(3, "string, number or nil expected, got " + L.TypeName(L.Type(-2)) + " in field " + k);
                }

                L.Pop(2);
            }

            L.Pop(1);
        }


        var connectTask = wsClient.ConnectAsync(uri, CancellationToken.None);
        connectTask.ContinueWith(async task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                _game.LuaRuntime.QueueEvent("websocket_failure", LK =>
                {
                    LK.PushInteger(requestId);
                    LK.PushString(task.Exception?.Message);

                    return 2;
                });
                return;
            }

            await task;

            var handle = new WebSocketClient.Client(wsClient, requestId);
            WebSocketConnections.Add(handle);

            _game.LuaRuntime.QueueEvent("websocket_connect", LK =>
            {
                LK.PushInteger(requestId);

                ObjectManager.PushObject(LK, handle);
                LK.SetMetaTable(WebSocketClient.ObjectType);

                return 2;
            });

            var buffer = new byte[4096];
            var builder = new StringBuilder();
            while (wsClient.State == WebSocketState.Open)
            {
                var result = await wsClient.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await wsClient.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    _game.LuaRuntime.QueueEvent("websocket_close", LK =>
                    {
                        LK.PushInteger(requestId);

                        return 1;
                    });
                    return;
                }
                else
                {
                    var data = Encoding.ASCII.GetString(buffer, 0, result.Count);
                    builder.Append(data);
                }

                if (result.EndOfMessage)
                {
                    var payload = builder.ToString();
                    _game.LuaRuntime.QueueEvent("websocket_message", LK =>
                    {
                        LK.PushInteger(requestId);
                        LK.PushString(payload);

                        return 2;
                    });
                    builder.Clear();
                }
            }
        });

        L.PushInteger(requestId);

        return 1;
    }
}
