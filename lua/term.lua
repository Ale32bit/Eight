local font = require("lua.fonts.tewi")
local grid = {}

local fontWidth = font._width or 0
local fontHeight = font._height or 0
local width, height, scale = 0, 0, 1

local fgColor = {0xff, 0xff, 0xff}
local bgColor = {0, 0, 0}

local posX = 0
local posY = 0

local function redraw()
	for i, y in pairs(grid) do
		for j, x in pairs(y) do
			drawChar(x[1], x[2], x[3], true)
		end
	end
end

local function drawChar(c, fg, bg, noset)
	local char = font[c] or font["?"] or {{}}
	fg = fg or fgColor
	bg = bg or bgColor
	
	local deltaX, deltaY = posX * fontWidth, posY * fontHeight
	
	local charWidth = #char[1]
	deltaX = deltaX + math.ceil((fontWidth / 2) - (charWidth / 2))
	
	screen.drawRectangle(
		posX * fontWidth,
		posY *  fontHeight,
		fontWidth,
		fontHeight,
		table.unpack(bg)
	)
	
	for y = 1, #char do
		for x = 1, #char[y] do
			if char[y][x] == 1 then
				screen.setPixel(deltaX + x, deltaY + y - 1, table.unpack(fg) )
			end
		end
	end
	
	if not noset then
		grid[posY] = grid[posY] or {}
		grid[posY][posX] = {
			c, fg, bg
		}
	end
	posX = posX + 1
	
end

local term = {}

function term.setSize(w, h, s)
	local ow, oh, os = screen.getSize()
	
	w = math.floor(w)
	h = math.floor(h)
	
	screen.setSize(w * fontWidth, h * fontHeight, s or os)
	
	width = w
	height = h
	scale = s or os
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
	fgColor = {r, g, b}
end

function term.setBackground(r, g, b)
	bgColor = {r, g, b}
end

function term.write(...)
	local chunks = {}
	for k, v in ipairs({...}) do
		chunks[#chunks+1] = tostring(v)
	end
	
	local text = table.concat(chunks, " ")
	
	for i = 1, #text do
		
		local char = text:sub(i,i)
		if char == "\n" then
			posY = posY + 1
			posX = 0
		elseif char == "\t" then
			posX = posX + 2
		else
			drawChar(char)
		end
	end
end

function term.print(...)
	term.write(..., "\n")
end

function term.read(sReplace)
	local bExit = false
	local content = ""
	local startx = posX
	local starty = posY
	local nPos = 0
	
	local function redraw(extra)
		extra = extra or 0
		screen.drawRectangle(startx * fontWidth, starty * fontHeight, fontWidth * (#content + extra), fontHeight, table.unpack(bgColor))
		posX = startx
		if sReplace then
			term.write(string.rep(sReplace:sub(1,1), #content))
		else
			term.write(content)
		end
	end
	
	while not bExit do
		local ev = {event.pull()}
		
		if ev[1] == "char" then
			content = string.sub(content, 1, nPos) .. ev[2] .. string.sub(content, nPos + 1)
			nPos = nPos + 1
			
			redraw()
		elseif ev[1] == "key_down" then
			local key = ev[3]
			if key == "return" then -- enter
				break
			elseif key == "backspace" then -- backspace
				if nPos > 0 then
					content = string.sub(content, 1, nPos - 1) .. string.sub(content, nPos + 1)
					nPos = nPos - 1
					redraw(1)
				end
			elseif key == "left" then
				if nPos > 1 then
					nPos = nPos - 1
				end
			elseif key == "right" then
				if nPos < #content then
					nPos = nPos + 1
				end
			elseif key == "home" then
				nPos = 0
			elseif key == "end" then
				nPos = #content
			elseif key == "delete" then
				if nPos < #content then
                    content = string.sub(content, 1, nPos) .. string.sub(content, nPos + 2)
                end
				redraw(1)
			end
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

return term