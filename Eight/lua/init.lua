local term = require("term")
local screen = require("screen")

term.setPos(0,0)
term.print("Eight", os.version())

local font = screen.loadFont("font.ttf", 6)
screen.drawText(font, "hello, world!", 0, 0, 255, 0, 0)