local argparser = require("argparser")

local args, options = argparser.parse(...)
print(table.concat(args, " "))