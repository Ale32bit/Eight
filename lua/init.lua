term.setSize(64, 24, 2)

local tEnv = {
    exit = os.quit,
    _echo = function(...) return ... end
}
setmetatable(tEnv, {__index=_ENV})

function pack(...)
    return { n = select("#", ...), ... }
end

while true do
    term.write("> ")
    local input = term.read()

    local func, e = load(input, "=lua", "t", tEnv)
    local func2 = load("return _echo(" .. input .. ");", "=lua", "t", tEnv)

    local nForcePrint = 0
    if not func then
        if func2 then
            func = func2
            e = nil
            nForcePrint = 1
        end
    else
        if func2 then
            func = func2
        end
    end

    if func then
        local tResults = pack(pcall(func))
        if tResults[1] then
            local n = 1
            while n < tResults.n or n <= nForcePrint do
                local value = tResults[n + 1]
                serialised = tostring(value)
                if ok then
                    print(serialised)
                else
                    print(tostring(value))
                end
                n = n + 1
            end
        else
            print(tResults[2])
        end
    else
        print(e)
    end
end