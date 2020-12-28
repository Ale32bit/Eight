local colors = {
    white = 0xf0f0f0,
    orange = 0xf2b233,
    magenta = 0xe57fd8,
    lightBlue = 0x99b2f2,
    yellow = 0xdede6c,
    lime = 0x7fcc19,
    pink = 0xf2b2cc,
    gray = 0x4c4c4c,
    lightGray = 0x999999,
    cyan = 0x4c99b2,
    purple = 0xb266e5,
    blue = 0x3366cc,
    brown = 0x7f664c,
    green = 0x57a64e,
    red = 0xcc4c4c,
    black = 0x111111,
}

function colors.toRGB(n)
    local b = n & 0xff
    local g = (n >> 8) & 0xff
    local r = (n >> 16) & 0xff

    return r, g, b
end

function colors.toHex(r, g, b)
    return (r << 16) | (g << 8) | b
end

function colors.toGrayscale(r, g, b)
    if not g or not b then
        r, g, b = colors.toRGB(r)
    end

    return (r + g + b) / 3
end

return colors;