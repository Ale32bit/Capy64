local fs = require("fs")

local args = {...}
if #args == 0 then
    print("Usage: rm <file>")
    return
end

local file = fs.combine(shell.getDir(), args[1])
fs.delete(file, true)