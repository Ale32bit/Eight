-- FIRMWARE

local fs = require("filesystem")

_HOST = _HOST or "Generic host"

local debug = package.loaded.debug
os.log = print
os.traceback = debug.traceback

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