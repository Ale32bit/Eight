local fs = require("filesystem")
local term = require("term")
local colors = require("colors")

local function printUsage()
    print("Usage: ls [-a] [-c] [directory]" .. [[

-a Show all files
-c Disable colors
--help Display this message]])
end

local args, options = shell.parse(...)

if options.help then
    return printUsage()
end

local directory

if args[1] then
    directory = fs.joinPath(shell.getWorkingDirectory(), args[1])
else
    directory = shell.getWorkingDirectory()
end

local noColor = options.c or false
local allFiles = options.a or false

local list, err = fs.list(directory)

local printed = false

if list then
    for k,v in ipairs(list) do
        if not string.match(v, "^%.") or (string.match(v, "^%.") and allFiles) then
            if not noColor then
                if fs.getType(fs.joinPath(directory, v)) == "directory" then
                    term.setForeground(colors.lime)
                else
                    term.setForeground(colors.white)
                end
            end
            write(v .. "\t")
            printed = true
        end
    end
    if printed then
        print()
    end
else
    printError(err)
    return false
end

return true