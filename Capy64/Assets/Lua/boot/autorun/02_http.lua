local http = require("http")
local event = require("event")
local expect = require("expect").expect


function http.request(url, body, headers, options)
    expect(1, url, "string")
    expect(2, body, "string", "nil")
    expect(3, headers, "table", "nil")
    expect(4, options, "table", "nil")

    if not http.checkURL(url) then
        return nil, "Invalid URL"
    end

    local requestId = http.requestAsync(url, body, headers, options)
    local ev, id, data, info
    repeat
        ev, id, data, info = event.pull("http_response", "http_failure")
    until id == requestId

    if ev == "http_failure" then
        return nil, data
    end

    return data, info
end

function http.get(url, headers, options)
    expect(1, url, "string")
    expect(2, headers, "table", "nil")
    expect(3, options, "table", "nil")

    return http.request(url, nil, headers, options)
end

function http.post(url, body, headers, options)
    expect(1, url, "string")
    expect(2, body, "string", "nil")
    expect(3, headers, "table", "nil")
    expect(4, options, "table", "nil")

    return http.request(url, body, headers, options)
end

local WebSocketHandle
local function buildWebsocketHandle(handle)
    if not WebSocketHandle then
        WebSocketHandle = getmetatable(handle) or { __index = {} }
        function WebSocketHandle.__index:close()
            self:closeAsync()
            local _, id
            repeat
                _, id = event.pull("websocket_close")
            until id == self:getRequestID()
        end

        function WebSocketHandle.__index:receive()
            local _, id, par
            repeat
                _, id, par = event.pull("websocket_message")
            until id == self:getRequestID()

            return par
        end
    end

    return handle
end

function http.websocket(url, headers)
    expect(1, url, "string")
    expect(2, headers, "table", "nil")

    if not http.checkURL(url) then
        return nil, "Invalid URL"
    end

    local requestId = http.websocketAsync(url, headers)
    local ev, id, par
    repeat
        ev, id, par = event.pull("websocket_connect", "websocket_failure")
    until id == requestId

    if ev == "http_failure" then
        return nil, par
    end

    return buildWebsocketHandle(par)
end
