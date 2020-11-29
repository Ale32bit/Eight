local timer = require("timer")
local event = require("event")
local expect = require("expect")

function timer.sleep(ms)
    expect(1, ms, "number")
    local timer = timer.start(ms or 1)

    local _, par
    repeat
        _, par = event.pull("timer")
    until par == timer
    return timer
end
