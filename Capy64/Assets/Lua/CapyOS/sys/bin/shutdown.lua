local timer = require("timer")
local machine = require("machine")

print("Goodbye!")

timer.sleep(1000)

machine.shutdown()
