local term = require("term")
local keys = require("keys")
local event = require("event")
local fs = require("fs")
local timer = require("timer")
local colors = require("colors")

local filename = shell.resolve(arg[1])

local f<close> = fs.open(filename, "r")
local lines = {}
local lineMax = 0
for line in f:lines() do
    table.insert(lines, line)
    lineMax = math.max(lineMax, #line)
end
f:close()


local width, height = term.getSize()
height = height - 1
local posx, posy = 0, 0

local function redraw()
    term.clear()
    term.setForeground(colors.white)
    for i = 1, height do
        if i + posy > #lines then
            break
        end
        term.setPos(-posx + 1, i)
        term.write(lines[i + posy])
    end

    term.setForeground(colors.yellow)
    term.setPos(1, height + 1)
    term.write("Use arrow keys to move or press Q to exit.")
end

while true do
    redraw()

    local _, key = event.pull("key_down")

    if key == keys.enter or key == keys.down then
        posy = posy + 1
    elseif key == keys.up then
        posy = posy - 1
    elseif key == keys.right then
        posx = posx + 1
    elseif key == keys.left then
        posx = posx - 1
    elseif key == keys.q or key == keys.escape then
        -- Clear event queue
        timer.sleep(0)
        term.clear()
        term.setPos(1, 1)
        break
    end

    

    if posy > #lines - height then
        posy = #lines - height
    end

    if posy < 0 then
        posy = 0
    end

    if posx + width > lineMax + 1 then
        posx = lineMax - width + 1
    end

    if posx < 0 then
        posx = 0
    end
end
