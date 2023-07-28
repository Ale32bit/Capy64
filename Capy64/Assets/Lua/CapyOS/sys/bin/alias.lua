local argparser = require("argparser")

local args, options = argparser.parse(...)


if options.l or options.list then
    for alias, value in pairs(shell.aliases) do
        print(string.format("%s = \"%s\"", alias, value))
    end
    return
end

local alias = args[1]

if not alias or options.h or options.help then
    print("Usage: alias [option...] <alias> [command]")
    print("Options:")
    print(" -l --list: List aliases")
    return false
end

local command = table.pack(select(2, ...))
if #command == 0 then
    shell.aliases[alias] = nil
    return
end

shell.aliases[alias] = table.concat(command, " ")