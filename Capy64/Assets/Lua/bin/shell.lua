local term = require("term")
local colors = require("colors")
local io = require("io")
local fs = require("fs")

local exit = false
local shell = {}

shell.path = "./?.lua;./?;/bin/?.lua"
shell.homePath = "/home"

local currentDir = shell.homePath

local function buildEnvironment()
    return setmetatable({
        shell = shell,
    }, { __index = _G })
end

local function printError(...)
    local cfg = {term.getForeground()}
    local cbg = {term.getBackground()}

    term.setForeground(colors.red)
    term.setBackground(colors.black)
    print(...)
    term.setForeground(table.unpack(cfg))
    term.setBackground(table.unpack(cbg))
end

local function tokenise(...)
    local sLine = table.concat({ ... }, " ")
    local tWords = {}
    local bQuoted = false
    for match in string.gmatch(sLine .. "\"", "(.-)\"") do
        if bQuoted then
            table.insert(tWords, match)
        else
            for m in string.gmatch(match, "[^ \t]+") do
                table.insert(tWords, m)
            end
        end
        bQuoted = not bQuoted
    end
    return tWords
end

function shell.getDir()
    return currentDir
end

function shell.setDir(path)
    currentDir = path
end

function shell.resolve(path)
    if path:sub(1, 1) == "/" then
        return path
    end

    for seg in shell.path:gmatch("[^;]+") do
        local resolved = seg:gsub("%?", path)
        if fs.exists(resolved) then
            return resolved
        end
    end
end

function shell.run(...)
    local args = tokenise(...)
    local command = args[1]
    local path = shell.resolve(command)

    if not path then
        printError("Command not found: " .. command)
        return false
    end

    local env = buildEnvironment()

    local func, err = loadfile(path, "t", env)

    if not func then
        printError(err)
        return false
    end

    local ok, err = pcall(func, table.unpack(args, 2))
    if not ok then
        printError(err)
        return false
    end

    return true
end

function shell.exit()
    exit = true
end

local history = {}
local lastExecSuccess = true
while not exit do
    term.setForeground(colors.white)
    write(":")
    term.setForeground(colors.lightBlue)
    local currentDir = shell.getDir()
    if currentDir == shell.homePath then
        write("~")
    else
        write(currentDir)
    end
    
    if lastExecSuccess then
        term.setForeground(colors.yellow)
    else
        term.setForeground(colors.red)
    end
    write("$ ")

    term.setForeground(colors.white)
    local line = io.read(nil, history)

    if line:match("%S") and history[#history] ~= line then
        table.insert(history, line)
    end

    if line:match("%S") then
        lastExecSuccess = shell.run(line)
    end
end
