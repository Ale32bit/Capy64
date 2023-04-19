local timer = require("timer")
local event = require("event")
local expect = require("expect").expect
local range = require("expect").range

function timer.sleep(n)
    expect(1, n, "number")

    local timerId = timer.start(n)
    repeat
        local _, par = event.pull("timer")
    until par == timerId
end
