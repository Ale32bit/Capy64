local term = require("term")
local colors = require("colors")
local machine = require("machine")

term.setForeground(0x59c9ff)
term.setBackground(colors.black)
term.clear()
term.setPos(1, 1)

term.write(os.version())
term.setPos(1, 2)

dofile("/bin/shell.lua")

machine.shutdown()
