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
    fgColor = { r, g, b }
end

function term.setBackground(r, g, b)
    bgColor = { r, g, b }
end

function term.write(...)
    local chunks = {}
    for k, v in ipairs({ ... }) do
        chunks[#chunks + 1] = tostring(v)
    end

    local text = table.concat(chunks, " ")

    for _, char in utf8.codes(text) do

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
    end
end

function term.print(...)
    if posX >= width then
        term.scroll(-1)
    end
    local chunks = {}
    for k, v in ipairs({ ... }) do
        chunks[#chunks + 1] = tostring(v)
    end

    local text = table.concat(chunks, " ")

    term.write(text, "\n")
end

function term.read(sReplace, tHistory)
    local bExit = false
    local content = ""
    local startx = posX
    local starty = posY
    local nPos = 0
    local tblSelection = 0
    
    local blink = true

    local function redraw(extra)
        extra = extra or 0
        screen.drawRectangle(startx * fontWidth, starty * fontHeight, fontWidth * (#content + extra), fontHeight, table.unpack(bgColor))
        posX = startx
        if sReplace then
            term.write(string.rep(utf8.sub(sReplace, 1, 1), utf8.len(content)))
        else
            term.write(content)
        end
    end

    local blinkTimer = timer.start(500);

    while not bExit do
        local ev = { event.pull() }

        if ev[1] == "char" then
            content = utf8.sub(content, 1, nPos) .. ev[2] .. utf8.sub(content, nPos + 1)
            nPos = nPos + 1

            redraw()
        elseif ev[1] == "key_down" then
            local key = ev[3]
            if key == "return" then
                blink = false
                redraw()
                break
            elseif key == "backspace" then
                -- backspace
                if nPos > 0 then
                    content = utf8.sub(content, 1, nPos - 1) .. utf8.sub(content, nPos + 1)
                    nPos = nPos - 1
                    blink = false
                    redraw(1)
                end
            elseif key == "left" then
                if nPos >= 1 then
                    blink = false
                    nPos = nPos - 1
                end
            elseif key == "right" then
                if nPos < #content then
                    blink = false
                    nPos = nPos + 1
                end
            elseif key == "up" then
                if tHistory then
                    if tblSelection == 0 then
                        tblSelection = #tHistory
                    elseif tblSelection > 1 then
                        tblSelection = tblSelection - 1
                    end

                    if not tHistory[tblSelection] then
                        content = "";
                    end

                    local oldLength = utf8.len(content)
                    content = tHistory[tblSelection] or ""
                    nPos = utf8.len(content)
                    blink = false
                    redraw(oldLength - utf8.len(content));
                end
            elseif key == "down" then
                if tHistory then
                    if tblSelection ~= 0 then
                        if tblSelection <= #tHistory then
                            tblSelection = tblSelection + 1
                        end
                        if tblSelection > #tHistory then
                            tblSelection = 0
                        end

                        local oldLength = utf8.len(content)
                        content = tHistory[tblSelection] or ""
                        nPos = utf8.len(content)
                        blink = false
                        redraw(oldLength - utf8.len(content));
                    end
                end
            elseif key == "home" then
                blink = false
                nPos = 0
            elseif key == "end" then
                blink = false
                nPos = #content
            elseif key == "delete" then
                if nPos < #content then
                    blink = false
                    content = utf8.sub(content, 1, nPos) .. utf8.sub(content, nPos + 2)
                    redraw(1)
                end
            end

        elseif ev[1] == "timer" then
            blinkTimer = timer.start(500);
            blink = not blink
            redraw()
        end
    end

    term.write("\n")
    return content
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
