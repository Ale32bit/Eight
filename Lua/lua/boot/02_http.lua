-- Extended HTTP Library

local http = require("http")
local event = require("event")
local expect = require("expect")

function http.request(url, body, headers, method)
    expect(1, url, "string")
    expect(2, body, "string", "nil")
    expect(3, headers, "table", "nil")
    expect(4, method, "string", "nil")
    
    method = method or "GET"
    
    local ok, par = http.requestAsync(url, body, headers, method)
    
    if ok then
        local ev = {}
        repeat
            ev = {event.pull("http_success", "http_failure")}
        until ev[2] == par
        
        if ev[1] == "http_success" then
            return ev[3], nil
        elseif ev[1] == "http_failure" then
            return false, ev[3]
        end
    else
        return false, par
    end
end

function http.get(url, headers)
    expect(1, url, "string")
    expect(2, headers, "table", "nil")
    
    local ok, par = http.requestAsync(url, nil, headers, "GET")
    
    if ok then
        local ev = {}
        repeat
            ev = {event.pull("http_success", "http_failure")}
        until ev[2] == par
        
        if ev[1] == "http_success" then
            return ev[3], nil
        elseif ev[1] == "http_failure" then
            return false, ev[3]
        end
    else
        return false, par
    end
end

function http.post(url, body, headers)
    expect(1, url, "string")
    expect(2, body, "string", "nil")
    expect(3, headers, "table", "nil")
    
    local ok, par = http.requestAsync(url, body, headers, "POST")
    
    if ok then
        local ev = {}
        repeat
            ev = {event.pull("http_success", "http_failure")}
        until ev[2] == par
        
        if ev[1] == "http_success" then
            return ev[3], nil
        elseif ev[1] == "http_failure" then
            return false, ev[3]
        end
    else
        return false, par
    end
end