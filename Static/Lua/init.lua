print("Eight 2 Alpha")

-- just printing events and more
while true do
    local ev = table.pack(coroutine.yield())
    for i = 1, ev.n do
        if type(ev[i]) == "table" then
            for k, v in pairs(ev[i]) do
                print("-", k, v)
            end
        else
            print(i, ev[i])
        end
    end
    print("-------------------------")
end