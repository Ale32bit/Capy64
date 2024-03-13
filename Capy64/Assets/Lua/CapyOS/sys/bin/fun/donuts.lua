local gpu = require("gpu")
local event = require("event")
local donuts = {}
local limit = 100

local w, h = gpu.getSize()
local function insert()
    local donut = {
        x = math.random(-20, w + 20),
        y = math.random(-20, h + 20),
        d = math.random() * math.pi*2,
        dir = math.random(0, 1),
        c = math.random(0xffffff),
        life = math.random(100, 1000),
    }
    table.insert(donuts, donut)
end

while true do
    if #donuts < limit then
        insert()
    end
    gpu.clear(0)
    for k, donut in ipairs(donuts) do
        if donut.life <= 0 then
            table.remove(donuts, k)
        end
        local doReverse = math.random(0, 1000) > 950
        donut.x = donut.x + math.cos(donut.d) * 4
        donut.y = donut.y + math.sin(donut.d) * 4
        donut.d = donut.d + (donut.dir == 1 and 0.05 or -0.05)
        gpu.drawCircle(donut.x, donut.y, 20, donut.c, 10)
        if doReverse then
            donut.dir = donut.dir == 1 and 0 or 1
        end
        donut.life = donut.life - 1
    end
    event.push("donuts")
    event.pull("donuts")
end
