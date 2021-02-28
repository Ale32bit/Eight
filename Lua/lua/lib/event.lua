-- Event library
-- Handles events

local expect = require("expect")
local event = {}

local function inTable(tbl, el)
    for k, v in ipairs(tbl) do
        if v == el then
            return true
        end
    end
    return false
end

event.__eventsQueue = {}
event.__listeners = {}

function event.push(...)
    event.__eventsQueue[#event.__eventsQueue + 1] = { ... }
end

function event.pull(...)
    local filters = {...}
    local ev = {}
    if #filters > 0 then
        repeat
            ev = {coroutine.yield()}
        until inTable(filters, ev[1]) or ev[1] == "interrupt"
    else
        ev = {coroutine.yield()}
    end

    if ev[1] == "interrupt" and not inTable(filters, "interrupt") then
        error("interrupted", 0)
    end

    return table.unpack(ev)
end

return event