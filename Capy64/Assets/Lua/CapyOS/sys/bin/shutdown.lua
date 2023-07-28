local machine = require("machine")
local scheduler = require("scheduler")
local argparser = require("argparser")

local args, options = argparser.parse(...)

if options.h or options.help then
    print("Usage: shutdown [option...]")
    print("Shutdown or restart Capy64.")
    print("Options:")
    print(" -s --shutdown: Shutdown and exit Capy64. (default)")
    print(" -r --reboot: Restart Capy64.")
    print(" -t --time: Time to wait in seconds. (\"now\" is 0 seconds, default)")
    return
end

local time = 0
if options.t or options.time then
    time = options.t or options.time
end
if time == "now" then
    time = 0
else
    time = tonumber(time)
    if not time then
        error("Invalid time option: " .. (options.t or options.time), 0)
    end
end

scheduler.ipc(1, "power", {
    reboot = options.r or options.reboot,
    time = time,
})
