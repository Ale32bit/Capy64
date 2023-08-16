-- This file is part of Capy64 - https://github.com/Capy64/Capy64
-- Copyright 2023 Alessandro "AlexDevs" Proto
--
-- Licensed under the Apache License, Version 2.0 (the "License").
-- you may not use this file except in compliance with the License.
-- You may obtain a copy of the License at
--
--     http://www.apache.org/licenses/LICENSE-2.0
--
-- Unless required by applicable law or agreed to in writing, software
-- distributed under the License is distributed on an "AS IS" BASIS,
-- WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
-- See the License for the specific language governing permissions and
-- limitations under the License.

local term = require("term")
local timer = require("timer")
local gpu = require("gpu")
local fs = require("fs")
local machine = require("machine")
local audio = require("audio")
local event = require("event")

local bootSleep = 2
local bg = 0x0
local fg = 0xffffff
local setupbg = 0x0608a6
local setupfg = 0xffffff
local accent = 0xffea00

term.setForeground(fg)
term.setBackground(bg)
term.clear()

if term.isResizable() then
	term.setSize(53, 20)
end

local w, h = term.getSize()

local function sleep(n)
	local timerId = timer.start(n)
	repeat
		local ev, par = coroutine.yield("timer")
	until par == timerId
end

local function writeCenter(text)
	local w, h = term.getSize()
	local _, y = term.getPos()
	term.setPos(
		(1 + w / 2) - (#text / 2),
		y
	)
	term.write(text)
end

local function drawVendorImage()
	if not fs.exists("/sys/vendor.bmp") then
		return
	end

	local w, h = gpu.getSize()
	local ok, err = pcall(function()
		local task<close> = gpu.loadImageAsync("/sys/vendor.bmp")
		local buffer<close> = task:await()

		local x, y = 
			math.ceil((w / 2) - (buffer.width / 2)),
			math.ceil((h / 2) - (buffer.height / 2))

		gpu.drawBuffer(buffer, x, y)
	end)

	if not ok then
		print("Warning: Could not draw vendor.bmp", err)
	end
end

local nPrint = print
local function print( text )
	local x, y = term.getPos()
	term.write(tostring(text))
	if y == h then
		term.scroll(1)
		term.setPos(1, y)
	else
		term.setPos(1, y + 1)
	end
end

local function printError( text )
	term.setForeground(0xff0000)
	print(text)
	term.setForeground(0xffffff)
end

local function promptKey()
	print("Press any key to continue")
	event.pull("key_down")
end

local function alert(...)
	local args = {...}
	table.insert(args, "[ OK ]")
	local lines = {}
	local width = 0
	local padding = 1
	for k, v in ipairs(args) do
		lines[k] = tostring(v)
		width = math.max(width, #tostring(v))
	end

	lines[#lines] = nil

	local okPad = string.rep(" ", (width - #args[#args]) // 2)
	local okLine = string.format("%s%s", okPad, args[#args])

	local w, h = term.getSize()
	local cx, cy = w//2, h//2
	local dx, dy = cx - (width + padding * 2) // 2, cy - #lines//2 - 1

	local pad = string.rep(" ", padding)
	local emptyLine = string.format("\u{258C}%s%s%s\u{2590}", pad, string.rep(" ", width), pad)

	term.setPos(dx, dy)
	print("\u{259B}" .. string.rep("\u{2580}", width + padding * 2) .. "\u{259C}")
	for k, v in ipairs(lines) do
		term.setPos(dx, dy + k)
		local space = string.rep(" ", width - #v)
		print(string.format("\u{258C}%s%s%s%s\u{2590}", pad, v, space, pad))
	end
	term.setPos(dx, dy + #lines + 1)
	print(emptyLine)
	term.setPos(dx, dy + #lines + 2)
	local space = string.rep(" ", width - #okLine)
	term.write(string.format("\u{258C}%s", pad))
	term.setForeground(accent)
	term.write(okLine)
	term.setForeground(setupfg)
	print(string.format("%s%s\u{2590}", space, pad))
	term.setPos(dx, dy + #lines + 3)
	print("\u{2599}" .. string.rep("\u{2584}", width + padding * 2) .. "\u{259F}")

	local _, key, keyname
	repeat
		_, key, keyname = event.pull("key_down")
	until keyname == "enter" or keyname == "escape"
end

local function installDefaultOS()
	if fs.exists("/sys") then
		fs.delete("/sys", true)
	end
	installOS()
	alert("Default OS installed!")
end

term.setBlink(false)

local function setupScreen()
	local options = {
		{
			"Open data folder",
			openDataFolder,
		},
		{
			"Install default OS",
			installDefaultOS,
		},
		{},
		{
			"Exit setup",
			exit,
		},
		{
			"Shutdown",
			machine.shutdown,
		}
	}

	local selection = 1
	local function redraw(noDrawSel)
		local w, h = term.getSize()
		term.setForeground(setupfg)
		term.setBackground(setupbg)
		term.clear()
		term.setPos(1,2)
		writeCenter("Capy64 Setup")

		term.setPos(1,3)

		term.setForeground(accent)

		for k, v in ipairs(options) do
			local _, y = term.getPos()
			term.setPos(1, y + 1)
			term.clearLine()

			if #v > 0 then
				if selection == k and not noDrawSel then
					writeCenter("[ " .. v[1] .. " ]")
				else
					writeCenter(v[1])
				end
			end
		end

		term.setForeground(setupfg)

		term.setPos(1, h - 2)
		term.write("\u{2190}\u{2191}\u{2192}\u{2193}: Move")
		term.setPos(1, h - 1)
		term.write("Escape: Exit")
		term.setPos(1, h)
		term.write("Enter: Select")
	end

	while true do
		redraw()
		local ev = { coroutine.yield("key_down") }
		if ev[3] == "up" then
			selection = selection - 1
			if options[selection] and #options[selection] == 0 then
				selection = selection - 1
			end
		elseif ev[3] == "down" then
			selection = selection + 1
			if options[selection] and #options[selection] == 0 then
				selection = selection + 1
			end
		elseif ev[3] == "enter" then
			redraw(true)
			options[selection][2]()
		elseif ev[3] == "escape" then
			exit()
		end

		if selection > #options then
			selection = 1
		elseif selection < 1 then
			selection = #options
		end
	end

end

local function bootScreen()
	drawVendorImage()

	term.setPos(1,2)
	writeCenter("Capy64")
	term.setPos(1,4)
	writeCenter("(c) 2023 AlexDevs")

	term.setPos(1, h - 1)
	writeCenter("Press F2 to open setup")

	local timerId = timer.start(bootSleep)
	while true do
		local ev = {coroutine.yield("timer", "key_down")}
		if ev[1] == "timer" and ev[2] == timerId then
			exit()
		elseif ev[1] == "key_down" and ev[3] == "f2" then
			setupScreen()
			break
		end
	end
end

audio.beep(1000, 0.2, 0.2, "square")

bootScreen()

term.clear()
