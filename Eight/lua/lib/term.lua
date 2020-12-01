local screen = require("screen");
local font = require("fonts.tewi")
local grid = {}
local term = {}

local fontWidth = font._width or 0
local fontHeight = font._height or 0
local width, height, scale = 0, 0, 1

local fgColor = { 0xff, 0xff, 0xff }
local bgColor = { 0, 0, 0 }

local posX = 0
local posY = 0

local initiated = false

if not utf8.sub then
    function utf8.sub(s, i, j)
        return string.sub(s, utf8.offset(s, i), j and (utf8.offset(s, j + 1) - 1) or #s)
    end
end

function utf8.isValid(str)
  local i, len = 1, #str
  while i <= len do
    if     i == string.find(str, "[%z\1-\127]", i) then i = i + 1
    elseif i == string.find(str, "[\194-\223][\128-\191]", i) then i = i + 2
    elseif i == string.find(str,        "\224[\160-\191][\128-\191]", i)
        or i == string.find(str, "[\225-\236][\128-\191][\128-\191]", i)
        or i == string.find(str,        "\237[\128-\159][\128-\191]", i)
        or i == string.find(str, "[\238-\239][\128-\191][\128-\191]", i) then i = i + 3
    elseif i == string.find(str,        "\240[\144-\191][\128-\191][\128-\191]", i)
        or i == string.find(str, "[\241-\243][\128-\191][\128-\191][\128-\191]", i)
        or i == string.find(str,        "\244[\128-\143][\128-\191][\128-\191]", i) then i = i + 4
    else
      return false, i
    end
  end

  return true
end

local function setChar(x, y, char, fg, bg)
    if x >= 0 and y >= 0 and x < width and y < height then
        grid[posY] = grid[posY] or {}
        grid[posY][posX] = {
            char, fg, bg
        }
    end
end

local function getChar(x, y)
    grid[posY] = grid[posY] or {}
    return grid[posY][posX]
end

local function drawChar(c, fg, bg, noset)
    local char = font[c] or font[string.byte("?")] or { {} }
    fg = fg or fgColor
    bg = bg or bgColor
    underline = underline or false

    local deltaX, deltaY = posX * fontWidth, posY * fontHeight

    local charWidth = #char[1]
    deltaX = deltaX + math.ceil((fontWidth / 2) - (charWidth / 2))

    screen.drawRectangle(
            posX * fontWidth,
            posY * fontHeight,
            fontWidth,
            fontHeight,
            table.unpack(bg)
    )

    for y = 1, #char do
        for x = 1, #char[y] do
            if char[y][x] == 1 then
                screen.setPixel(deltaX + x, deltaY + y - 1, table.unpack(fg))
            end
        end
    end

    if not noset then
        setChar(posX, posY, c, fg, bg)
    end
    posX = posX + 1

end

local function redraw()
    local cx, cy = term.getPos()
    for y, row in pairs(grid) do
        for x, char in pairs(row) do
            term.setPos(x, y)
            if char[1] then
                drawChar(char[1], char[2] or fgColor, char[3] or bgColor, true)
            end
        end
    end

    term.setPos(cx, cy)
end

function term.setSize(w, h, s)
    local ow, oh, os = screen.getSize()

    w = math.floor(w)
    h = math.floor(h)

    screen.setSize(w * fontWidth, h * fontHeight, s or os)

    width = w
    height = h
    scale = s or os

    grid = {}

    for y = 1, h do
        grid[y] = {}
        for x = 1, w do
            grid[y][x] = {}
        end
    end
end

function term.getSize()
    return width, height, scale
end

function term.setPos(x, y)
    posX = x
    posY = y
end

function term.getPos()
    return posX, posY
end

function term.setForeground(r, g, b)
    if type(r) == "table" then
        fgColor = r
        return    
    end
    fgColor = { r, g, b }
end

function term.setBackground(r, g, b)
    if type(r) == "table" then
        fgColor = r
        return    
    end
    bgColor = { r, g, b }
end

function term.getForeground()
    return table.unpack(fgColor)
end

function term.getBackground()
    return table.unpack(bgColor)
end

function term.write(...)
    local chunks = {}
    for k, v in ipairs({...}) do
        chunks[#chunks + 1] = tostring(v)
    end

    local text = table.concat(chunks, " ")

    local function iterate(char)
        if char == 10 then
            posY = posY + 1
            if posY >= height then
                term.scroll(-1)
                posY = height - 1
            end
            posX = 0
        elseif char == 9 then
            posX = posX + 2
        elseif char ~= 13 then
            drawChar(char)
        end

        if posX >= width then
            posX = 0
            posY = posY + 1
        end
    end

    if utf8.isValid(text) then
        for _, char in utf8.codes(text) do
            iterate(char)
        end
    else
        for char in string.gmatch(text, "(.)") do
            iterate(char)
        end
    end
end


function term.clear()
    screen.clear()
    local w, h = screen.getSize()
    screen.drawRectangle(0, 0, w, h, table.unpack(bgColor))
end

function term.clearLine()
    local w, h = screen.getSize()
    screen.drawRectangle(0, posY * fontHeight, w, fontWidth, table.unpack(bgColor))
end

function term.scroll(n)
    local copy = {}
    for k, v in pairs(grid) do
        copy[k + 1] = v
    end

    if n < 0 then
        for i = 1, math.abs(n) do
            table.remove(copy, 1)
            table.insert(copy, #copy, {})
        end
    end

    if n > 0 then
        for i = 1, math.abs(n) do
            table.insert(copy, 1, {})
            table.remove(copy, #copy)
        end
    end

    grid = {}
    for k, v in pairs(copy) do
        grid[k - 1] = v
    end

    term.clear()
    redraw()
end

function term.init()
    if initiated then
        return
    end
    initiated = true

    local w, h, s = screen.getSize()

    term.setSize(
            math.floor(w / fontWidth),
            math.floor(h / fontHeight),
            s
    )
end

return term
