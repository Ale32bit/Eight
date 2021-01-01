local fs = require("filesystem")
local screen = require("screen")

local function load(path)
    local image = {}
    image.palette = {}
    image.pixels = {}
    image.width = 0
    image.height = 0
    image.version = 0;
    
    local offset = 0
    
    local emg = fs.readFile(path, true)
    
    local function nextByte()
        local byte = string.unpack(">B", emg, offset )
        offset = offset + 1
        return byte
    end
    
    local function nextShort()
        local byte = string.unpack(">H", emg, offset )
        --cprint("Reading short at ", offset, string.format("%X", byte))
        offset = offset + 2
        return byte
    end
    
    if emg:sub(1, 11) ~= "\0EIGHTIMAGE" then
        return false, "Invalid emage"
    end
    
    offset = 12
    
    image.version = nextByte()
    
    if nextByte() ~= 0 then
        return false, "Invalid emage"
    end
    
    image.width = nextShort()
    image.height = nextShort()
    
    --cprint("reading palette")
    
    for i = 0, 255 do
        local r = nextByte()
        local g = nextByte()
        local b = nextByte()
        --cprint("Color " .. i, string.format("%02X%02X%02X", r, g, b))
        image.palette[i] = {r, g, b}
    end
    
    for i = 1, image.width * image.height do
        local pixel = nextByte()
        image.pixels[#image.pixels + 1] = pixel
    end
    
    return image, nil
end

local function draw(emg, x, y)
    local dx, dy = 0, 0
    
    for k, v in ipairs(emg.pixels) do
        local color = emg.palette[v]
        
        screen.setPixel(x + dx, y + dy, table.unpack(color))
        --cprint(dx, dy, table.unpack(color))
        dx = dx + 1
        if dx >= emg.width then
            dx = 0
            dy = dy + 1
        end
    end
end

local emg = load("/home/out.emg")

draw(emg, 0,0)