-- Eight BIOS v1

local screen = require("screen")
local fs = require("filesystem")
local audio = require("audio")

_HOST = _HOST or "Generic host"

local w, h, oSize = screen.getSize()

local cX, cY = 0, 0
local function log(msg, offset, nonewline)
    os.log(msg)
    offset = offset or 0
    for i = 1, #msg do
        if cX >= w then
            cX = 0
            cY = cY + 1
        end
        local c = string.sub(msg, i, i)

        if c == "\n" then
            cY = cY + 1
            cX = 0
        elseif c == "\t" then
            cX = cX + 2
        else
            screen.setChar(c, cX + offset, cY)
            cX = cX + 1
        end
        
    end

    cX = 0
    if not nonewline then
        cY = cY + 1
    end

    if cY >= h then
        screen.scroll(-1)
        cY = h - 1
    end
end

local function center(msg)
    cX = math.floor(w / 2 - #msg / 2)
    log(msg)
end

local function panic(err)
    local stacktrace = os.traceback(err or "Unknown", 2);
    screen.setSize(w, h, oSize)
    screen.setBackground(0x0000ff)
    screen.clear()

    cY = math.floor(h / 4)

    screen.setForeground(0x0000ff)
    screen.setBackground(0xffffff)
    center(" Unhandled exception ")
    
    screen.setForeground(0xffffff)
    screen.setBackground(0x0000ff)
    cY = cY + 1

    log(stacktrace, 1)

    cY = h-1
    log("Hold CTRL + R to restart", nil, true)

    while true do
        coroutine.yield()
    end
end

screen.clear()

log("Eight Bootloader v1")
log("Lua: " .. _VERSION)
log("Host: " .. _HOST)

-- Load the OS
if not fs.exists("init.lua") then
    log("Operating System not found:")
    log("Missing init.lua")
else
    local func, err = loadfile("init.lua")
    if func then
        log("Booting OS...")
        audio.beep(840, 200)
        local ok, err = pcall(func, ...)
        if not ok then
            panic(err)
        end
    else
        panic(err)
    end
end

-- Keep going and do absolutely nothing, let the OS manage the shutdown
while true do
    coroutine.yield()
end