local expect = require("expect").expect
local tableutils = {}

local function serialize(data, circular)
    expect(1, data, "string", "table", "number", "boolean", "nil")
    if type(data) == "table" then
        if not circular then
            circular = {}
        end
        local output = "{"
        for k, v in pairs(data) do
            if type(v) == "table" then
                local name = tostring(v)
                if circular[name] then
                    error("circular reference in table", 2)
                end
                circular[name] = true
            end
            output = output .. string.format("[%q] = %s,", k, serialize(v, circular))
        end
        output = output .. "}"
        return output
    else
        return string.format("%q", data)
    end
end

function tableutils.serialize(data)
    expect(1, data, "string", "table", "number", "boolean", "nil")
    return serialize(data)
end

function tableutils.deserialize(data)
    local func, err = load("return " .. data, "=tableutils", "t", {})
    if not func then
        error(err, 2)
    end
    return func()
end

local function prettyvalue(value)
    if type(value) == "table" or type(value) == "function" or type(value) == "thread" or type(value) == "userdata" or type(value) == "number" then
        return tostring(value)
    else
        return string.format("%q", value)
    end
end

function tableutils.pretty(data)
    if type(data) == "table" then
        local output = "{"

        local index = 0
        for k, v in pairs(data) do
            local value = prettyvalue(v)
            
            if type(k) == "number" and k - 1 == index then
                index = index + 1
                output = output .. string.format("\n  %s,", value)
            elseif type(k) == "string" and k:match("^[%a_][%w_]*$") then
                output = output .. string.format("\n  %s = %s,", k, value)
            else
                output = output .. string.format("\n  [%s] = %s,", prettyvalue(k), value)
            end
        end
        if output == "{" then
            return "{}"
        end
        output = output .. "\n}"

        return output
    else
        return prettyvalue(data)
    end
end

function tableutils.find(tbl, element)
    for i = 1, #tbl do
        if tbl[i] == element then
            return i
        end
    end

    return nil
end

return tableutils