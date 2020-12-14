local fs = require("filesystem")
local args, options = shell.parse(...)

local function printUsage()
	print("Usage: mv [-o] <source> <dest>\n-o Overwrite destination")
	return true
end

if #args < 2 or options.help then
	return printUsage()
end



local source = fs.joinPath(shell.getWorkingDirectory(), args[1])
local dest = fs.joinPath(shell.getWorkingDirectory(), args[2])

local ok, err = fs.move(source, dest, options.o)
if not ok then
	printError(err)
	return false
end

return true