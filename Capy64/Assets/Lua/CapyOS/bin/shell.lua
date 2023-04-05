local term = require("term")
local colors = require("colors")
local fs = require("fs")
local machine = require("machine")

local exit = false
local shell = {}

shell.path = "./?;./?.lua;/bin/?.lua"
shell.homePath = "/home"

local currentDir = shell.homePath

local function buildEnvironment(path, args)
    local arg = { table.unpack(args, 2) }
    arg[0] = path
    
    return setmetatable({
        shell = shell,
        arg = arg
    }, { __index = _G })
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
        return fs.combine("", path)
    end

    if path:sub(1, 1) == "~" then
        return fs.combine(shell.homePath, path)
    end

    return fs.combine(currentDir, path)
end

function shell.resolveProgram(path)
    if path:sub(1, 1) == "/" then
        return shell.resolve(path)
    end

    for seg in shell.path:gmatch("[^;]+") do
        local resolved = shell.resolve(seg:gsub("%?", path))
        if fs.exists(resolved) and not fs.isDir(resolved) then
            return resolved
        end
    end
end

function shell.run(...)
    local args = tokenise(...)
    local command = args[1]
    local path = shell.resolveProgram(command)

    if not path then
        io.stderr.print("Command not found: " .. command)
        return false
    end

    local env = buildEnvironment(command, args)

    local func, err = loadfile(path, "t", env)

    if not func then
        io.stderr.print(err)
        return false
    end

    local ok, err = pcall(func, table.unpack(args, 2))
    if not ok then
        io.stderr.print(err)
        return false
    end

    return true
end

function shell.exit()
    exit = true
end

if not fs.exists(shell.homePath) then
    fs.makeDir(shell.homePath)
end

local history = {}
local lastExecSuccess = true
while not exit do
    machine.setRPC(os.version(), "On shell")

    term.setBackground(colors.black)
    term.setForeground(colors.white)
    io.write(":")
    term.setForeground(colors.lightBlue)
    if currentDir == shell.homePath then
        io.write("~")
    else
        io.write(currentDir)
    end

    if lastExecSuccess then
        term.setForeground(colors.yellow)
    else
        term.setForeground(colors.red)
    end
    io.write("$ ")

    term.setForeground(colors.white)
    local line = io.read(nil, history)

    if line:match("%S") and history[#history] ~= line then
        table.insert(history, line)
    end

    if line:match("%S") then
        machine.setRPC(os.version(), "Running: " .. line)
        lastExecSuccess = shell.run(line)
    end
end
