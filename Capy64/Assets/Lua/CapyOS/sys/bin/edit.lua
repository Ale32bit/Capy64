-- Get file to edit
local fs = require("fs")
local event = require("event")
local colours = require("colors")
local term = require("term")
local tArgs = { ... }
if #tArgs == 0 then
    print( "Usage: edit <path>" )
    return
end

-- Error checking
local sPath = shell.resolve( tArgs[1] )
local bReadOnly = true

local x,y = 1,1
local w,h = term.getSize()
local scrollX, scrollY = 0,0

local tLines = {}
local bRunning = true

-- Colours
local highlightColour, keywordColour, commentColour, textColour, bgColour, stringColour
if true then
    bgColour = colours.black
    textColour = colours.white
    highlightColour = colours.yellow
    keywordColour = colours.yellow
    commentColour = colours.green
    stringColour = colours.red
else
    bgColour = colours.black
    textColour = colours.white
    highlightColour = colours.white
    keywordColour = colours.white
    commentColour = colours.white
    stringColour = colours.white
end

-- Menus
local bMenu = false
local nMenuItem = 1
local tMenuItems = {}
if not bReadOnly then
    table.insert( tMenuItems, "Save" )
end
table.insert( tMenuItems, "Exit" )

local sStatus = "Press Ctrl to access menu"
if string.len( sStatus ) > w - 5 then
    sStatus = "Press Ctrl for menu"
end

local function load( _sPath )
    tLines = {}
    if fs.exists( _sPath ) then
        local file = io.open( _sPath, "r" )
        local sLine = file:read()
        while sLine do
            table.insert( tLines, sLine )
            sLine = file:read()
        end
        file:close()
    end
    
    if #tLines == 0 then
        table.insert( tLines, "" )
    end
end

local function save( _sPath )
    -- Create intervening folder
    local sDir = _sPath:sub(1, _sPath:len() - fs.getName(_sPath):len() )
    if not fs.exists( sDir ) then
        fs.makeDir( sDir )
    end

    -- Save
    local file = nil
    local function innerSave()
        file = fs.open( _sPath, "w" )
        if file then
            for n, sLine in ipairs( tLines ) do
                file.write( sLine .. "\n" )
            end
        else
            error( "Failed to open ".._sPath )
        end
    end
    
    local ok, err = pcall( innerSave )
    if file then 
        file.close()
    end
    return ok, err
end

local tKeywords = {
    ["and"] = true,
    ["break"] = true,
    ["do"] = true,
    ["else"] = true,
    ["elseif"] = true,
    ["end"] = true,
    ["false"] = true,
    ["for"] = true,
    ["function"] = true,
    ["if"] = true,
    ["in"] = true,
    ["local"] = true,
    ["nil"] = true,
    ["not"] = true,
    ["or"] = true,
    ["repeat"] = true,
    ["return"] = true,
    ["then"] = true,
    ["true"] = true,
    ["until"]= true,
    ["while"] = true,
}

local function tryWrite( sLine, regex, colour )
    local match = string.match( sLine, regex )
    if match then
        if type(colour) == "number" then
            term.setForeground( colour )
        else
            term.setForeground( colour(match) )
        end
        term.write( match )
        term.setForeground( textColour )
        return string.sub( sLine, string.len(match) + 1 )
    end
    return nil
end

local function writeHighlighted( sLine )
    while string.len(sLine) > 0 do    
        sLine = 
            tryWrite( sLine, "^%-%-%[%[.-%]%]", commentColour ) or
            tryWrite( sLine, "^%-%-.*", commentColour ) or
            tryWrite( sLine, "^\"\"", stringColour ) or
            tryWrite( sLine, "^\".-[^\\]\"", stringColour ) or
            tryWrite( sLine, "^\'\'", stringColour ) or
            tryWrite( sLine, "^\'.-[^\\]\'", stringColour ) or
            tryWrite( sLine, "^%[%[.-%]%]", stringColour ) or
            tryWrite( sLine, "^[%w_]+", function( match )
                if tKeywords[ match ] then
                    return keywordColour
                end
                return textColour
            end ) or
            tryWrite( sLine, "^[^%w_]", textColour )
    end
end

local tCompletions
local nCompletion

