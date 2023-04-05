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

    local task<close> = http.requestAsync(url, body, headers, options)
    return task:await()
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
    if not handle then
        return nil
    end
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

    local task<close> = http.websocketAsync(url, headers)
    local client, err = task:await()

    return buildWebsocketHandle(client), err
end
