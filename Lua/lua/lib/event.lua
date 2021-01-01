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
    if #filters > 0 then
        local ev = {}
        repeat
            ev = {coroutine.yield()}
        until inTable(filters, ev[1])
        return table.unpack(ev)
    else
        return coroutine.yield()
    end
end

function event.on(eventName, callback)
    expect(1, eventName, "string")
    expect(2, callback, "function")
    
    local id = #event.__listeners + 1
    
    event.__listeners[id] = {
        event = eventName,
        callback = callback,
    }
    
    return id
end

function event.removeListener(id)
    expect(1, id, "number")
    
    event.__listeners[id] = nil
end

return event