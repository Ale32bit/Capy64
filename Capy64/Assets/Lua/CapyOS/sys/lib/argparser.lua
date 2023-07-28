local argparser = {}

function argparser.tokenize(...)
    local input = table.concat(table.pack(...), " ")
    local tokens = {}

    -- surely there must be a better way
    local quoted = false
    local escaped = false
    local current = ""
    for i = 1, #input do
        local char = input:sub(i, i)
        if escaped then
            escaped = false
            current = current .. char
        else
            if char == "\\" then
                escaped = true
            elseif char == "\"" then
                if quoted then
                    -- close quote
                    table.insert(tokens, current)
                    current = ""
                end
                quoted = not quoted
            elseif char == " " and not quoted then
                if #current > 0 then
                    table.insert(tokens, current)
                end
                current = ""
            else
                current = current .. char
            end
        end
    end

    if current ~= "" then
        table.insert(tokens, current)
    end

    return tokens
end

function argparser.parse(...)
    local tokens = { ... }
    local args = {}
    local options = {}
    local ignoreOptions = false

    for i = 1, #tokens do
        local token = tokens[i]
        if not ignoreOptions then
            if token == "--" then
                ignoreOptions = true
            elseif token:sub(1, 2) == "--" then
                local opt, value = token:match("%-%-(.+)=(.+)")
                if not opt then
                    opt = token:sub(3)
                    if opt:sub(-1) == "=" then
                        -- next token is value
                        value = tokens[i + 1]
                        opt = opt:sub(1, -2)
                        options[opt] = value
                        i = i + 1
                    else
                        options[opt] = true
                    end
                else
                    options[opt] = value
                end
            elseif token:sub(1, 1) == "-" then
                local opts = token:sub(2)
                for j = 1, #opts do
                    options[opts:sub(j, j)] = true
                end
            else
                if #token > 0 then
                    table.insert(args, token)
                end
            end
        else
            if #token > 0 then
                table.insert(args, token)
            end
        end
    end

    return args, options
end

return argparser
