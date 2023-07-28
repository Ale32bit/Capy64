local scheduler = require("scheduler")
scheduler.spawn(function()
    shell.run(arg.string)
end)