local fs = require("fs")
local args = {...}

if #args == 0 then
    print("Usage: mkdir <directory>")
    return
end

local dir = fs.combine(shell.getDir(), args[1])
if fs.exists(dir) then
    error("Path already exists", 0)
end

fs.makeDir(dir)