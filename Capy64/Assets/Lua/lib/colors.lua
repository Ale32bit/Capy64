local expect = require("expect")

local palette = {
    {
        "white",
        0xf0f0f0
    },
    {
        "orange",
        0xf2b233
    },
    {
        "magenta",
        0xe57fd8
    },
    {
        "lightBlue",
        0x99b2f2
    },
    {
        "yellow",
        0xdede6c
    },
    {
        "lime",
        0x7fcc19
    },
    {
        "pink",
        0xf2b2cc
    },
    {
        "gray",
        0x4c4c4c
    },
    {
        "lightGray",
        0x999999
    },
    {
        "cyan",
        0x4c99b2
    },
    {
        "purple",
        0xb266e5
    },
    {
        "blue",
        0x3366cc
    },
    {
        "brown",
        0x7f664c
    },
    {
        "green",
        0x57a64e
    },
    {
        "red",
        0xcc4c4c
    },
    {
        "black",
        0x111111
    }
}

local colors = {}
for k, v in ipairs(palette) do
    colors[v[1]] = v[2]
    colors[k] = v[2]
end

function colors.packRGB(r, g, b)
    expect(1, r, "number")
    expect(2, g, "number")
    expect(3, b, "number")

    return (r << 16) +
        (g << 8) +
        b
end

function colors.unpackRGB(rgb)
    expect(1, rgb, "number")

    return (rgb >> 16) & 0xff,
        (rgb >> 8) & 0xff,
        rgb & 0xff
end

return colors;
