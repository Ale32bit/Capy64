local fs = require("fs")

local args = {...}

local path = shell.resolve(args[1])

local f<close> = fs.open(path, "r")
print(f:read("a"))
f:close()