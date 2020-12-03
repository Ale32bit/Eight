local fs = require("filesystem")

local args = {...}
local directory

if args[1] then
    directory = fs.joinPath(shell.getWorkingDirectory(), args[1])
else
    directory = fs.resolve("/home")
end

if fs.exists(directory) and fs.getType(directory) == "directory" then
    shell.setWorkingDirectory(directory)
else
    printError("Directory not found")
    return false
end

return true