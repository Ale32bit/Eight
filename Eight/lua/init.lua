local term = require("term")

local function main()
    term.setPos(0,0)
    term.print("Eight", os.version())
end

local ok, err = pcall(main)
if not ok then
    panic(err)
end