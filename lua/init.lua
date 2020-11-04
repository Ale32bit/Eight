local rw,rh, scale = screen.getSize()
local term = require("lua.term")

term.setSize(200 / 6, 150 / 12, 4)

term.print("Hello, world!")

term.write("> ")
local input = term.read("*")
term.setBackground(math.random(0, 255), math.random(0, 255), math.random(0, 255))
term.clear()
term.setPos(0,0)
term.print(input)

while true do
	local _, _, x, y = event.pull("mouse_click", "mouse_drag")
	
	screen.setPixel(x, y, 0xff, 0xff, 0xff)
end