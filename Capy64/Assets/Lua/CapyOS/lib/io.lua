local expect = require("expect").expect
local event = require("event")
local term = require("term")
local keys = require("keys")
local machine = require("machine")

local io = {}

function io.write(text)
    text = tostring(text)

    local lines = 0
    local w, h = term.getSize()

    local function inc_cy(cy)
        lines = lines + 1

        if cy > h - 1 then
            term.scroll(1)
            return cy
        else
            return cy + 1
        end
    end

    while #text > 0 do
        local nl = text:find("\n") or #text
        local chunk = text:sub(1, nl)
        text = text:sub(#chunk + 1)

        local has_nl = chunk:sub( -1) == "\n"
        if has_nl then chunk = chunk:sub(1, -2) end

        local cx, cy = term.getPos()
        while #chunk > 0 do
            if cx > w then
                term.setPos(1, inc_cy(cy))
                cx, cy = term.getPos()
            end

            local to_write = chunk:sub(1, w - cx + 1)
            term.write(to_write)

            chunk = chunk:sub(#to_write + 1)
            cx, cy = term.getPos()
        end

        if has_nl then
            term.setPos(1, inc_cy(cy))
        end
    end

    return lines
end

local empty = {}
function io.read(replace, history, complete, default)
    expect(1, replace, "string", "nil")
    expect(2, history, "table", "nil")
    expect(3, complete, "function", "nil")
    expect(4, default, "string", "nil")

    if replace then replace = replace:sub(1, 1) end
    local hist = history or {}
    history = {}
    for i = 1, #hist, 1 do
        history[i] = hist[i]
    end

    local buffer = default or ""
    local prev_buf = buffer
    history[#history + 1] = buffer

    local hist_pos = #history
    local cursor_pos = 0

    local stx, sty = term.getPos()
    local w, h = term.getSize()

    local dirty = false
    local completions = {}
    local comp_id = 0

    local function clearCompletion()
        if completions[comp_id] then
            write((" "):rep(#completions[comp_id]))
        end
    end

    local function full_redraw(force)
        if force or dirty then
            if complete and buffer ~= prev_buf then
                completions = complete(buffer) or empty
                comp_id = math.min(1, #completions)
            end
            prev_buf = buffer

            term.setPos(stx, sty)
            local text = buffer
            if replace then text = replace:rep(#text) end
            local ln = io.write(text)

            if completions[comp_id] then
                local oldfg = term.getForeground()
                local oldbg = term.getBackground()
                term.setForeground(colors.white)
                term.setBackground(colors.gray)
                ln = ln + write(completions[comp_id])
                term.setForeground(oldfg)
                term.setBackground(oldbg)
            else
                ln = ln + io.write(" ")
            end

            if sty + ln > h then
                sty = sty - (sty + ln - h)
            end
        end

        -- set cursor to the appropriate spot
        local cx, cy = stx, sty
        cx = cx + #buffer - cursor_pos -- + #(completions[comp_id] or "")
        while cx > w do
            cx = cx - w
            cy = cy + 1
        end
        term.setPos(cx, cy)
    end

    term.setBlink(true)

    while true do
        full_redraw()
        -- get input
        local evt, par1, par2, mods = event.pull()

        if evt == "char" then
            dirty = true
            clearCompletion()
            if cursor_pos == 0 then
                buffer = buffer .. par1
            elseif cursor_pos == #buffer then
                buffer = par1 .. buffer
            else
                buffer = buffer:sub(0, -cursor_pos - 1) .. par1 .. buffer:sub( -cursor_pos)
            end
        elseif evt == "key_down" then
            if par1 == keys.back and #buffer > 0 then
                dirty = true
                if cursor_pos == 0 then
                    buffer = buffer:sub(1, -2)
                    clearCompletion()
                elseif cursor_pos < #buffer then
                    buffer = buffer:sub(0, -cursor_pos - 2) .. buffer:sub( -cursor_pos)
                end
            elseif par1 == keys.delete and cursor_pos > 0 then
                dirty = true

                if cursor_pos == #buffer then
                    buffer = buffer:sub(2)
                elseif cursor_pos == 1 then
                    buffer = buffer:sub(1, -2)
                else
                    buffer = buffer:sub(0, -cursor_pos - 1) .. buffer:sub( -cursor_pos + 1)
                end
                cursor_pos = cursor_pos - 1
            elseif par1 == keys.up then
                if #completions > 1 then
                    dirty = true
                    clearCompletion()
                    if comp_id > 1 then
                        comp_id = comp_id - 1
                    else
                        comp_id = #completions
                    end
                elseif hist_pos > 1 then
                    cursor_pos = 0

                    history[hist_pos] = buffer
                    hist_pos = hist_pos - 1

                    buffer = (" "):rep(#buffer)
                    full_redraw(true)

                    buffer = history[hist_pos]
                    dirty = true
                end
            elseif par1 == keys.down then
                if #completions > 1 then
                    dirty = true
                    clearCompletion()
                    if comp_id < #completions then
                        comp_id = comp_id + 1
                    else
                        comp_id = 1
                    end
                elseif hist_pos < #history then
                    cursor_pos = 0

                    history[hist_pos] = buffer
                    hist_pos = hist_pos + 1

                    buffer = (" "):rep(#buffer)
                    full_redraw(true)

                    buffer = history[hist_pos]
                    dirty = true
                end
            elseif par1 == keys.left then
                if cursor_pos < #buffer then
                    clearCompletion()
                    cursor_pos = cursor_pos + 1
                end
            elseif par1 == keys.right then
                if cursor_pos > 0 then
                    cursor_pos = cursor_pos - 1
                elseif comp_id > 0 then
                    dirty = true
                    buffer = buffer .. completions[comp_id]
                end
            elseif par1 == keys.tab then
                if comp_id > 0 then
                    dirty = true
                    buffer = buffer .. completions[comp_id]
                end
            elseif par1 == keys.home then
                cursor_pos = #buffer
            elseif par1 == keys["end"] then
                cursor_pos = 0
            elseif par1 == keys.enter then
                clearCompletion()
                print()
                break
            elseif mods & keys.mods.ctrl ~= 0 then
                if par1 == keys.v then
                    dirty = true
                    clearCompletion()
                    local text = machine.getClipboard()
                    if text then
                        if cursor_pos == 0 then
                            buffer = buffer .. text
                        elseif cursor_pos == #buffer then
                            buffer = text .. buffer
                        else
                            buffer = buffer:sub(0, -cursor_pos - 1) .. text ..
                                buffer:sub( -cursor_pos + (#text - 1))
                        end
                    end
                end
            end
        end
    end

    term.setBlink(false)

    return buffer
end

io.stderr = {}

function io.stderr.write(text)
    local fg = term.getForeground()
    term.setForeground(0xff0000)
    io.write(text)
    term.setForeground(fg)
end

function io.stderr.print(...)
    local fg = term.getForeground()
    term.setForeground(0xff0000)
    print(...)
    term.setForeground(fg)
end

return io
