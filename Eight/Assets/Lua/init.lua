print("hi")

local fs = require("fs")
local readbdf = require("Assets.Lua.bdf")
local file = fs.read("Assets/Lua/craftos.bdf")
local font = readbdf(file)

print(font)

print("hi")

while true do
    coroutine.yield()
end