local http = require("http");
local expect = require("expect");

function http.get(url, headers)
    expect(1, url, "string");
    expect(2, headers, "table", "nil");
    
    local ok, par = http.request(url, nil, headers, "GET")
    
    if ok then
        local ev = {};
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