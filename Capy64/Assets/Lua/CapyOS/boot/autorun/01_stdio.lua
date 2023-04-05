function _G.print(...)
    local args = { ... }
    local size = #args
    local lines = 0
    for n, v in ipairs(args) do
        local s = tostring(v)
        if n < size then
            s = s .. "\t"
        end
        lines = lines + io.write(s)
    end
    lines = lines + io.write("\n")

    return lines
end
