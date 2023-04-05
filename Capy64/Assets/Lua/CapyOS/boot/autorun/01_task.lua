local event = require("event")

local function awaiter(task)
    local status = task:getStatus()
    local uuid = task:getID()
    if status == "running" then
        local _, taskId, result, err
        repeat
            _, taskId, result, err = event.pull("task_finish")
        until taskId == uuid
        return result, err
    elseif status == "succeeded" then
        return task:getResult(), nil
    elseif status == "failed" then
        return nil, task:getError()
    end
end

-- Second argument freezes the awaiter function, so it cannot be modified
event.setAwaiter(awaiter, true)