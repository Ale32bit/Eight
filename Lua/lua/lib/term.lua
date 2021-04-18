-- Terminal library, for text mode related functions

local screen = require("screen")
local graphics = require("graphics")
local event = require("event")
local timer = require("timer")
local expect = require("expect")
local colors = require("colors")

local term = {}

local posX = 0
local posY = 0

local blinkDelay = 500
local isBlinking = false
local timerBlinkId

local initiated = false

if not utf8.sub then
    local function sub(s, i, j)
        expect(1, s, "string")
        expect(2, i, "number")
        expect(3, j, "number", "nil")
        return string.sub(s, utf8.offset(s, i), j and (utf8.offset(s, j + 1) - 1) or #s)
    end
end

local function isValidUtf8(str)
    local i, len = 1, #str
    while i <= len do
        if i == string.find(str, "[%z\1-\127]", i) then
            i = i + 1
        elseif i == string.find(str, "[\194-\223][\128-\191]", i) then
            i = i + 2
        elseif
            i == string.find(str, "\224[\160-\191][\128-\191]", i) or
                i == string.find(str, "[\225-\236][\128-\191][\128-\191]", i) or
                i == string.find(str, "\237[\128-\159][\128-\191]", i) or
                i == string.find(str, "[\238-\239][\128-\191][\128-\191]", i)
         then
            i = i + 3
        elseif
            i == string.find(str, "\240[\144-\191][\128-\191][\128-\191]", i) or
                i == string.find(str, "[\241-\243][\128-\191][\128-\191][\128-\191]", i) or
                i == string.find(str, "\244[\128-\143][\128-\191][\128-\191]", i)
         then
            i = i + 4
        else
            return false, i
        end
    end

    return true
end

local function sub(s, i, j)
    if isValidUtf8(s) then
        return utf8.sub(s, i, j)
    else
        return string.sub(s, i, j)
    end
end

local function len(s)
    if isValidUtf8(s) then
        return utf8.len(s)
    else
        return #s
    end
end

local function writeChar(c)
    screen.setChar(c or " ", math.floor(posX), math.floor(posY))
    posX = posX + 1
end

local function redrawChar(x, y)
    local w, h = term.getSize()
    if (x < 0 or y < 0 or x >= w or y >= h) then
        return
    end
    local ofg, obg = screen.getForeground(), screen.getBackground()

    local c, fg, bg = screen.getChar(x, y)

    screen.setForeground(fg)
    screen.setBackground(bg)
    screen.setChar(c, x, y)
    screen.setForeground(ofg)
    screen.setBackground(obg)
end

local function clear()
    screen.clear()
end

local function getCellSize()
    local w, h = screen.getSize()
    local rw, rh = screen.getRealSize()
    local cw, ch = rw / w, rh / h

    return cw, ch
end

local function getRealPos(x, y)
    local cw, ch = getCellSize()

    return cw * x, ch * y
end

local function drawCursor()
    if isBlinking then
        local rw, rh = screen.getRealSize()
        local cw, ch = getCellSize()
        graphics.drawLine(posX * cw, posY * ch + 2, posX * cw, posY * ch + ch - 2, screen.getForeground())
    end
end

function term.setSize(w, h, s)
    expect(1, w, "number")
    expect(2, h, "number")
    expect(1, s, "number", "nil")

    local ow, oh, os = screen.getSize()

    w = math.floor(w)
    h = math.floor(h)

    screen.setSize(w, h, s or os)

    clear(true)
end

function term.getSize()
    return screen.getSize()
end

function term.setPos(x, y)
    expect(1, x, "number")
    expect(2, y, "number")

    local oldX, oldY = posX, posY
    posX = x
    posY = y

    redrawChar(oldX, oldY)
    drawCursor()
end

function term.getPos()
    return posX, posY
end

function term.setForeground(r, g, b)
    expect(1, r, "number", "integer")
    expect(1, g, "number", "nil")
    expect(1, b, "number", "nil")

    if g and b then
        r = colors.toHex(r, g, b)
    end

    screen.setForeground(r)
end

function term.setBackground(r, g, b)
    expect(1, r, "number", "integer")
    expect(1, g, "number", "nil")
    expect(1, b, "number", "nil")

    if g and b then
        r = colors.toHex(r, g, b)
    end

    screen.setBackground(r)
end

function term.getForeground()
    return screen.getForeground()
end

function term.getBackground()
    return screen.getBackground()
end

function term.write(...)
    local width, height = term.getSize()

    local chunks = {}
    for k, v in ipairs({...}) do
        chunks[#chunks + 1] = tostring(v)
    end

    local text = table.concat(chunks, " ")

    local function iterate(char)
        if char == "\n" then
            posY = posY + 1
            if posY >= height then
                term.scroll(-1)
                posY = height - 1
            end
            posX = 0
        elseif char == "\r" then
            posX = 0
        elseif char == "\t" then
            posX = posX + 2
        elseif char ~= "\13" then
            writeChar(char)
        end

        if posX >= width then
            posX = 0
            posY = posY + 1
        end
    end

    if isValidUtf8(text) then
        for _, char in utf8.codes(text) do
            iterate(utf8.char(char))
        end
    else
        for char in string.gmatch(text, "(.)") do
            iterate(char)
        end
    end
end

function term.clear()
    clear()
end

function term.clearLine()
    local w = term.getSize()
    for i = 0, w do
        screen.setChar(" ", i, posY)
    end
end

function term.scroll(n)
    expect(1, n, "number")

    screen.scroll(n)
end

function term.setBlinking(blink)
    expect(1, blink, "boolean")
    isBlinking = blink
end

function term.getBlinking()
    return isBlinking
end

function term.read(_sReplaceChar, _tHistory, _fnComplete, _sDefault)
    expect(1, _sReplaceChar, "string", "nil")
    expect(2, _tHistory, "table", "nil")
    expect(3, _fnComplete, "function", "nil")
    expect(4, _sDefault, "string", "nil")

    term.setBlinking(true)

    local sLine
    if type(_sDefault) == "string" then
        sLine = _sDefault
    else
        sLine = ""
    end
    local nHistoryPos
    local nPos, nScroll = len(sLine), 0
    if _sReplaceChar then
        _sReplaceChar = string.sub(_sReplaceChar, 1, 1)
    end

    local tCompletions
    local nCompletion
    local function recomplete()
        if _fnComplete and nPos == len(sLine) then
            tCompletions = _fnComplete(sLine)
            if tCompletions and #tCompletions > 0 then
                nCompletion = 1
            else
                nCompletion = nil
            end
        else
            tCompletions = nil
            nCompletion = nil
        end
    end

    local function uncomplete()
        tCompletions = nil
        nCompletion = nil
    end

    local w = term.getSize()
    local sx = term.getPos()

    local function redraw(_bClear)
        local cursor_pos = nPos - nScroll
        if sx + cursor_pos > w then
            -- We've moved beyond the RHS, ensure we're on the edge.
            nScroll = sx + nPos - w
        elseif cursor_pos < 0 then
            -- We've moved beyond the LHS, ensure we're on the edge.
            nScroll = nPos
        end

        local _, cy = term.getPos()
        term.setPos(sx, cy)
        local sReplace = _bClear and " " or _sReplaceChar
        if sReplace then
            term.write(string.rep(sub(sReplace, 1, 1), math.max(len(sLine) - nScroll, 0)))
        else
            term.write(sub(sLine, nScroll + 1))
        end

        if nCompletion then
            local sCompletion = tCompletions[nCompletion]
            local oldText, oldBg
            if not _bClear then
                oldText = term.getForeground()
                oldBg = term.getBackground()
                term.setForeground(colors.white)
                term.setBackground(colors.gray)
            end
            if sReplace then
                term.write(string.rep(sReplace, #sCompletion))
            else
                term.write(sCompletion)
            end
            if not _bClear then
                term.setForeground(oldText)
                term.getBackground(oldBg)
            end
        end

        term.setPos(sx + nPos - nScroll, cy)
    end

    local function clear()
        redraw(true)
    end

    recomplete()
    redraw()

    local function acceptCompletion()
        if nCompletion then
            -- Clear
            clear()

            -- Find the common prefix of all the other suggestions which start with the same letter as the current one
            local sCompletion = tCompletions[nCompletion]
            sLine = sLine .. sCompletion
            nPos = len(sLine)

            -- Redraw
            recomplete()
            redraw()
        end
    end
    while true do
        local sEvent, param, param1, param2 = event.pull()
        if sEvent == "char" then
            -- Typed key
            clear()
            sLine = sub(sLine, 1, nPos) .. param .. sub(sLine, nPos + 1)
            nPos = nPos + 1
            recomplete()
            redraw()
        elseif sEvent == "paste" then
            -- Pasted text
            clear()
            sLine = string.sub(sLine, 1, nPos) .. param .. string.sub(sLine, nPos + 1)
            nPos = nPos + #param
            recomplete()
            redraw()
        elseif sEvent == "key_down" then
            if param == "return" then
                -- Enter
                if nCompletion then
                    clear()
                    uncomplete()
                    redraw()
                end
                break
            elseif param == "left" then
                -- Left
                if nPos > 0 then
                    clear()
                    nPos = nPos - 1
                    recomplete()
                    redraw()
                end
            elseif param == "right" then
                -- Right
                if nPos < len(sLine) then
                    -- Move right
                    clear()
                    nPos = nPos + 1
                    recomplete()
                    redraw()
                else
                    -- Accept autocomplete
                    acceptCompletion()
                end
            elseif param == "up" or param == "down" then
                -- Up or down
                if nCompletion then
                    -- Cycle completions
                    clear()
                    if param == "up" then
                        nCompletion = nCompletion - 1
                        if nCompletion < 1 then
                            nCompletion = #tCompletions
                        end
                    elseif param == "down" then
                        nCompletion = nCompletion + 1
                        if nCompletion > #tCompletions then
                            nCompletion = 1
                        end
                    end
                    redraw()
                elseif _tHistory then
                    -- Cycle history
                    clear()
                    if param == "up" then
                        -- Up
                        if nHistoryPos == nil then
                            if #_tHistory > 0 then
                                nHistoryPos = #_tHistory
                            end
                        elseif nHistoryPos > 1 then
                            nHistoryPos = nHistoryPos - 1
                        end
                    else
                        -- Down
                        if nHistoryPos == #_tHistory then
                            nHistoryPos = nil
                        elseif nHistoryPos ~= nil then
                            nHistoryPos = nHistoryPos + 1
                        end
                    end
                    if nHistoryPos then
                        sLine = _tHistory[nHistoryPos]
                        nPos, nScroll = len(sLine), 0
                    else
                        sLine = ""
                        nPos, nScroll = 0, 0
                    end
                    uncomplete()
                    redraw()
                end
            elseif param == "backspace" then
                -- Backspace
                if nPos > 0 then
                    clear()
                    sLine = sub(sLine, 1, nPos - 1) .. sub(sLine, nPos + 1)
                    nPos = nPos - 1
                    if nScroll > 0 then
                        nScroll = nScroll - 1
                    end
                    recomplete()
                    redraw()
                end
            elseif param == "home" then
                -- Home
                if nPos > 0 then
                    clear()
                    nPos = 0
                    recomplete()
                    redraw()
                end
            elseif param == "delete" then
                -- Delete
                if nPos < len(sLine) then
                    clear()
                    sLine = sub(sLine, 1, nPos) .. sub(sLine, nPos + 2)
                    recomplete()
                    redraw()
                end
            elseif param == "end" then
                -- End
                if nPos < len(sLine) then
                    clear()
                    nPos = len(sLine)
                    recomplete()
                    redraw()
                end
            elseif param == "tab" then
                -- Tab (accept autocomplete)
                acceptCompletion()
            end
        elseif sEvent == "mouse_click" or sEvent == "mouse_drag" and param == 1 then
            local _, cy = term.getPos()
            if param1 >= sx and param1 <= w and param2 == cy then
                -- Ensure we don't scroll beyond the current line
                nPos = math.min(math.max(nScroll + param1 - sx, 0), len(sLine))
                redraw()
            end
        elseif sEvent == "term_resize" then
            -- Terminal resized
            w = term.getSize()
            redraw()
        end
    end

    local _, cy = term.getPos()
    term.setBlinking(false)
    term.setPos(w + 1, cy)
    print()

    return sLine
end

function term.run()
    if initiated then
        return
    end
    initiated = true

    timerBlinkId = timer.start(blinkDelay)
    local blink = false
    while true do
        local _, timerId = coroutine.yield("timer")
        if timerId == timerBlinkId then
            if isBlinking then
                redrawChar(posX, posY)
                blink = not blink
                if blink then
                    drawCursor()
                end
            end
            timerBlinkId = timer.start(blinkDelay)
        end
    end
end

return term
