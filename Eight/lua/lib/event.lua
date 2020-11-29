local expect = require("expect")

local eventsQueue = {}

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

return {
    push = push,
    pull = pull,
    __eventsQueue = eventsQueue,
}