local tCompleteEnv = _ENV
local function complete( sLine )
    if settings.get( "edit.autocomplete" ) then
        local nStartPos = string.find( sLine, "[a-zA-Z0-9_%.:]+$" )
        if nStartPos then
            sLine = string.sub( sLine, nStartPos )
        end
        if #sLine > 0 then
            return textutils.complete( sLine, tCompleteEnv )
        end
    end
    return nil
end

local function recomplete()
    local sLine = tLines[y]
    if not bMenu and not bReadOnly and x == string.len(sLine) + 1 then
        tCompletions = complete( sLine )
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

local function writeCompletion( sLine )
    if nCompletion then
        local sCompletion = tCompletions[ nCompletion ]
        term.setTextColor( colours.white )
        term.setBackground( colours.grey )
        term.write( sCompletion )
        term.setTextColor( textColour )
        term.setBackground( bgColour )
    end
end

local function redrawText()
    local cursorX, cursorY = x, y
    for y=1,h-1 do
        term.setPos( 1 - scrollX, y )
        term.clearLine()

        local sLine = tLines[ y + scrollY ]
        if sLine ~= nil then
            writeHighlighted( sLine )
            if cursorY == y and cursorX == #sLine + 1 then
                writeCompletion()
            end
        end
    end
    term.setPos( x - scrollX, y - scrollY )
end

local function redrawLine(_nY)
    local sLine = tLines[_nY]
    if sLine then
        term.setPos( 1 - scrollX, _nY - scrollY )
        term.clearLine()
        writeHighlighted( sLine )
        if _nY == y and x == #sLine + 1 then
            writeCompletion()
        end
        term.setPos( x - scrollX, _nY - scrollY )
    end
end

local function redrawMenu()
    -- Clear line
    term.setPos( 1, h )
    term.clearLine()

    -- Draw line numbers
    term.setPos( w - string.len( "Ln "..y ) + 1, h )
    term.setForeground( highlightColour )
    term.write( "Ln " )
    term.setForeground( textColour )
    term.write( y )

    term.setPos( 1, h )
    if bMenu then
        -- Draw menu
        term.setForeground( textColour )
        for nItem,sItem in pairs( tMenuItems ) do
            if nItem == nMenuItem then
                term.setForeground( highlightColour )
                term.write( "[" )
                term.setForeground( textColour )
                term.write( sItem )
                term.setForeground( highlightColour )
                term.write( "]" )
                term.setForeground( textColour )
            else
                term.write( " "..sItem.." " )
            end
        end
    else
        -- Draw status
        term.setForeground( highlightColour )
        term.write( sStatus )
        term.setForeground( textColour )
    end

    -- Reset cursor
    term.setPos( x - scrollX, y - scrollY )
end

local tMenuFuncs = { 
    Save = function()
        if bReadOnly then
            sStatus = "Access denied"
        else
            local ok, err = save( sPath )
            if ok then
                sStatus="Saved to "..sPath
            else
                sStatus="Error saving to "..sPath
            end
        end
        redrawMenu()
    end,
    Exit = function()
        bRunning = false
    end
}

local function doMenuItem( _n )
    tMenuFuncs[tMenuItems[_n]]()
    if bMenu then
        bMenu = false
        term.setBlink( true )
    end
    redrawMenu()
end

local function setCursor( newX, newY )
    local oldX, oldY = x, y
    x, y = newX, newY
    local screenX = x - scrollX
    local screenY = y - scrollY
    
    local bRedraw = false
    if screenX < 1 then
        scrollX = x - 1
        screenX = 1
        bRedraw = true
    elseif screenX > w then
        scrollX = x - w
        screenX = w
        bRedraw = true
    end
    
    if screenY < 1 then
        scrollY = y - 1
        screenY = 1
        bRedraw = true
    elseif screenY > h-1 then
        scrollY = y - (h-1)
        screenY = h-1
        bRedraw = true
    end

    recomplete()
    if bRedraw then
        redrawText()
    elseif y ~= oldY then
        redrawLine( oldY )
        redrawLine( y )
    else
        redrawLine( y )
    end
    term.setPos( screenX, screenY )

    redrawMenu()
end

-- Actual program functionality begins
load(sPath)

term.setBackground( bgColour )
term.clear()
term.setPos(x,y)
term.setBlink( true )

