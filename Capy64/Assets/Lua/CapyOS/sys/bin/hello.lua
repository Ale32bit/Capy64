local timer = require("timer")
local colors = require("colors")
local term = require("term")

local function slowPrint(text, delay)
    for i = 1, #text do
        local ch = text:sub(i, i)
        io.write(ch)
        timer.sleep(delay)
    end
    print()
end

local args = {...}
local text = "Hello, World!"
if #args > 0 then
    text = table.concat(args, " ")
end

local color = colors[math.random(1, #colors)]

term.setForeground(color)
slowPrint(text, 0.05)
