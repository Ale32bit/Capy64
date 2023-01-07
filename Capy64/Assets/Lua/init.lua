local fs = require("fs")

local files = fs.list("/boot")
for k, v in ipairs(files) do
    dofile("/boot/" .. v)
end