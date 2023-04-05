local args = { ... }
local fs = require("fs")
local term = require("term")
local colors = require("colors")
local dir = shell.getDir()

if args[1] then
    dir = shell.resolve(args[1])
end

if not fs.isDir(dir) then
    error("No such directory: " .. dir, 0)
    return false
end

local files = fs.list(dir)
for k, v in ipairs(files) do
    if fs.isDir(fs.combine(dir, v)) then
        term.setForeground(colors.lightBlue)
        print(v .. "/")
    else
        term.setForeground(colors.white)
        print(v)
    end
end
