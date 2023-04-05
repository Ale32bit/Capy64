local fs = require("fs")

local args = { ... }
if #args == 0 then
    print("Usage: rm <file>")
    return
end

local file = shell.resolve(args[1])
fs.delete(file, true)
