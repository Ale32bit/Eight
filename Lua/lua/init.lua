-- INIT

local args = {...}
local screen = require("screen")
local fs = require("filesystem")

-- Boot
for _, file in ipairs(fs.list("boot")) do
    dofile("boot/" .. file)
end

local event = require("event")
local term = require("term")

-- not really the fanciest way
local function init()
    while true do
        dofile("/bin/shell.lua")
    end
end

local initThread = coroutine.create(init)

local filter
local function resume()
    for i = 1, #event.__eventsQueue do
        local ev = event.__eventsQueue[i]
        if filter == nil or filter == event[1] then
            if coroutine.status(initThread) == "dead" then
                return false
            end
            local ok, par = coroutine.resume(initThread, table.unpack(ev))
            if ok then
                filter = par
                local eventName = ev[1]
                table.remove(ev, 1)
                for _, v in pairs(event.__listeners) do
                    if v.event == eventName then
                        v.callback(table.unpack(ev))
                    end
                end
            else
                error(par, 0)
                return false
            end
        end
    end

    event.__eventsQueue = {}
    
    return true
end

screen.setForeground(0xffffff)
screen.setBackground(0x000000)
screen.clear()

term.init()

term.clear()
term.setPos(0, 0)

term.setForeground(0x2196f3)
print("Eight", os.version())
term.setForeground(0xffffff)

event.push("_eight_init")
resume();

while true do
    event.__eventsQueue[#event.__eventsQueue+1] = {coroutine.yield()}
    if not resume() then
        os.exit(0, true)
    end
end