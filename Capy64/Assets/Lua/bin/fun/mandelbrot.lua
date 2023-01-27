-- Mandelbrot in Capy64

local gpu = require("gpu")
local timer = require("timer")
local event = require("event")
local term = require("term")

-- lower = closer = slower
local scale = 4

-- higher = more detailed = slower
local iterations = 100

local pscale = scale
local w, h = gpu.getSize()
local px = pscale / w
local colorUnit = math.floor(0xffffff / iterations)
-- todo: make it interactive
local dx, dy = 0, 0
local cx, cy = math.floor(w / 2), math.floor(h / 2)

-- z = z^2 + c
local function mandelbrot(zr, zi, cr, ci)
    return zr ^ 2 - zi ^ 2 + cr,
        2 * zr * zi + ci
end

local function iter(cr, ci)
    local zr, zi = 0, 0
    for i = 1, iterations do
        zr, zi = mandelbrot(zr, zi, cr, ci)
        if math.abs(zr) >= (pscale >= 2 and pscale or 2) or math.abs(zi) >= (pscale >= 2 and pscale or 2) then
            return false, colorUnit, i
        end
    end

    return true, 0, iterations
end

local function draw()
    local buffer <close> = gpu.newBuffer()

    for y = 0, h - 1 do
        for x = 0, w - 1 do
            local _, _, i = iter((x - cx + dx * pscale) * px, (y - cy + dy * pscale) * px)
            buffer[y * w + x] = colorUnit * (iterations - i)
        end
    end

    gpu.setBuffer(buffer)
end

-- no idea why it's needed
timer.sleep(1)

draw()

local tw, th = term.getSize()

while true do
    term.setPos(1, th)
    term.setBackground(0)
    term.setForeground(0xffffff)
    term.write("X: " .. dx .. "; Y: " .. dy .. "; S: " .. pscale .. "; " .. px .. "!")
    local ev = { event.pull("key_down") }
    if ev[1] == "key_down" then
        local key = ev[3]
        if key == "up" then
            dy = dy - 10 / pscale
        elseif key == "down" then
            dy = dy + 10 / pscale
        elseif key == "right" then
            dx = dx + 10 / pscale
        elseif key == "left" then
            dx = dx - 10 / pscale
        elseif key == "enter" then
            draw()
        elseif key == "page_down" then
            pscale = pscale * 1.25
            dx = dx * pscale
            dy = dy * pscale
        elseif key == "page_up" then
            pscale = pscale / 1.25
            dx = dx / pscale
            dy = dy / pscale
        elseif key == "r" then
            pscale = scale
            dx = 0
            dy = 0
        end
    end

    px = pscale / w
end