recomplete()
redrawText()
redrawMenu()

local function acceptCompletion()
    if nCompletion then
        -- Append the completion
        local sCompletion = tCompletions[ nCompletion ]
        tLines[y] = tLines[y] .. sCompletion
        setCursor( x + string.len( sCompletion ), y )
    end
end

-- Handle input
while bRunning do
    local sEvent, param, param2, param3 = event.pull()
    if sEvent == "key" then
        local oldX, oldY = x, y
        if param == keys.up then
            -- Up
            if not bMenu then
                if nCompletion then
                    -- Cycle completions
                    nCompletion = nCompletion - 1
                    if nCompletion < 1 then
                        nCompletion = #tCompletions
                    end
                    redrawLine(y)

                elseif y > 1 then
                    -- Move cursor up
                    setCursor(
                        math.min( x, string.len( tLines[y - 1] ) + 1 ),
                        y - 1
                    )
                end
            end

        elseif param == keys.down then
            -- Down
            if not bMenu then
                -- Move cursor down
                if nCompletion then
                    -- Cycle completions
                    nCompletion = nCompletion + 1
                    if nCompletion > #tCompletions then
                        nCompletion = 1
                    end
                    redrawLine(y)

                elseif y < #tLines then
                    -- Move cursor down
                    setCursor(
                        math.min( x, string.len( tLines[y + 1] ) + 1 ),
                        y + 1
                    )
                end
            end

        elseif param == keys.tab then
            -- Tab
            if not bMenu and not bReadOnly then
                if nCompletion and x == string.len(tLines[y]) + 1 then
                    -- Accept autocomplete
                    acceptCompletion()
                else
                    -- Indent line
                    local sLine = tLines[y]
                    tLines[y] = string.sub(sLine,1,x-1) .. "    " .. string.sub(sLine,x)
                    setCursor( x + 4, y )
                end
            end

        elseif param == keys.pageUp then
            -- Page Up
            if not bMenu then
                -- Move up a page
                local newY
                if y - (h - 1) >= 1 then
                    newY = y - (h - 1)
                else
                    newY = 1
                end
                setCursor(
                    math.min( x, string.len( tLines[newY] ) + 1 ),
                    newY
                )
            end

        elseif param == keys.pageDown then
            -- Page Down
            if not bMenu then
                -- Move down a page
                local newY
                if y + (h - 1) <= #tLines then
                    newY = y + (h - 1)
                else
                    newY = #tLines
                end
                local newX = math.min( x, string.len( tLines[newY] ) + 1 )
                setCursor( newX, newY )
            end

        elseif param == keys.home then
            -- Home
            if not bMenu then
                -- Move cursor to the beginning
                if x > 1 then
                    setCursor(1,y)
                end
            end

        elseif param == keys["end"] then
            -- End
            if not bMenu then
                -- Move cursor to the end
                local nLimit = string.len( tLines[y] ) + 1
                if x < nLimit then
                    setCursor( nLimit, y )
                end
            end

        elseif param == keys.left then
            -- Left
            if not bMenu then
                if x > 1 then
                    -- Move cursor left
                    setCursor( x - 1, y )
                elseif x==1 and y>1 then
                    setCursor( string.len( tLines[y-1] ) + 1, y - 1 )
                end
            else
                -- Move menu left
                nMenuItem = nMenuItem - 1
                if nMenuItem < 1 then
                    nMenuItem = #tMenuItems
                end
                redrawMenu()
            end

        elseif param == keys.right then
            -- Right
            if not bMenu then
                local nLimit = string.len( tLines[y] ) + 1
                if x < nLimit then
                    -- Move cursor right
                    setCursor( x + 1, y )
                elseif nCompletion and x == string.len(tLines[y]) + 1 then
                    -- Accept autocomplete
                    acceptCompletion()
                elseif x==nLimit and y<#tLines then
                    -- Go to next line
                    setCursor( 1, y + 1 )
                end
            else
                -- Move menu right
                nMenuItem = nMenuItem + 1
                if nMenuItem > #tMenuItems then
                    nMenuItem = 1
                end
                redrawMenu()
            end

        elseif param == keys.delete then
            -- Delete
            if not bMenu and not bReadOnly then
                local nLimit = string.len( tLines[y] ) + 1
                if x < nLimit then
                    local sLine = tLines[y]
                    tLines[y] = string.sub(sLine,1,x-1) .. string.sub(sLine,x+1)
                    recomplete()
                    redrawLine(y)
                elseif y<#tLines then
                    tLines[y] = tLines[y] .. tLines[y+1]
                    table.remove( tLines, y+1 )
                    recomplete()
                    redrawText()
                end
            end

        elseif param == keys.backspace then
            -- Backspace
            if not bMenu and not bReadOnly then
                if x > 1 then
                    -- Remove character
                    local sLine = tLines[y]
                    if x > 4 and string.sub(sLine,x-4,x-1) == "    " and not string.sub(sLine, 1, x - 1):find("%S") then
                        tLines[y] = string.sub(sLine,1,x-5) .. string.sub(sLine,x)
                        setCursor( x - 4, y )
                    else
                        tLines[y] = string.sub(sLine,1,x-2) .. string.sub(sLine,x)
                        setCursor( x - 1, y )
                    end
                elseif y > 1 then
                    -- Remove newline
                    local sPrevLen = string.len( tLines[y-1] )
                    tLines[y-1] = tLines[y-1] .. tLines[y]
                    table.remove( tLines, y )
                    setCursor( sPrevLen + 1, y - 1 )
                    redrawText()
                end
            end

        elseif param == keys.enter then
            -- Enter
            if not bMenu and not bReadOnly then
                -- Newline
                local sLine = tLines[y]
                local _,spaces=string.find(sLine,"^[ ]+")
                if not spaces then
                    spaces=0
                end
                tLines[y] = string.sub(sLine,1,x-1)
                table.insert( tLines, y+1, string.rep(' ',spaces)..string.sub(sLine,x) )
                setCursor( spaces + 1, y + 1 )
                redrawText()

            elseif bMenu then
                -- Menu selection
                doMenuItem( nMenuItem )

            end

        elseif param == keys.leftCtrl or param == keys.rightCtrl or param == keys.rightAlt then
            -- Menu toggle
            bMenu = not bMenu
            if bMenu then
                term.setBlink( false )
            else
                term.setBlink( true )
            end
            redrawMenu()

        end
        
    elseif sEvent == "char" then
        if not bMenu and not bReadOnly then
            -- Input text
            local sLine = tLines[y]
            tLines[y] = string.sub(sLine,1,x-1) .. param .. string.sub(sLine,x)
            setCursor( x + 1, y )

        elseif bMenu then
            -- Select menu items
            for n,sMenuItem in ipairs( tMenuItems ) do
                if string.lower(string.sub(sMenuItem,1,1)) == string.lower(param) then
                    doMenuItem( n )
                    break
                end
            end
        end

    elseif sEvent == "paste" then
        if not bReadOnly then
            -- Close menu if open
            if bMenu then
                bMenu = false
                term.setBlink( true )
                redrawMenu()
            end
            -- Input text
            local sLine = tLines[y]
            tLines[y] = string.sub(sLine,1,x-1) .. param .. string.sub(sLine,x)
            setCursor( x + string.len( param ), y )
        end
        
    elseif sEvent == "mouse_click" then
        if not bMenu then
            if param == 1 then
                -- Left click
                local cx,cy = param2, param3
                if cy < h then
                    local newY = math.min( math.max( scrollY + cy, 1 ), #tLines )
                    local newX = math.min( math.max( scrollX + cx, 1 ), string.len( tLines[newY] ) + 1 )
                    setCursor( newX, newY )
                end
            end
        end
        
    elseif sEvent == "mouse_scroll" then
        if not bMenu then
            if param == -1 then
                -- Scroll up
                if scrollY > 0 then
                    -- Move cursor up
                    scrollY = scrollY - 1
                    redrawText()
                end
            
            elseif param == 1 then
                -- Scroll down
                local nMaxScroll = #tLines - (h-1)
                if scrollY < nMaxScroll then
                    -- Move cursor down
                    scrollY = scrollY + 1
                    redrawText()
                end
                
            end
        end

    elseif sEvent == "term_resize" then
        w,h = term.getSize()
        setCursor( x, y )
        redrawMenu()
        redrawText()

    end
end

-- Cleanup
term.clear()
term.setBlink( false )
term.setPos( 1, 1 )
