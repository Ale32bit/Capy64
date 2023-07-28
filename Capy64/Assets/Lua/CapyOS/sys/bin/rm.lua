local fs = require("fs")
local argparser = require("argparser")

local args, options = argparser.parse(...)

if not args[1] or options.h or options.help then
    print("Usage: rm [option...] <path>")
    print("Options:")
    print(" -r --recursive: Delete non-empty directories")
    print(" -h --help: Display help")
    return
end

local file = shell.resolve(args[1])

fs.delete(file, options.recursive or options.r)
