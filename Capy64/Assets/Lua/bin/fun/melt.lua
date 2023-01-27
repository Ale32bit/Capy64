local gpu = require("gpu")
local timer = require("timer")
local event = require("event")
local colors = require("colors")
local parallel = require("parallel")

local melts = 2 ^ 12

local function contains(arr, val)
    for k, v in ipairs(arr) do
        if v == val then
            return true
        end
    end
    return false
end

local function melt()
    local w, h = gpu.getSize()
    local x, y = 0, 0

    while true do
        local buffer <close> = gpu.getBuffer()
        for i = 1, melts do
            local nx = math.random(x, w)
            local ny = math.random(y, h)

            local c = buffer[ny * w + nx]
            buffer[(ny + 1) * w + nx] = c
        end
        gpu.setBuffer(buffer)

        timer.sleep(10)
    end
end

local function draw()
    local ox, oy
    while true do
        local ev, b, x, y = event.pull("mouse_move", "mouse_down")

        if ev == "mouse_down" then
            if b == 1 then
                ox = x
                oy = y
            end
        elseif ev == "mouse_move" then
            if contains(b, 1) then
                gpu.plot(x, y, colors.red)
                gpu.drawLine(x, y, ox, oy, colors.red)
                ox, oy = x, y
            end
        end
    end
end

local function random()
    local w, h = gpu.getSize()
    while true do
        for i = 1, 24 do
            gpu.drawString(
                math.random(-7, w),
                math.random(-13, h),
                math.random(0, 0xffffff),
                string.char(math.random(32, 127))
            )
        end

        timer.sleep(100)
    end
end

parallel.waitForAny(draw, melt, random)
