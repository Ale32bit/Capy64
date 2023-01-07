local event = {}

function event.pull(...)
    local pars = table.pack(event.pullRaw(...))
    if pars[1] == "interrupt" then
        error("Interrupted", 0)
    end
    return table.unpack(pars)
end

function event.pullRaw(...)
    return coroutine.yield(...)
end

return event