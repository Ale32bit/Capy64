local expect = require("expect").expect
local tableutils = require("tableutils")
local event = require("event")

local scheduler = {}

local function contains(array, value)
    for k, v in pairs(array) do
        if v == value then
            return true
        end
    end
    return false
end

local tasks = {}
local processes = 0

local Task = {}
local TaskMeta = {
    __index = Task,
    __name = "OS_TASK",
    __tostring = function(self)
        return string.format("OS_TASK[%s]: %d", self.source or "", self.pid or 0)
    end,
}
local function newTask()
    local task = {}
    return setmetatable(task, TaskMeta)
end

function Task:queue(eventName, ...)
    expect(1, eventName, "string")
    event.push("scheduler", self.pid, eventName, ...)
end

local function findParent()
    local i = 3

    while true do
        local info = debug.getinfo(i)
        if not info then
            break
        end

        for pid, task in pairs(tasks) do
            if task.uuid == tostring(info.func) then
                return task
            end
        end

        i = i + 1
    end

    return nil
end

local function cascadeKill(pid, err)
    local task = tasks[pid]
    if not task then
        return
    end
    for i, cpid in ipairs(task.children) do
        cascadeKill(cpid, err)
    end
    if task.parent then
        local parent = tasks[task.parent]
        if parent then
            local index = tableutils.find(parent.children, task.pid)
            table.remove(parent.children, index)
            parent:queue("scheduler_task_end", task, err == nil, err)
        end
    else
        if err then
            error(err, 0)
        end
    end
    if task then
        task.killed = true
        coroutine.close(task.thread)
        tasks[pid] = nil
        processes = processes - 1
    end
end

local function resumeTask(task, yieldPars)
    local pars = table.pack(coroutine.resume(task.thread, table.unpack(yieldPars)))
    if pars[1] then
        task.filters = table.pack(table.unpack(pars, 2))
        return coroutine.status(task.thread) ~= "dead"
    else
        cascadeKill(task.pid, pars[2])
        return false
    end
end

function scheduler.spawn(func, options)
    expect(1, func, "function")
    expect(2, options, "nil", "table")

    options = options or {}
    options.args = options.args or {}

    local source = debug.getinfo(2)

    local task = newTask()
    local pid = #tasks + 1
    task.pid = pid
    task.options = options
    task.source = source.source
    task.uuid = tostring(func)
    task.thread = coroutine.create(func)
    local parent = findParent()
    if parent then
        task.parent = parent.pid
        table.insert(parent.children, pid)
    end
    task.filters = {}
    task.children = {}
    task.eventQueue = {}
    task.skip = true

    tasks[pid] = task

    processes = processes + 1

    return task, resumeTask(task, task.options.args)
end

function scheduler.kill(pid)
    expect(1, pid, "number")
    cascadeKill(pid)
end

function scheduler.ipc(pid, ...)
    expect(1, pid, "number")
    if not tasks[pid] then
        error("process by pid " .. pid .. " does not exist.", 2)
    end

    local sender = findParent()
    tasks[pid]:queue("ipc_message", sender, ...)
end

local running = false
function scheduler.init()
    if running then
        error("scheduler already running", 2)
    end
    running = true

    local ev = { n = 0 }
    while processes > 0 do
        for pid, task in pairs(tasks) do
            local yieldPars = ev
            if ev[1] == "scheduler" and ev[2] == pid then
                yieldPars = table.pack(table.unpack(ev, 3))
            end
            if yieldPars[1] ~= "scheduler" and not task.filters or #task.filters == 0 or contains(task.filters, yieldPars[1]) or yieldPars[1] == "interrupt" then
                if task.skip then
                    task.skip = false
                else
                    resumeTask(task, yieldPars)
                end
            end

            if coroutine.status(task.thread) == "dead" then
                cascadeKill(pid)
            end
        end

        if processes <= 0 then
            break
        end

        ev = table.pack(coroutine.yield())
    end

    running = false
end

return scheduler
