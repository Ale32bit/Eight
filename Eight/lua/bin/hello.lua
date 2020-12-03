local term = require("term")
local currentFg = {term.getForeground()}

local r, g, b = math.random(0, 255), math.random(0, 255), math.random(0, 255)

term.setForeground(r, g, b)

print("Hello, World!")

term.setForeground(currentFg)