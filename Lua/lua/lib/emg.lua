local fs = require("filesystem")
local graphics = require("graphics")

local function load(path)
    local emg = {}
    emg.pixels = {}
    emg.width = 0
    emg.height = 0
    emg.version = 2

    local offset = 0

    local data = fs.readFile(path, true)

    local function nextByte()
        local byte = string.unpack(">B", data, offset )
        offset = offset + 1
        return byte
    end
    
    local function nextShort()
        local short = string.unpack(">H", data, offset )
        offset = offset + 2
        return short
    end

    if data:sub(1, 11) ~= "\0EIGHTIMAGE" then
        return false, "Invalid emage"
    end

    offset = 12

    emg.version = nextByte();

    if nextByte() ~= 0 then
        return false, "Invalid emage"
    end

    emg.width = nextShort()
    emg.height = nextShort()

    for i = 1, emg.width * emg.height do
        local r, g, b = nextByte(), nextByte(), nextByte()
        local x, y = i % emg.width, i // emg.width
        local color = (r << 16) | (g << 8) | b;

        if not emg.pixels[color] then
            emg.pixels[color] = {}
        end

        table.insert(emg.pixels[color], x)
        table.insert(emg.pixels[color], y)
    end

    return emg, nil
end

local function draw(emg, --[[x, y]])
    local dx, dy = 0, 0
	
	-- TODO: work delta coords out

    for color, pixels in pairs(emg.pixels) do
        graphics.drawPixels(pixels, color)
        dx = dx + 1
        if dx >= emg.width then
            dx = 0
            dy = dy + 1
        end
    end
end

return {
    load = load,
    draw = draw,
}