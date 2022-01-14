print("Eight 2 Alpha")

-- just printing events
while true do
    local ev, key, keyx, mods = coroutine.yield()
    if ev == "key_down" then
        for k, v in ipairs(mods) do
            print(k, v)
        end
    end
end