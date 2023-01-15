local term = require("term")
local colors = require("colors")

term.setSize(51, 19)

term.setForeground(0x59c9ff)
term.setBackground(colors.black)
term.clear()
term.setPos(1, 1)

print(os.version())

local func, err = loadfile("/bin/shell.lua")
if func then
    while true do
        local ok, err = pcall(func)
        if not ok then
            print(err)
        end
    end
else
    print(err)
end



print("Press any key to continue...")
coroutine.yield("key_down")
os.shutdown(false)