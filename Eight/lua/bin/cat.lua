local fs = require("filesystem")

local args = {...}

local path
if args[1] then
    path = fs.joinPath(shell.getWorkingDirectory(), args[1])
else
    print("Usage: cat <file>")
    return true
end

if fs.exists(path) and fs.getType(path) == "file" then
    local content = fs.readFile(path)
    
    print(content)
    return true
else
    printError("File not found")
    return false
end