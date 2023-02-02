local term = require("term")
local timer = require("timer")
local gpu = require("gpu")
local fs = require("fs")
local machine = require("machine")

local bootSleep = 2000
local bg = 0x0
local fg = 0xffffff

term.setForeground(fg)
term.setBackground(bg)
term.clear()

term.setSize(51, 19)
gpu.setScale(2)

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
	if not fs.exists("/boot/vendor.bmp") then
		return
	end

	local w, h = gpu.getSize()
	local ok, err = pcall(function()
		local buffer<close>, width, height = gpu.loadImage("/boot/vendor.bmp")

		local x, y = 
			math.ceil((w / 2) - (width / 2)),
			math.ceil((h / 2) - (height / 2))

		gpu.drawBuffer(buffer, x, y, width, height)
	end)

	if not ok then
		print("Warning: Could not draw vendor.bmp", err)
	end
end

local w, h = term.getSize()

term.setBlink(false)

local function setupScreen()
	local options = {
		{
			"Open data folder",
			openDataFolder,
		},
		{
			"Install default OS",
			installOS,
		},
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
	local function redraw()
		term.setForeground(fg)
		term.setBackground(bg)
		term.clear()
		term.setPos(1,2)
		writeCenter("Capy64 Setup")

		term.setPos(1,3)

		for k, v in ipairs(options) do
			local _, y = term.getPos()
			term.setPos(1, y + 1)
			term.clearLine()

			if selection == k then
				writeCenter("[ " .. v[1] .. " ]")
			else
				writeCenter(v[1])
			end
		end
	end

	while true do
		redraw()
		local ev = { coroutine.yield("key_down") }
		if ev[3] == "up" then
			selection = selection - 1
		elseif ev[3] == "down" then
			selection = selection + 1
		elseif ev[3] == "enter" then
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
	writeCenter("Powered by Capybaras")

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

bootScreen()

term.clear()