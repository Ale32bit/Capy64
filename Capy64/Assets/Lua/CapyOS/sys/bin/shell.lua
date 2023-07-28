local term = require("term")
local colors = require("colors")
local fs = require("fs")
local machine = require("machine")
local argparser = require("argparser")
local scheduler = require("scheduler")

local exit = false
local parentShell = shell
local isStartupShell = parentShell == nil
local shell = {}

shell.path = parentShell and parentShell.path or "./?;./?.lua;/bin/?.lua;/sys/bin/?.lua"
shell.homePath = parentShell and parentShell.home or "/home"
shell.aliases = parentShell and parentShell.aliases or {}

local currentDir = parentShell and parentShell.getDir() or shell.homePath

local function buildEnvironment(path, args, argf)
    local arg = { table.unpack(args, 2) }
    arg[0] = path
    arg.string = argf

    return setmetatable({
        shell = shell,
        arg = arg,
    }, { __index = _G })
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
    local args = argparser.tokenize(...)
    local argf = table.concat({...}, " ")
    local command = args[1]

    argf = argf:sub(#command + 2)

    local path = shell.resolveProgram(command)

    if not path then
        if shell.aliases[command] then
            return shell.run(shell.aliases[command], select(2, table.unpack(args)))
        else
            io.stderr.print("Command not found: " .. command)
            return false
        end
    end

    local env = buildEnvironment(command, args, argf)

    local func, err = loadfile(path, "t", env)

    if not func then
        io.stderr.print(err)
        return false
    end

    local ok, err
    local function run()
        ok, err = pcall(func, table.unpack(args, 2))
        
    end

    local programTask = scheduler.spawn(run)
    coroutine.yield("scheduler_task_end")

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

term.setForeground(colors.white)
term.setBackground(colors.black)

if isStartupShell then
    if fs.exists(fs.combine(shell.homePath, ".shrc")) then
        local f <close> = fs.open(fs.combine(shell.homePath, ".shrc"), "r")
        for line in f:lines() do
            if line:match("%S") and not line:match("^%s-#") then
                shell.run(line)
            end
        end
        f:close()
    end
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
