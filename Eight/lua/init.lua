local term = require("term")
local screen = require("screen")
local event = require("event")
local colors = require("colors")
local fs = require("filesystem")
local expect = require("expect")

local shell = {}

local currentDirectory = "/home"
local binPath = "?.lua;?;/bin/?.lua;/bin/?"

local function tokenise(...)
    local sLine = table.concat({ ... }, " ")
    local tWords = {}
    local bQuoted = false
    for match in string.gmatch(sLine .. "\"", "(.-)\"") do
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
    local tEnv = {
        args = {...},
        shell = shell,
    }
    setmetatable(tEnv, {__index = _ENV})
    
    return tEnv
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

        local fileContent = fs.readFile(resolved, true)

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

local w, h, s = term.getSize()
term.setSize(w * 2, h * 2, s)

term.setPos(0,0)
term.setForeground(0x21, 0x96, 0xf3)
print("Eight", os.version())
term.setForeground(colors.white)

local history = {}
while true do
    local cwd = shell.getWorkingDirectory()
    if cwd == "/home" then
        cwd = "~"
    end
    term.setForeground(colors.yellow)
    write(cwd .. "$ ")
    term.setForeground(colors.white)
    local input = term.read(nil, history)
    
    if input:match("%S") and history[#history] ~= input then
        table.insert(history, input)
    end
    
    shell.run(input)
end

