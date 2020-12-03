local fs = require("filesystem")
local http = require("http")

local args = {...}
local url = args[1]
local outPath

if not url then
    print("Usage: wget <url> [file name]")
    return true
end

if args[2] then
    outPath = fs.joinPath(shell.getWorkingDirectory(), args[2])
else
    outPath = fs.joinPath(shell.getWorkingDirectory(), fs.getName(url))
end

local content, err = http.get(url)
if not content then
    printError(err)
    return false
end

fs.writeFile(outPath, content, true)

print("Saved as " .. outPath)

return true