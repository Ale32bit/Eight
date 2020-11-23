local term = require("term")
local screen = require("screen")

term.setPos(0,0)
term.print("Eight", os.version())

local font = screen.loadFont("font.ttf", 16)
while true do
    local _, _, x, y = event.pull("mouse_click")
    screen.drawText(font.font, "Hello, World!", x, y, 255, 0, 0)
end
