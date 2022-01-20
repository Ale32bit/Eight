print("Eight 2 Alpha")
local screen = require("screen")
local function deepPrintArray(tbl, indent, index)
    indent = indent or 0
    for k, v in ipairs(tbl) do
        if type(v) == "table" then
            print(tostring(k) .. " {")
            deepPrintArray(v, indent + 1)
            print("}")
        else
            if indent > 0 then
                print("", k, v)
            else
                print(k, v)
            end
        end
    end
end

local function inArr(t, va)
    for k, v in ipairs(t) do
        if v == va then
            return true
        end
    end
    return false
end

-- just printing events and more
local oldx, oldy
while true do
    local ev = table.pack(coroutine.yield())
    deepPrintArray(ev)
    --print("-------------------------")

    if ev[1] == "mouse_down" or ev[1] == "mouse_drag" then
        oldx, oldy = oldx or ev[2], oldy or ev[3]
        local c = 0xffffffff

        if type(ev[4]) == "table" then
            if not inArr(ev[4], 1) then
                c = 0xff000000
            end
        end
        screen.drawLine(ev[2], ev[3], oldx, oldy, c)
        oldx, oldy = ev[2], ev[3]
    elseif ev[1] == "mouse_up" then
        oldx, oldy = nil, nil
    elseif ev[1] == "key_down" and ev[2] == "r" then
        screen.clear()
    end
end