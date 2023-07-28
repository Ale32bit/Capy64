local fs = require("fs")
local helpPath = "/sys/share/help"

local topicName = arg[1] or "index"

if not fs.exists(fs.combine(helpPath, topicName)) then
	print(string.format("Topic \"%s\" not found.", topicName))
	return false
end

shell.run("/sys/bin/less.lua", fs.combine(helpPath, topicName))