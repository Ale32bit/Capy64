-- This file is part of Capy64 - https://github.com/Capy64/Capy64
-- Copyright 2023 Alessandro "AlexDevs" Proto
--
-- Licensed under the Apache License, Version 2.0 (the "License").
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
--
--     http://www.apache.org/licenses/LICENSE-2.0
--
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.

local event = require("event")
local coroutine = coroutine

-- Declare event functions
function event.pull(...)
    local ev = table.pack(coroutine.yield(...))
    if ev[1] == "interrupt" then
        error("Interrupted", 2)
    end
    return table.unpack(ev)
end

function event.pullRaw(...)
    return coroutine.yield(...)
end

-- Set task awaiter

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