local fs = require("fs")
local args = { ... }

if #args == 0 then
    print("Usage: mkdir <directory>")
    return
end

local dir = shell.resolve(args[1])
if fs.exists(dir) then
    error("Path already exists", 0)
end

fs.makeDir(dir)
