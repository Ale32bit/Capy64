local version = "0.0.2"

print("Starting CapyOS")

local term = require("term")
local fs = require("fs")
local gpu = require("gpu")

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
term.write(_HOST)

local files = fs.list("/boot/autorun")
for i = 1, #files do
    local func, err = loadfile("/boot/autorun/" .. files[i])
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
