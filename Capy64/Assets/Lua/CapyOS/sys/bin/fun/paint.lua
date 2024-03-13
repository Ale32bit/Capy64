local event = require("event")
local gpu = require("gpu")
local colors = require("colors")
local term = require("term")
local timer = require("timer")

local w, h = gpu.getSize()
local tw, th = term.getSize()

local selectedColor = 1
local thickness = 4

local canvasW, canvasH = term.toRealPos(tw - 1, th + 1)
canvasW = canvasW - 3
local size = canvasW * canvasH
local canvas = {string.unpack(("B"):rep(size), ("\0"):rep(size))}
canvas[#canvas] = nil

local function drawCircle(buffer, x, y, radius, color)
    radius = math.max(0, radius)
    if radius == 0 then
        buffer[x + buffer.width * y] = color
        return
    end
    local width = buffer.width
    local height = buffer.height

    local index = function(x, y)
        return y * width + x
    end

    local isValid = function(x, y)
        return x >= 0 and x < width and y >= 0 and y < height
    end

    local setPixel = function(x, y, color)
        if isValid(x, y) then
            buffer[index(x, y)] = color
        end
    end

    local drawFilledCirclePoints = function(cx, cy, x, y)
        for dx = -x, x do
            for dy = -y, y do
                setPixel(cx + dx, cy + dy, color)
            end
        end
    end

    local drawCircleBresenham = function(cx, cy, radius)
        local x = 0
        local y = radius
        local d = 3 - 2 * radius
        drawFilledCirclePoints(cx, cy, x, y)
        while y >= x do
            x = x + 1
            if d > 0 then
                y = y - 1
                d = d + 4 * (x - y) + 10
            else
                d = d + 4 * x + 6
            end
            drawFilledCirclePoints(cx, cy, x, y)
        end
    end

    drawCircleBresenham(x, y, radius)
end

local function drawLine(buffer, x0, y0, x1, y1, color, thickness)
    local width = canvasW
    local height = canvasH

    local index = function(x, y)
        return y * width + x
    end

    local isValid = function(x, y)
        return x >= 0 and x < width and y >= 0 and y < height
    end

    local setPixel = function(x, y)
        if isValid(x, y) then
            buffer[index(x, y)] = color
        end
    end

    local drawLineBresenham = function()
        local i = 0
        local dx = math.abs(x1 - x0)
        local dy = math.abs(y1 - y0)
        local sx = x0 < x1 and 1 or -1
        local sy = y0 < y1 and 1 or -1
        local err = dx - dy

        local majorAxis = dx > dy

        while x0 ~= x1 or y0 ~= y1 do
            for i = 0, thickness - 1 do
                if majorAxis then
                    setPixel(x0, y0 + i)
                else
                    setPixel(x0 + i, y0)
                end
            end

            local err2 = 2 * err
            if err2 > -dy then
                err = err - dy
                x0 = x0 + sx
            end
            if err2 < dx then
                err = err + dx
                y0 = y0 + sy
            end

            if i % 1024 == 0 then
                --event.push("paint")
                --event.pull("paint")
                --timer.sleep(0)
            end
        end
    end
    drawLineBresenham()
end

local function drawUI()
    term.setBackground(0)
    term.clear()
    for y = 1, 16 do
        term.setPos(tw - 1, y)
        term.setBackground(0)
        term.setForeground(colors[y])
        if selectedColor == y then
            term.setBackground(colors[y])
            term.write("  ")
        else
            term.write("##")
        end
    end
    term.setPos(tw - 1, 17)

    if selectedColor == 0 then
        term.setBackground(colors.white)
        term.setForeground(0)
    else
        term.setBackground(0)
        term.setForeground(colors.white)
    end
    term.write("XX")

    term.setPos(tw - 1, 18)
    term.setBackground(colors.black)
    term.setForeground(colors.white)
    term.write(thickness)

    gpu.drawLine(canvasW + 1, 0, canvasW, canvasH, colors.gray, 2)

    local b<close> = gpu.bufferFrom(canvas, canvasW, canvasH)
    gpu.drawBuffer(b, 0, 0, {
        source = {
            0, 0, canvasW, canvasH
        }
    })
end

local function contains(arr, val)
    for i, v in ipairs(arr) do
        if v == val then
            return true
        end
    end
    return false
end

local oldX, oldY
while true do
    drawUI()

    local ev, b, x, y = event.pull("mouse_down", "mouse_up", "mouse_move", "mouse_scroll")
    local tx, ty = term.fromRealPos(x, y)
    if ev == "mouse_up" then
        if x >= canvasW then
            if ty <= 16 then
                selectedColor = ty
            elseif ty == 17 then
                selectedColor = 0
            end
        end
        oldX, oldY = nil, nil
    elseif ev == "mouse_down" or (ev == "mouse_move" and contains(b, 1)) then
        if x < canvasW and y < canvasH then
            --canvas[x + y * canvasW] = colors[selectedColor] or 0
            --drawCircle(canvas, x, y, thickness - 2, colors[selectedColor])

            drawLine(canvas, x, y, oldX or x, oldY or y, colors[selectedColor] or 0, thickness)
            --gpu.drawLine(x, y, oldX or x, oldY or y, colors[selectedColor] or 0)
            --canvas = gpu.getBuffer()

            oldX, oldY = x, y
        end
    elseif ev == "mouse_scroll" then
        local x, y, b = b, x, y
        local tx, ty = term.fromRealPos(x, y)
        if x >= canvasW and ty == 18 then
            thickness = math.min(99, math.max(0, thickness - b))
        end
    end
end
