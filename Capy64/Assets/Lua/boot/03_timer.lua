local timer = require("timer")

function timer.sleep(n)
    local timerId = timer.start(n)
    repeat
        local _, par = coroutine.yield("timer")
    until par == timerId
end