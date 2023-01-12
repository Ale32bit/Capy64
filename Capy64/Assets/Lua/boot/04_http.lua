local http = require("http")
local event = require("event")

function http.request(url, body, headers, options)
    local requestId = http.requestAsync(url, body, headers, options)
    local _, response
    repeat
        _, id, response = event.pull("http_response")
    until id == requestId
    return response
end

function http.get(url, headers, options)
    return http.request(url, nil, headers, options)
end

function http.post(url, body, headers, options)
    return http.request(url, body, headers, options)
end