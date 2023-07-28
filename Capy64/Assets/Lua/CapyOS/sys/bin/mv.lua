local fs = require("fs")
local argparser = require("argparser")

local args, options = argparser.parse(...)

if not args[1] or not args[2] or options.h or options.help then
    print("Usage: mv [option...] <source> <target>")
    print("Options:")
    print(" -h --help: Display help")
    return
end

local source = shell.resolve(args[1])
local destination = shell.resolve(args[2])

fs.move(source, destination)