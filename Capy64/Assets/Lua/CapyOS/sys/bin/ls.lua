local fs = require("fs")
local term = require("term")
local colors = require("colors")
local argparser = require("argparser")

local theme = {
    directory = colors.lightBlue,
    file = colors.white,
    lua = colors.yellow,
}

local function humanizeBytes(n)
    local prefixes = {
        [0] = "",
        "k",
        "M",
        "G",
        "T",
    }
    local block = 1024
    local prefixIndex = 0

    while n >= block do
        n = n / 1024
        prefixIndex = prefixIndex + 1
    end

    return string.format("%.0f%s", n, prefixes[prefixIndex])
end

local args, options = argparser.parse(...)

if options.h or options.help then
    print("Usage: ls [option...] [path]")
    print("List files (current directory by default)")
    print("Options:")
    print(" -a: Include hidden files")
    print(" -l: Use long listing format")
    return
end
local path = shell.getDir()

if args[1] then
    path = shell.resolve(args[1])
end

if not fs.isDir(path) then
    error("No such directory: " .. path, 0)
    return false
end

local entries = fs.list(path)

if options.l then
    print(string.format("total %d", #entries))
end
local printed = 0
for i, entry in ipairs(entries) do
    if entry:sub(1, 1) ~= "." or options.a then
        printed = printed + 1
        local attributes = fs.attributes(fs.combine(path, entry))
        local size = humanizeBytes(attributes.size)
        local date = os.date("%x %H:%m", attributes.modified // 1000)

        local entryType
        if attributes.isDirectory then
            entryType = "directory"
        else
            entryType = "file"
            if string.match(entry, "%.lua$") then
                entryType = "lua"
            end
        end

        if options.l then
            term.setForeground(colors.white)
            term.write(string.format("%s %5s %s ", attributes.isDirectory and "d" or "-", size, date))
        end
        term.setForeground(theme[entryType])
        io.write(entry)
        
        io.write(options.l and "\n" or "\t")
    end
end

if not options.l and printed > 0 then
    print()
end