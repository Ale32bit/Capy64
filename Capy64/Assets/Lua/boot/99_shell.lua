local term = require("term")
local colors = require("colors")

term.setSize(51, 19)

term.setForeground(0x59c9ff)
term.setBackground(colors.black)
term.clear()
term.setPos(1,1)

print(os.version())

dofile("/bin/lua.lua")