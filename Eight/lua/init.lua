term.setSize(64, 24, 2)

cprint(os);
for k,v in pairs(os) do
    cprint(k, v)
end

local bExit = false
local tHistory = {}

shell = {}

function shell.exit()
    bExit = true
end

function shell.run(program)

end

term.setForeground(33, 150, 243)
print("Eight", os.version())
term.setForeground(255, 255, 255)

while not bExit do
    term.write("> ")
    local input = term.read(nil, tHistory)
    table.insert(tHistory, input)
    shell.run(input)
end 