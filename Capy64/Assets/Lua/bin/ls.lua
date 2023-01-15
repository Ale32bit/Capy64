local args = { ... }
local fs = require("fs")
local term = require("term")
local colors = require("colors")
local dir = shell.getDir()

if args[1] then
    dir = fs.combine(shell.getDir(), args[1])
end

if not fs.exists(dir) then
    error("No such directory: " .. dir, 0)
    return false
end

local attr = fs.getAttributes(dir)
if not attr.isDirectory then
    error("No such directory: " .. dir, 0)
    return false
end

local files = fs.list(dir)
for k,v in ipairs(files) do
    local attr = fs.getAttributes(fs.combine(dir, v))
    if attr.isDirectory then
        term.setForeground(colors.lightBlue)
        print(v .. "/")
    else
        term.setForeground(colors.white)
        print(v)
    end
end