local fs = require("fs")
local expect = require("expect").expect

local fsList = fs.list

function fs.list(path, filter)
    expect(1, path, "string")
    expect(2, filter, "nil", "function")

    if not fs.isDir(path) then
        error("directory not found", 2)
    end

    local list = fsList(path)
    if not filter then
        return list
    end

    local filteredList = {}
    for i = 1, #list do
        local attributes = fs.attributes(fs.combine(path, list[i]))
        if filter(list[i], attributes) then
            table.insert(filteredList, list[i])
        end
    end

    return filteredList
end