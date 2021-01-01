local term = require("term")
local currentFg = term.getForeground()

local c = math.random(0, 256^3-1)

term.setForeground(c)

print("Hello, World!")

term.setForeground(currentFg)