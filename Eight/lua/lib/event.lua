local expect = require("expect")

local function inTable(tbl, el)
    for k, v in ipairs(tbl) do
        if v == el then
            return true
        end
    end
    return false
end

local eventsQueue = {}
local listeners = {}

local function push(...)
    eventsQueue[#eventsQueue + 1] = { ... }
end

local function pull(...)
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

local function on(eventName, callback)
    expect(1, eventName, "string")
    expect(2, callback, "function")
    
    local id = #listeners + 1
    
    listeners[id] = {
        event = eventName,
        callback = callback,
    }
    
    return id
end

local function removeListener(id)
    expect(1, id, "number")
    
    listeners[id] = nil
end

return {
    push = push,
    pull = pull,
    __eventsQueue = eventsQueue,
    __listeners = listeners
}