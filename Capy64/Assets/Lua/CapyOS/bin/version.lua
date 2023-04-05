local machine = require("machine")
print(string.format("%s @ %s - %s", os.version(), machine.version(), _VERSION))
