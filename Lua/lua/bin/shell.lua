local term = require("term")
local colors = require("colors")
local fs = require("filesystem")
local expect = require("expect")

local bExit = false

local currentDirectory = "/"
local binPath = "?.lua;?;/bin/?.lua;/bin/?"

local shell = shell

if not shell then
    shell = {}

    local function tokenise(...)
        local sLine = table.concat({...}, " ")
        local tWords = {}
        local bQuoted = false
        for match in string.gmatch(sLine .. '"', '(.-)"') do
            if bQuoted then
                table.insert(tWords, match)
            else
                for m in string.gmatch(match, "[^ \t]+") do
                    table.insert(tWords, m)
                end
            end
            bQuoted = not bQuoted
        end
        return tWords
    end

    local function makeEnv(programName, ...)
        local arg = {...}
        arg[0] = programName
        local tEnv = {
            arg = arg,
            shell = shell
        }
        setmetatable(tEnv, {__index = _ENV})

        return tEnv
    end

    function shell.parse(...)
        local params = table.pack(...)
        local args = {}
        local options = {}
        local doneWithOptions = false
        for i = 1, params.n do
            local param = params[i]
            if not doneWithOptions and type(param) == "string" then
                if param == "--" then
                    doneWithOptions = true -- stop processing options at `--`
                elseif param:sub(1, 2) == "--" then
                    local key, value = param:match("%-%-(.-)=(.*)")
                    if not key then
                        key, value = param:sub(3), true
                    end
                    options[key] = value
                elseif param:sub(1, 1) == "-" and param ~= "-" then
                    for j = 2, utf8.len(param) do
                        options[utf8.sub(param, j, j)] = true
                    end
                else
                    table.insert(args, param)
                end
            else
                table.insert(args, param)
            end
        end
        return args, options
    end

    function shell.getWorkingDirectory()
        return currentDirectory
    end

    function shell.setWorkingDirectory(path)
        expect(1, path, "string")

        local newPath = fs.resolve(path)
        if fs.exists(newPath) and fs.getType(newPath) == "directory" then
            currentDirectory = newPath
        else
            error("Directory not found", 2)
        end
    end

    function shell.resolveProgram(programName)
        for pattern in string.gmatch(binPath, "[^;]+") do
            local sPath = string.gsub(pattern, "%?", programName)
            local resolved = fs.joinPath(currentDirectory, sPath)

            if fs.exists(resolved) and fs.getType(resolved) == "file" then
                return resolved
            end
        end
        return false
    end

    function shell.execute(programName, ...)
        local resolved = shell.resolveProgram(programName)

        for i = 1, select("#", ...) do
            expect(i + 1, select(i, ...), "string")
        end

        if resolved then
            local env = makeEnv(programName, ...)

            local func, err = loadfile(resolved, "bt", env)
            if func then
                local ok, err = pcall(func, ...)
                if ok then
                    return true
                else
                    printError(err)
                    return false
                end
            else
                printError(err)
                return false
            end
        else
            printError("No such program")
            return false
        end
    end

    function shell.run(...)
        local tWords = tokenise(...)
        local sCommand = tWords[1]
        if sCommand then
            return shell.execute(sCommand, table.unpack(tWords, 2))
        end
        return false
    end

    function shell.exit()
        bExit = true
    end

    shell.setWorkingDirectory("/home")
end

local history = {}
os.setRPC("In shell")
while not bExit do
    local cwd = shell.getWorkingDirectory()
    if cwd == "/home" then
        cwd = "~"
    end
    term.setBackground(0x0)
    term.setForeground(colors.yellow)
    write("[" .. cwd .. "]$ ")
    term.setForeground(colors.white)
    local input = term.read(nil, history)

    if input:match("%S") and history[#history] ~= input then
        table.insert(history, input)
    end

    local name = shell.resolveProgram(input)
    if name then
        os.setRPC("In shell", "Running " .. fs.getName(name))
    end

    shell.run(input)

    if name then
        os.setRPC("In shell")
    end
end
