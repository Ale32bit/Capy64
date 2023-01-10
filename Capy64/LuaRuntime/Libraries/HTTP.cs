using Capy64.API;
using KeraLua;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Capy64.LuaRuntime.Libraries;
#nullable enable
public class HTTP : IPlugin
{
    private static IGame _game;
    private static HttpClient _client;
    private static long RequestId;

    private readonly LuaRegister[] HttpLib = new LuaRegister[]
    {
        new()
        {
            name = "request",
            function = L_Request,
        },
        new()
        {
            name = "checkURL",
            function = L_CheckUrl,
        },
        new(),
    };
    public HTTP(IGame game)
    {
        _game = game;
        RequestId = 0;
        _client = new();
        _client.DefaultRequestHeaders.Add("User-Agent", $"Capy64/{Capy64.Version}");
    }

    public void LuaInit(Lua L)
    {
        L.RequireF("http", Open, false);
    }

    private int Open(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);
        L.NewLib(HttpLib);
        return 1;
    }

    public static bool TryGetUri(string url, out Uri? uri)
    {
        return (Uri.TryCreate(url, UriKind.Absolute, out uri)
            && uri?.Scheme == Uri.UriSchemeHttp) || uri?.Scheme == Uri.UriSchemeHttps;
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

        var requestId = RequestId++;

        var reqTask = _client.SendAsync(request);
        reqTask.ContinueWith(async (task) =>
        {
            var response = await task;
            object content;
            if ((bool)options["binary"])
                content = await response.Content.ReadAsByteArrayAsync();
            else
                content = await response.Content.ReadAsStringAsync();

            // wip
            /*
            _game.LuaRuntime.PushEvent("http_response", L =>
            {
                L.PushInteger(requestId);

                L.NewTable();

                L.PushString("success");
                L.PushBoolean(response.IsSuccessStatusCode);
                L.SetTable(-3);

                return 2;
            });*/

            _game.LuaRuntime.PushEvent("http_response", requestId, response.IsSuccessStatusCode, content, (int)response.StatusCode, response.ReasonPhrase);
        });

        L.PushInteger(requestId);

        return 1;
    }

    private static int L_CheckUrl(IntPtr state)
    {
        var L = Lua.FromIntPtr(state);

        var url = L.CheckString(1);

        var isValid = TryGetUri(url, out _);

        L.PushBoolean(isValid);

        return 1;
    }
}
