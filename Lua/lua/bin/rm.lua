local fs = require("filesystem")

local args, options = shell.parse(...)

local function printUsage()
    print("Usage: rm [-r] <path>" .. [[

-r Recursive
--help Display this message]])
end

if #args == 0 or options.help then
	printUsage()
	return true
end

local path = fs.joinPath(shell.getWorkingDirectory(), args[1])
local recursive = options.r

local ok, err = fs.delete(fs.getAbsolutePath(path), recursive)
if not ok then
	printError(err)
	return false
end