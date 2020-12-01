local term = require("term")
local screen = require("screen")
local event = require("event")
local colors = require("colors")

term.setPos(0,0)
term.setForeground(colors.lightBlue)
print("Eight", os.version())
term.setForeground(colors.white)

while true do
    local input = read()
end

