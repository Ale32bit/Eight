local fs = require("filesystem")
local term = require("term")
local colors = require("colors")

local args = {...}
local directory

if args[1] then
    directory = fs.joinPath(shell.getWorkingDirectory(), args[1])
else
    directory = shell.getWorkingDirectory()
end

local list, err = fs.list(directory)

if list then
    for k,v in ipairs(list) do
        if fs.getType(fs.joinPath(directory, v)) == "directory" then
            term.setForeground(colors.lime)
        else
            term.setForeground(colors.white)
        end
        write(v .. "\t")
    end
    print()
else
    printError(err)
    return false
end

return true