local expect = require("expect").expect
local event = require("event")
local term = require("term")
local keys = require("keys")
local machine = require("machine")

local io = {}

function io.write(sText)
    sText = tostring(sText)

    local w, h = term.getSize()
    local x, y = term.getPos()

    local nLinesPrinted = 0
    local function newLine()
        if y + 1 <= h then
            term.setPos(1, y + 1)
        else
            term.setPos(1, h)
            term.scroll(1)
        end
        x, y = term.getPos()
        nLinesPrinted = nLinesPrinted + 1
    end

    -- Print the line with proper word wrapping
    sText = tostring(sText)
    while #sText > 0 do
        local whitespace = string.match(sText, "^[ \t]+")
        if whitespace then
            -- Print whitespace
            term.write(whitespace)
            x, y = term.getPos()
            sText = string.sub(sText, #whitespace + 1)
        end

        local newline = string.match(sText, "^\n")
        if newline then
            -- Print newlines
            newLine()
            sText = string.sub(sText, 2)
        end

        local text = string.match(sText, "^[^ \t\n]+")
        if text then
            sText = string.sub(sText, #text + 1)
            if #text > w then
                -- Print a multiline word
                while #text > 0 do
                    if x > w then
                        newLine()
                    end
                    term.write(text)
                    text = string.sub(text, w - x + 2)
                    x, y = term.getPos()
                end
            else
                -- Print a word normally
                if x + #text - 1 > w then
                    newLine()
                end
                term.write(text)
                x, y = term.getPos()
            end
        end
    end

    return nLinesPrinted
end

function io.read(_sReplaceChar, _tHistory, _fnComplete, _sDefault)
    expect(1, _sReplaceChar, "string", "nil")
    expect(2, _tHistory, "table", "nil")
    expect(3, _fnComplete, "function", "nil")
    expect(4, _sDefault, "string", "nil")

    term.setBlink(true)

    local sLine
    if type(_sDefault) == "string" then
        sLine = _sDefault
    else
        sLine = ""
    end
    local nHistoryPos
    local nPos, nScroll = #sLine, 0
    if _sReplaceChar then
        _sReplaceChar = string.sub(_sReplaceChar, 1, 1)
    end

    local tCompletions
    local nCompletion
    local function recomplete()
        if _fnComplete and nPos == #sLine then
            tCompletions = _fnComplete(sLine)
            if tCompletions and #tCompletions > 0 then
                nCompletion = 1
            else
                nCompletion = nil
            end
        else
            tCompletions = nil
            nCompletion = nil
        end
    end

    local function uncomplete()
        tCompletions = nil
        nCompletion = nil
    end

    local w = term.getSize()
    local sx = term.getPos()

    local function redraw(_bClear)
        local cursor_pos = nPos - nScroll
        if sx + cursor_pos >= w then
            -- We've moved beyond the RHS, ensure we're on the edge.
            nScroll = sx + nPos - w
        elseif cursor_pos < 0 then
            -- We've moved beyond the LHS, ensure we're on the edge.
            nScroll = nPos
        end

        local _, cy = term.getPos()
        term.setPos(sx, cy)
        local sReplace = _bClear and " " or _sReplaceChar
        if sReplace then
            term.write(string.rep(sReplace, math.max(#sLine - nScroll, 0)))
        else
            term.write(string.sub(sLine, nScroll + 1))
        end

        if nCompletion then
            local sCompletion = tCompletions[nCompletion]
            local oldText, oldBg
            if not _bClear then
                oldText = term.getTextColor()
                oldBg = term.getBackgroundColor()
                term.setTextColor(colors.white)
                term.setBackgroundColor(colors.gray)
            end
            if sReplace then
                term.write(string.rep(sReplace, #sCompletion))
            else
                term.write(sCompletion)
            end
            if not _bClear then
                term.setTextColor(oldText)
                term.setBackgroundColor(oldBg)
            end
        end

        term.setPos(sx + nPos - nScroll, cy)
    end

    local function clear()
        redraw(true)
    end

    recomplete()
    redraw()

    local function acceptCompletion()
        if nCompletion then
            -- Clear
            clear()

            -- Find the common prefix of all the other suggestions which start with the same letter as the current one
            local sCompletion = tCompletions[nCompletion]
            sLine = sLine .. sCompletion
            nPos = #sLine

            -- Redraw
            recomplete()
            redraw()
        end
    end
    while true do
        local sEvent, param, param1, param2 = event.pull()
        if sEvent == "char" then
            -- Typed key
            clear()
            sLine = string.sub(sLine, 1, nPos) .. param .. string.sub(sLine, nPos + 1)
            nPos = nPos + 1
            recomplete()
            redraw()

        elseif sEvent == "paste" then
            -- Pasted text
            clear()
            sLine = string.sub(sLine, 1, nPos) .. param .. string.sub(sLine, nPos + 1)
            nPos = nPos + #param
            recomplete()
            redraw()

        elseif sEvent == "key_down" then
            if param == keys.enter or param == keys.numPadEnter then
                -- Enter/Numpad Enter
                if nCompletion then
                    clear()
                    uncomplete()
                    redraw()
                end
                break

            elseif param == keys.left then
                -- Left
                if nPos > 0 then
                    clear()
                    nPos = nPos - 1
                    recomplete()
                    redraw()
                end

            elseif param == keys.right then
                -- Right
                if nPos < #sLine then
                    -- Move right
                    clear()
                    nPos = nPos + 1
                    recomplete()
                    redraw()
                else
                    -- Accept autocomplete
                    acceptCompletion()
                end

            elseif param == keys.up or param == keys.down then
                -- Up or down
                if nCompletion then
                    -- Cycle completions
                    clear()
                    if param == keys.up then
                        nCompletion = nCompletion - 1
                        if nCompletion < 1 then
                            nCompletion = #tCompletions
                        end
                    elseif param == keys.down then
                        nCompletion = nCompletion + 1
                        if nCompletion > #tCompletions then
                            nCompletion = 1
                        end
                    end
                    redraw()

                elseif _tHistory then
                    -- Cycle history
                    clear()
                    if param == keys.up then
                        -- Up
                        if nHistoryPos == nil then
                            if #_tHistory > 0 then
                                nHistoryPos = #_tHistory
                            end
                        elseif nHistoryPos > 1 then
                            nHistoryPos = nHistoryPos - 1
                        end
                    else
                        -- Down
                        if nHistoryPos == #_tHistory then
                            nHistoryPos = nil
                        elseif nHistoryPos ~= nil then
                            nHistoryPos = nHistoryPos + 1
                        end
                    end
                    if nHistoryPos then
                        sLine = _tHistory[nHistoryPos]
                        nPos, nScroll = #sLine, 0
                    else
                        sLine = ""
                        nPos, nScroll = 0, 0
                    end
                    uncomplete()
                    redraw()

                end

            elseif param == keys.back then
                -- Backspace
                if nPos > 0 then
                    clear()
                    sLine = string.sub(sLine, 1, nPos - 1) .. string.sub(sLine, nPos + 1)
                    nPos = nPos - 1
                    if nScroll > 0 then nScroll = nScroll - 1 end
                    recomplete()
                    redraw()
                end

            elseif param == keys.home then
                -- Home
                if nPos > 0 then
                    clear()
                    nPos = 0
                    recomplete()
                    redraw()
                end

            elseif param == keys.delete then
                -- Delete
                if nPos < #sLine then
                    clear()
                    sLine = string.sub(sLine, 1, nPos) .. string.sub(sLine, nPos + 2)
                    recomplete()
                    redraw()
                end

            elseif param == keys["end"] then
                -- End
                if nPos < #sLine then
                    clear()
                    nPos = #sLine
                    recomplete()
                    redraw()
                end

            elseif param == keys.tab then
                -- Tab (accept autocomplete)
                acceptCompletion()

            end

        elseif sEvent == "mouse_down" or sEvent == "mouse_drag" and param == 1 then
            local _, cy = term.getPos()
            if param1 >= sx and param1 <= w and param2 == cy then
                -- Ensure we don't scroll beyond the current line
                nPos = math.min(math.max(nScroll + param1 - sx, 0), #sLine)
                redraw()
            end

        elseif sEvent == "term_resize" then
            -- Terminal resized
            w = term.getSize()
            redraw()

        end
    end

    local _, cy = term.getPos()
    term.setBlink(false)
    term.setPos(w + 1, cy)
    print()

    return sLine
end


--[[function io.write(text)
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

        local has_nl = chunk:sub(-1) == "\n"
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
            io.write((" "):rep(#completions[comp_id]))
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
                buffer = buffer:sub(0, -cursor_pos - 1) .. par1 .. buffer:sub(-cursor_pos)
            end
        elseif evt == "key_down" then
            if par1 == keys.back and #buffer > 0 then
                dirty = true
                if cursor_pos == 0 then
                    buffer = buffer:sub(1, -2)
                    clearCompletion()
                elseif cursor_pos < #buffer then
                    buffer = buffer:sub(0, -cursor_pos - 2) .. buffer:sub(-cursor_pos)
                end
            elseif par1 == keys.delete and cursor_pos > 0 then
                dirty = true

                if cursor_pos == #buffer then
                    buffer = buffer:sub(2)
                elseif cursor_pos == 1 then
                    buffer = buffer:sub(1, -2)
                else
                    buffer = buffer:sub(0, -cursor_pos - 1) .. buffer:sub(-cursor_pos + 1)
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
                                buffer:sub(-cursor_pos + (#text - 1))
                        end
                    end
                end
            end
        end
    end

    term.setBlink(false)

    return buffer
end
]]

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
