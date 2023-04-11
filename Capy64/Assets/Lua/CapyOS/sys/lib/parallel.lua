local expect = require("expect")

local parallel = {}

local function contains(array, value)
    for k, v in pairs(array) do
        if v == value then
            return true
        end
    end
    return false
end

local function run(threads, exitOnAny)
    local alive = #threads
    local filters = {}
    local ev = {}
    while true do
        for i, thread in pairs(threads) do
            if not filters[i] or #filters[i] == 0 or contains(filters[i], ev[1]) or ev[1] == "interrupt" then
                local pars = table.pack(coroutine.resume(thread, table.unpack(ev)))
                if pars[1] then
                    filters[i] = table.pack(table.unpack(pars, 2))
                else
                    error(pars[2], 0)
                end
            end

            if coroutine.status(thread) == "dead" then
                alive = alive - 1
                if exitOnAny or alive <= 0 then
                    return
                end
            end
        end

        ev = table.pack(coroutine.yield())
    end
end

function parallel.waitForAll(...)
    local threads = {}
    for k, v in ipairs({ ... }) do
        expect(k, v, "function")
        table.insert(threads, coroutine.create(v))
    end

    return run(threads, false)
end

function parallel.waitForAny(...)
    local threads = {}
    for k, v in ipairs({ ... }) do
        expect(k, v, "function")
        table.insert(threads, coroutine.create(v))
    end

    return run(threads, true)
end

return parallel
