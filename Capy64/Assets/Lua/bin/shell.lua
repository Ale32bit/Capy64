local term = require("term")
local colors = require("colors")
local event = require("event")
local io = require("io")

local shell = {}

shell.path = "/bin/?.lua"

function shell.getDir()

end

function shell.setDir(path)

end

function shell.resolve(path)

end

function shell.run(path, args)

end


while true do
    term.setForeground(colors.yellow)
    write("$ ")
    term.setForeground(colors.white)
    local line = io.read()
end