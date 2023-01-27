-- Credits: https://github.com/Ocawesome101/recrafted

-- cc.expect

local _expect = {}

local function checkType(index, valueType, value, ...)
    local expected = table.pack(...)
    local isType = false

    for i = 1, expected.n, 1 do
        if type(value) == expected[i] then
            isType = true
            break
        end
    end

    if not isType then
        error(string.format("bad %s %s (%s expected, got %s)", valueType,
            index, table.concat(expected, " or "), type(value)), 3)
    end

    return value
end

function _expect.expect(index, value, ...)
    return checkType(("#%d"):format(index), "argument", value, ...)
end

function _expect.field(tbl, index, ...)
    _expect.expect(1, tbl, "table")
    _expect.expect(2, index, "string")
    return checkType(("%q"):format(index), "field", tbl[index], ...)
end

function _expect.range(num, min, max)
    _expect.expect(1, num, "number")
    _expect.expect(2, min, "number", "nil")
    _expect.expect(3, max, "number", "nil")
    min = min or -math.huge
    max = max or math.huge
    if num < min or num > max then
        error(("number outside of range (expected %d to be within %d and %d")
            :format(num, min, max), 2)
    end
end

setmetatable(_expect, { __call = function(_, ...)
    return _expect.expect(...)
end })

return _expect
