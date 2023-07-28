local term = require("term")
local io = require("io")
local colors = require("colors")
local argparser = require("argparser")
local tableutils = require("tableutils")

local args, options = argparser.parse(...)

local function evaluate(str, env, chunkname)
    chunkname = chunkname or "=lua"
    local nForcePrint = 0
    local func, e = load(str, chunkname, "t", env)
    local func2 = load("return " .. str, chunkname, "t", env)
    if not func then
        if func2 then
            func = func2
            e = nil
            nForcePrint = 1
        end
    else
        if func2 then
            func = func2
        end
    end

    if func then
        local tResults = table.pack(pcall(func))
        if tResults[1] then
            local n = 1
            while n < tResults.n or n <= nForcePrint do
                local value = tResults[n + 1]
                print(tableutils.pretty(value))
                n = n + 1
            end
        else
            io.stderr.print(tResults[2])
            return false
        end
    else
        io.stderr.print(e)
        return false
    end
    return true
end

local function createEnvironment()
    return setmetatable({}, { __index = _ENV })
end

local function loadPackages(env)
    for k, v in pairs(package.loaded) do
        env[k] = v
    end
end

if options.e then
    local env = createEnvironment()
    loadPackages(env)
    return evaluate(table.concat(args, " "), env)
end

if #args > 0 then
    print("This is an interactive Lua prompt.")
    print("To run a lua program, just type its name.")
    return
end

--local pretty = require "cc.pretty"

local bRunning = true
local tCommandHistory = {}


local tEnv = createEnvironment()
tEnv.exit = setmetatable({}, {
    __tostring = function() return "Call exit() to exit." end,
    __call = function() bRunning = false end,
})
loadPackages(tEnv)

term.setForeground(colors.yellow)
print(_VERSION .. " interactive prompt")
print("Call exit() to exit.")
term.setForeground(colors.white)

while bRunning do
    term.setForeground(colors.yellow)
    io.write("> ")
    term.setForeground(colors.white)

    local s = io.read(nil, tCommandHistory)
    if s:match("%S") and tCommandHistory[#tCommandHistory] ~= s then
        table.insert(tCommandHistory, s)
    end

    evaluate(s, tEnv)

end
