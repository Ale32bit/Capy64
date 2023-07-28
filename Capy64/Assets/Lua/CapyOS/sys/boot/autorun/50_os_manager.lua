local machine = require("machine")
local scheduler = require("scheduler")

local term = require("term")
local colors = require("colors")
term.setForeground(0x59c9ff)
term.setBackground(colors.black)
term.clear()
term.setPos(1, 1)

term.write(os.version())
term.setPos(1, 2)

local function spawnShell()
    return scheduler.spawn(loadfile("/sys/bin/shell.lua"))
end

local function main()
    local shellTask = spawnShell()
    while true do
        local ev = {coroutine.yield()}
        if ev[1] == "ipc_message" then
            local sender = ev[2]
            local call = ev[3]
            if call == "power" then
                local options = ev[4]
                --todo: handle time and cancels
                if options.reboot then
                    machine.reboot()
                else
                    machine.shutdown()
                end
            end
        elseif ev[1] == "scheduler_task_end" then
            if ev[2].pid == shellTask.pid then
                if not ev[3] then
                    io.stderr.print(ev[4])
                end
                shellTask = spawnShell()
            end
        end
    end
end

scheduler.spawn(main)

scheduler.init()