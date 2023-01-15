local args = {...}
local fs = require("fs")

local dir = args[1]

if #args == 0 then
    dir = shell.homePath
end

dir = fs.combine(shell.getDir(), dir)

if not fs.exists(dir) or not fs.getAttributes(dir).isDirectory then
    error("No such directory: " .. dir, 0)
    return false
end

shell.setDir(dir)