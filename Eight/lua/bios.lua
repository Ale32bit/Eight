print("Booting Eight...")

local fs = require("filesystem")
local screen = require("screen")

function _G.loadfile(filename, mode, env)
    local f = fs.open(filename, "r");
    local content = f:read("*a")
    f:close()
    return load(content, "=" .. filename, mode, env or _ENV)
end

function _G.dofile(filename)
    local func, err = loadfile(filename)
    if func then
        return func()
    else
        error(err, 2)
    end
end

-- Boot
for _, file in ipairs(fs.list("boot")) do
    dofile("boot/" .. file)
end

local function inTable(tbl, el)
    for k, v in ipairs(tbl) do
        if v == el then
            return true
        end
    end
    return false
end

local event = require("event")
local term = require("term")
local cprint = print
_G.cprint = cprint
_G.print = term.print
_G.write = term.write

local function panic(err)
    err = err or "Panic"
    cprint(err)
    local ok, err = pcall(function()
        if term then
            local w, h, s = term.getSize()
            local screenWidth, screenHeight = screen.getSize()
            term.setSize(w, h, s)

            screen.drawRectangle(0, 0, screenWidth, screenHeight, 0, 0, 0xff)

            term.setForeground(0xff, 0xff, 0xff)
            term.setBackground(0, 0, 0xff)

            term.setPos(0, 0)

            term.write(err)
        end
    end)
    cprint(ok, err)
    while true do
        coroutine.yield() -- Just yield forever
    end
end

local func, err = loadfile("init.lua")
if not func then
    panic(err)
end

local initThread = coroutine.create(func)

local filter
local function resume()
    for i = 1, #event.__eventsQueue do
        local event = event.__eventsQueue[i]
        if filter == nil or filter == event[1] then
            if coroutine.status(initThread) == "dead" then
                return false
            end
            local ok, par = coroutine.resume(initThread, table.unpack(event))
            if ok then
                filter = par
            else
                panic(par)
                return false
            end
        end
    end

    event.__eventsQueue = {}
    
    return true
end

term.init()

event.push("_eight_init")
resume();

while true do
    event.push(coroutine.yield())
    resume()
end
