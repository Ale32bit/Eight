-- INIT

local args = {...}
local screen = require("screen")
local fs = require("filesystem")
local parallel = require("parallel")

-- Boot
local bootErrors = {}
for _, file in ipairs(fs.list("boot")) do
    local ok, err = pcall(dofile, "boot/" .. file)
    if not ok then
        print(err)
        local n = string.find(err, "\n")
        table.insert(bootErrors, string.sub(err, 1, n))
    end
end

local event = require("event")
local term = require("term")

-- not really the fanciest way
local function init()
    parallel.waitForAny(
        function()
            while true do
                local ok, err = pcall(loadfile("/bin/shell.lua"))
                if not ok then
                    if err == "interrupted" then
                        print("^C")
                    else
                        error(err)
                    end
                end
            end
        end,
        term.run
    )
end

local initThread = coroutine.create(init)

local filter
local function resume(...)
    local ev = select(1, ...)
    if filter == nil or filter == ev then
        if coroutine.status(initThread) == "dead" then
            return false
        end
        local ok, par = coroutine.resume(initThread, ...)
        if ok then
            filter = par
        else
            error(par, 0)
            return false
        end
    end
    return true
end

term.setBackground(0x000000)
term.setForeground(0x2196f3)
term.clear()
term.setPos(0, 0)
print("Eight", os.version())
term.setForeground(0xffffff)

for i = 1, #bootErrors do
    print(bootErrors[i])
end

resume()

while true do
    event.__eventsQueue[#event.__eventsQueue+1] = table.pack(coroutine.yield())
    for i, ev in pairs(event.__eventsQueue) do
        resume(table.unpack(ev))
        event.__eventsQueue[i] = nil
    end
end