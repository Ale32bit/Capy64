local fs = require("fs")
local programs = fs.list("/sys/bin", function(name, attr)
    return not attr.isDirectory
end)

for i, v in ipairs(programs) do
    programs[i] = string.gsub(v, "%.lua$", "")
end

print(table.concat(programs, " "))