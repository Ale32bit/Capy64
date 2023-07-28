local fs = require("fs")

local date = os.date("*t")

if date.month == 4 and date.day == 28 then
    print("Ed Balls")
    return
end

local motdList = {}

local f<close> = fs.open("/sys/share/motd.txt", "r")
for line in f:lines() do
    table.insert(motdList, line)
end
f:close()

local motdIndex = math.random(1, #motdList)

print(motdList[motdIndex])