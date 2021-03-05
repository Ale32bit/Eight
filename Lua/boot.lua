-- Eight BIOS v1

local screen = require("screen")
local fs = require("filesystem")
local audio = require("audio")

_HOST = _HOST or "Generic host"

local debug = package.loaded.debug
os.log = print

local w, h, oSize = screen.getSize()

local cX, cY = 0, -1
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
    local stacktrace = debug.traceback(err or "Unknown", 2);
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

audio.beep(840, 200);

log("Eight Bootloader v1")
log("Lua: " .. _VERSION)
log("Host: " .. _HOST)

-- Removing dangerous functions
-- Todo: find a safer way

-- Patch exploits
package.loaded.os = os
package.loaded.io = nil
package.loaded.debug = nil

_G.package.cpath = ""
_G.package.path = "?;?.lua;?/init.lua;lib/?.lua;lib/?/init.lua;libs/?.lua;libs/?/init.lua"

-- Patch require()
local function preload()
    return function(name)
        if package.preload[name] then
            return package.preload[name]
        else
            return nil, "no field package.preload['" .. name .. "']"
        end
    end
end

local function from_file(env, dir)
    return function(name)
        local fname = string.gsub(name, "%.", "/")
        local sError = ""
        for pattern in string.gmatch(package.path, "[^;]+") do
            local sPath = string.gsub(pattern, "%?", fname)
            if fs.exists(sPath) and fs.getType(sPath) ~= "directory" then
                local fnFile, sError = loadfile(sPath, nil, env)
                if fnFile then
                    return fnFile, sPath
                else
                    return nil, sError
                end
            else
                if #sError > 0 then
                    sError = sError .. "\n  "
                end
                sError = sError .. "no file '" .. sPath .. "'"
            end
        end
        return nil, sError
    end
end

_G.package.searchers = {
    preload(),
    from_file(_G, "/")
}

local sentinel = {}
function _G.require(name)
    if package.loaded[name] == sentinel then
        error("loop or previous error loading module '" .. name .. "'", 0)
    end

    if package.loaded[name] then
        return package.loaded[name]
    end

    local sError = "module '" .. name .. "' not found:"
    for _, searcher in ipairs(package.searchers) do
        local loader = table.pack(searcher(name))
        if loader[1] then
            package.loaded[name] = sentinel
            local result = loader[1](name, table.unpack(loader, 2, loader.n))
            if result == nil then
                result = true
            end

            package.loaded[name] = result
            return result
        else
            sError = sError .. "\n  " .. loader[2]
        end
    end
    error(sError, 2)
end

-- Patch filesystem
function _G.loadfile(filename, mode, env)
    local f = fs.open(filename, "r")
    local content = f:read("*a")
    f:close()
    return load(content, "@" .. filename, mode, env or _ENV)
end

function _G.dofile(filename)
    local func, err = loadfile(filename, nil, _G)
    if func then
        return func()
    else
        error(err, 2)
    end
end

-- Load the OS
if not fs.exists("init.lua") then
    log("Operating System not found:")
    log("Missing init.lua")
else
    local func, err = loadfile("init.lua")
    if func then
        log("Booting OS...")
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