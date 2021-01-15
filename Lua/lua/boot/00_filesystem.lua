-- Extended FileSystem Library

local fs = require("filesystem")
local expect = require("expect")

local function segments(path)
    local parts = {}
    for part in path:gmatch("[^\\/]+") do
        local current, up = part:find("^%.?%.$")
        if current then
            if up == 2 then
                table.remove(parts)
            end
        else
            table.insert(parts, part)
        end
    end
    return parts
end

function fs.getAbsolutePath(path)
    expect(1, path, "string")
    local result = table.concat(segments(path), "/")
    if string.sub(path, 1, 1) == "/" then
        return "/" .. result
    else
        return result
    end
end
fs.resolve = fs.getAbsolutePath

function fs.joinPath(base, path)
    expect(1, base, "string")
    expect(2, path, "string")
    if path:sub(1, 1) == "/" then
        return fs.getAbsolutePath(path)
    else
        return fs.getAbsolutePath(base .. "/" .. path)
    end
end

function fs.getName(path)
    expect(1, path, "string")
    path = fs.getAbsolutePath(path)
    return string.match(path, "^.+/(.+)$")
end

function fs.getDirectory(path)
    expect(1, path, "string")
    return fs.getAbsolutePath(string.match(path, "^.+/"))
end

function fs.readFile(path, binary)
    expect(1, path, "string")
    expect(2, binary, "boolean", "nil")

    if not fs.exists(path) then
        error("File not found", 2)
    end

    local f = fs.open(path, binary and "rb" or "r")
    local content = f:read("*a")
    f:close()
    return content
end

function fs.writeFile(path, content, binary)
    expect(1, path, "string")
    expect(2, content, "string")
    expect(3, binary, "boolean", "nil")

    local f = fs.open(path, binary and "wb" or "w")
    f:write(content)
    f:close()
end

function fs.appendFile(path, content, binary)
    expect(1, path, "string")
    expect(2, content, "string")
    expect(3, binary, "boolean", "nil")

    local f = fs.open(path, binary and "ab" or "a")
    f:write(content)
    f:close()
end
