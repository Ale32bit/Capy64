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
    local ev, id, par
    repeat
        ev, id, par = event.pull("http_response", "http_failure")
    until id == requestId

    if ev == "http_failure" then
        return nil, par
    end

    return par
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