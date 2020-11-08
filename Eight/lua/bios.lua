print("EightOS")

local function inTable(tbl, el)
    for k, v in ipairs(tbl) do
        if v == el then
            return true
        end
    end
    return false
end

timer = require("timer");
function timer.sleep(ms)
    local timer = timer.start(ms or 1)

    local _, par
    repeat
        _, par = event.pull("timer")
    until par == timer
    return timer
end

_G.event = {}
local eventsQueue = {}
function event.pull(...)
    local filters = { ... }
    if #filters > 0 then
        local ev = {}
        repeat
            ev = { coroutine.yield() }
        until inTable(filters, ev[1])
        return table.unpack(ev)
    else
        return coroutine.yield()
    end
end
function event.push(...)
    eventsQueue[#eventsQueue + 1] = { ... }
end

local term = require("term")

_G.term = term
term.init()

local cprint = print
_G.cprint = cprint

_G.print = term.print
_G.write = term.write

local func, err = loadfile("init.lua")
if not func then
    error(err, 0)
end

local initThread = coroutine.create(func)

event.push("_eight_init")
local filter
local function resume()
    for i = 1, #eventsQueue do
        local event = eventsQueue[i]
        if filter == nil or filter == event[1] then
            local ok, par = coroutine.resume(initThread, table.unpack(event))
            if ok then
                filter = par
            else
                error(par, 0)
            end
        end
    end

    eventsQueue = {}
end

while true do
    local ev = { coroutine.yield() }

    if ev[1] == "_eight_tick" then
        eventsQueue[#eventsQueue + 1] = { "tick" }
        resume()
    else
        eventsQueue[#eventsQueue + 1] = ev
        resume()
    end
end
