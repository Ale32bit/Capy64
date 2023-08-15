local version = "0.1.1"
local systemDirectory = "/sys"

print("Starting CapyOS")

local term = require("term")
local fs = require("fs")
local machine = require("machine")

local nPrint = print
local function showError(err)
    nPrint(err)
    local x, y = term.getPos()
    term.setForeground(0xff0000)
    term.setPos(1, y)
    term.write(err)

    term.setPos(1, y + 1)
    term.setForeground(0xffffff)
    term.write("Press any key to continue")

    coroutine.yield("key_down")
end

function os.version()
    return "CapyOS " .. version
end

term.setPos(1, 1)
term.write(machine.version())
term.setPos(1, 3)

local files = fs.list(fs.combine(systemDirectory, "boot/autorun"))
for i = 1, #files do
    local func, err = loadfile(fs.combine(systemDirectory, "boot/autorun", files[i]))
    if not func then
        showError(err)
        break
    end

    local ok, err = pcall(func)
    if not ok then
        showError(debug.traceback(err))
        break
    end
end
