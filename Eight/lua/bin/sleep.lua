local timer = require("timer")

local args = {...}

if not args[1] then
    print("Usage: sleep <seconds>")
    return true
end

local time = tonumber(args[1])

if not time then
    error("Argument must be a number", 0)
    return false
end

time = time * 1000

timer.sleep(time)

return true