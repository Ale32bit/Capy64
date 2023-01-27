local http = require("http")
local fs = require("fs")

local args = { ... }

if not http then
    error("HTTP is not enabled", 0)
end

if #args == 0 then
    print("Usage: wget <url> [outputPath]")
    return false
end

local outputName = args[2] or fs.getName(args[1])
local outputPath = shell.resolve(outputName)

if not http.checkURL(args[1]) then
    error("Invalid URL", 0)
end

local response <close>, err = http.get(args[1], nil, {
    binary = true,
})
if not response then
    error(err, 0)
end

local file <close> = fs.open(outputPath, "wb")
file:write(response:readAll())
file:close()
response:close()

print("File written to " .. outputPath)
