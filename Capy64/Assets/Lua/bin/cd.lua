local args = {...}
local fs = require("fs")

local dir = args[1]

if not dir then
    dir = shell.homePath
end

dir = shell.resolve(dir)

if not fs.isDir(dir) then
    error("No such directory: " .. dir, 0)
    return false
end

shell.setDir(dir)