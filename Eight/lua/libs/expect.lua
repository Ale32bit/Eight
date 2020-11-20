local function contains(tbl, val)
    for k,v in ipairs(tbl) do
        if v == val then
            return true
        end
    end
    return false
end

return function(i, v, ...)
    local types = {...}
    local vType = type(v)
    if not contains(types, vType) then
        error(("bad argument #%d (expected %s, got %s)"):format(i, table.concat(types, ", "), vType), 3)
    end
    
    return true
end